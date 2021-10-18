using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public abstract class Spirit : IOption {

		#region constructor

		public Spirit( SpiritPresence presence, params PowerCard[] initialCards ){
			Presence = presence;

			foreach(var card in initialCards)
				AddCardToHand(card);

			Action = new ActionGateway();
		}
		public ActionGateway Action { get; }

		public void AddCardToHand( PowerCard card ){
			Hand.Add(card);
		}

		#endregion

		#region Elements

		public readonly CountDictionary<Element> Elements = new CountDictionary<Element>();

		#endregion

		#region Growth

		public GrowthOption[] GrowthOptions => growthOptionGroup.Options;
		protected GrowthOptionGroup growthOptionGroup;

		// Called from unit tests:  Bringer, Lightning, Ocean, Rampant Green, Shadows, Thunderspeaker
		public void Grow( int optionIndex ) {
			Grow( GrowthOptions[optionIndex] );
		}

		// !!! TESTING
		public Task GrowAndResolve( int optionIndex, GameState gameState ) {
			return GrowAndResolve( GrowthOptions[optionIndex], gameState );
		}

		public Task GrowAndResolve( GrowthOption option, GameState gameState ) {
			if( option.AutoSelectSingle && option.GrowthActions.Length == 1 )
				return option.GrowthActions[0].ActivateAsync( this, gameState );
			else {
				Grow( option );
				return ResolveActions( gameState, Speed.Growth, Present.Always );
			}
		}

		public void Grow( GrowthOption option ) {
			foreach(GrowthActionFactory action in option.GrowthActions)
				AddActionFactory( action );
		}

		/// <remarks>override in a test.  Could be refactored to not need to be virtual</remarks>
		public virtual GrowthOptionGroup GetGrowthOptions() => growthOptionGroup;


		public virtual async Task DoGrowth(GameState gameState) {

			int count = growthOptionGroup.SelectionCount;
			List<GrowthOption> remainingOptions = growthOptionGroup.Options.ToList();

			while(count-- > 0) {
				var currentOptions = remainingOptions.Where( o => o.GainEnergy + Energy >= 0 ).ToArray();
				GrowthOption option = (GrowthOption)await this.Select( "Select Growth Option", currentOptions, Present.Always );
				remainingOptions.Remove( option );

				await GrowAndResolve( option, gameState );
			}

			await TriggerEnergyElementsAndReclaims();

		}

		public async Task ResolveActions( GameState gameState, Speed speed, Present present ) {

			IActionFactory[] factoryOptions;

			while((factoryOptions = this.GetAvailableActions( speed ).ToArray()).Length> 0) {

				// -------------
				// Select Actions to resolve
				// -------------
				IActionFactory option = await this.SelectFactory( "Select " + speed + " to resolve:", factoryOptions, present );
				if(option == null)
					break;

				// if user clicked a slow card that was made fast, // slow card won't be in the options
				if(!factoryOptions.Contains( option ))
					// find the fast version of the slow card that was clicked
					option = factoryOptions.Cast<IActionFactory>()
						.First( factory => factory == option );

				if(!factoryOptions.Contains( option ))
					throw new Exception( "Dude! - You selected something that wasn't an option" );

				await TakeAction( (IActionFactory)option, gameState );
			}

		}

		#endregion

		#region Cards

		public List<PowerCard> Hand = new List<PowerCard>();	// in hand
		public List<PowerCard> PurchasedCards = new List<PowerCard>();		// paid for

		// Cards are not transferred to discard pile until end of turn because we need to keep track of their elements.
		public List<PowerCard> DiscardPile = new List<PowerCard>();

		readonly List<IActionFactory> availableActions = new List<IActionFactory>();
		readonly List<IActionFactory> usedActions = new List<IActionFactory>();
		readonly List<InnatePower> usedInnates = new List<InnatePower>();

		public void Forget( PowerCard cardToRemove ) {
			// A card can be in one of 3 places
			// (1) Purchased / Active
			if(PurchasedCards.Contains( cardToRemove )) {
				foreach(var el in cardToRemove.Elements) --Elements[el];// lose elements from forgotten card
				PurchasedCards.Remove( cardToRemove );
			}
			// (2) Unpurchased, still in hand
			Hand.Remove( cardToRemove );
			// (3) used, discarded
			DiscardPile.Remove( cardToRemove );
		}

		// Holds Fast and Slow actions,
		// depends on Fast/Slow phase to only select the actions that are appropriate
		protected IEnumerable<IActionFactory> AvailableActions { 
			get {
				foreach(IActionFactory action in availableActions)
					yield return action;

				foreach(InnatePower innate in InnatePowers)
					if( !usedInnates.Contains( innate ) )
						yield return innate;
			}
		}

		// so spirits can replay used cards or collect them instead of discard
		public IEnumerable<IActionFactory> UsedActions => usedActions.Distinct();  // distinct incase played twice

		public Speed LastSpeedRequested; // trying to capture what phase spirit is in.

		public virtual IEnumerable<IActionFactory> GetAvailableActions(Speed speed) {
			LastSpeedRequested = speed;
			foreach(var action in AvailableActions) {
				if(IsActiveDuring( speed, action ))
					yield return action;
			}
		}

		public bool IsActiveDuring(Speed speed, IActionFactory card) => card.IsActiveDuring(speed,Elements);


		/// <summary>
		/// Removes it from the Unresolved-list
		/// </summary>
		public void RemoveFromUnresolvedActions(IActionFactory selectedActionFactory ) {
			if(selectedActionFactory is InnatePower ip) { // Could reverse this and instead of listing the used, create a not-used list that we remove them from when used
				usedInnates.Add( ip );
				return;
			}

			int index = availableActions.IndexOf( selectedActionFactory );
			if(index == -1) 
				throw new InvalidOperationException( "can't remove factory that isn't there." );
			usedActions.Add(availableActions[index]);
			availableActions.RemoveAt( index );

		}

		public async Task TriggerEnergyElementsAndReclaims() {

			// Energy
			Energy += EnergyPerTurn;
			// Elements
			Presence.AddElements( Elements );

			int anyCount = Elements[Element.Any];
			Elements[Element.Any] = 0; // we can't draw these in our activated element list
			if(anyCount > 0)
				AddActionFactory( new SelectAnyElements( anyCount ) );

			// Reclaims-1
			int reclaim1Count = Math.Min(
				DiscardPile.Count,  // Reclaim-all will make this 0, disabling any reclaim-1
				Presence.CardPlays.Revealed.Count( x => x.ReclaimOne )
			);
			while(reclaim1Count-- > 0)
				await new Reclaim1().ActivateAsync( this, null );

		}

		public void AddActionFactory( IActionFactory factory ) {
			availableActions.Add( factory );
		}

		public virtual void PurchaseAvailableCards( params PowerCard[] cards ) {
			if(cards.Length > NumberOfCardsPlayablePerTurn)
				throw new InsufficientCardPlaysException();

			foreach(var card in cards)
				PurchaseCard( card );

			foreach(var card in cards)
				AddActionFactory( card );

		}

		public virtual async Task TakeAction(IActionFactory factory, GameState gameState) {
			var oldActionGuid = CurrentActionId; // capture old
			CurrentActionId = Guid.NewGuid(); // set new
			try {
				await factory.ActivateAsync( this, gameState );
				RemoveFromUnresolvedActions( factory );
			} finally {
				CurrentActionId = oldActionGuid; // restore
			}
		}

		void PurchaseCard( PowerCard card ) {
			if(!Hand.Contains( card )) throw new CardNotAvailableException();
			if(card.Cost > Energy) throw new InsufficientEnergyException();

			Hand.Remove( card );
			PurchasedCards.Add( card );
			Energy -= card.Cost;
			Elements.AddRange( card.Elements );
		}

		public Spirit UsePowerProgression() {
			CardDrawer = GetPowerProgression();
			return this;
		}

		protected virtual PowerProgression GetPowerProgression() => throw new NotImplementedException();

		#endregion

		#region presence

		public virtual IEnumerable<Space> SacredSites => Presence.Placed
			.GroupBy(x=>x)
			.Where(grp=>grp.Count()>1)
			.Select(grp=>grp.Key);

		#endregion

		#region Presence Tracks

		/// <summary> # of coins in the bank. </summary>
		public int Energy { get; set; }

		public SpiritPresence Presence {get; }

		/// <summary> Energy gain per turn </summary>
		public int EnergyPerTurn => Presence.Energy.Revealed.Where( x => x.Energy.HasValue ).Last().Energy.Value;

		public virtual int NumberOfCardsPlayablePerTurn => Presence.CardPlays.Revealed.Where(x=>x.CardPlay.HasValue).Last().CardPlay.Value;

		public abstract string Text { get; }

		#endregion

		#region abstract

		public virtual InnatePower[] InnatePowers { get; set; } = Array.Empty<InnatePower>();

		public void Initialize( Board board, GameState gameState ){
			gameState.TimePassed += On_TimePassed;
			InitializeInternal(board,gameState);
		}

		protected abstract void InitializeInternal( Board board, GameState gameState );

		void On_TimePassed(GameState _ ) {
			// reset cards / powers
			DiscardPile.AddRange( PurchasedCards );
			PurchasedCards.Clear();
			availableActions.Clear();
			usedActions.Clear();
			usedInnates.Clear();

			Elements.Clear();
		}

		#endregion

		abstract public string SpecialRules { get; }

		// pluggable, draw power card, or powerprogression
		#region Draw Card
		static readonly IPowerCardDrawer DefaultCardDrawer = new DrawFromDeck();
		public IPowerCardDrawer CardDrawer = DefaultCardDrawer;

		public Task<PowerCard> Draw( GameState gameState, Func<List<PowerCard>, Task> handleNotUsed ) => CardDrawer.Draw(this,gameState,handleNotUsed);

		/// <summary> short cut to CardDrawer.DrawMinor </summary>
		public Task<PowerCard> DrawMinor( GameState gameState ) 
			=> CardDrawer.DrawMinor( this, gameState, null );
		/// <summary> short cut to CardDrawer.DrawMajor </summary>
		public Task<PowerCard> DrawMajor( GameState gameState, int numberToDraw=4, bool forgetCard=true ) 
			=> CardDrawer.DrawMajor( this, gameState, null, forgetCard, numberToDraw ); // Instead of passing in null, could return Tupple with discard cards in it()

		#endregion

		public TargetLandApi PowerApi = new TargetLandApi(); // Replace by: Reaching Grasp, Entwined Power, Shadows

		public Guid CurrentActionId; // Used by Flame's Fury to detect new actions
		public IDamageApplier CustomDamageStrategy = null;

		public virtual InvaderGroup BuildInvaderGroupForPowers( GameState gs, Space space ) {
			var invaderCounts = gs.Tokens[ space ];
			return new InvaderGroup( 
				invaderCounts,
				new DestroyInvaderStrategy( gs.Fear.AddDirect, Cause.Power ),
				CustomDamageStrategy
			);
		}

		public virtual Task DestroyTokenForPowers( GameState gs, Space space, int count, Token token ) { // overriden by Bringer
			return gs.DestroyToken(space,count,token, Cause.Power);
		}

		public Task BuyPowerCardsAsync() => PurchaseCards( NumberOfCardsPlayablePerTurn );

		// Called for both normal buy-cards & from select Power cards that allow puchasing additional
		public async Task PurchaseCards( int canPurchase ) {
			PowerCard[] getPowerCardOptions() => Hand
				.Where( c => c.Cost <= Energy )
				.ToArray();

			PowerCard[] powerCardOptions;
			while(0 < canPurchase
				&& 0 < (powerCardOptions = getPowerCardOptions()).Length
			) {
				string prompt = $"Buy power cards: (${Energy} / {canPurchase})";
				var card = await this.SelectPowerCard( prompt, powerCardOptions, CardUse.Buy, Present.Done );
				if(card != null) {
					PurchaseAvailableCards( card );
					--canPurchase;
				} else
					canPurchase = 0;

			}
		}

		#region Save/Load Memento
		public virtual IMemento<Spirit> SaveToMemento() => new Memento(this);
		public virtual void LoadFrom( IMemento<Spirit> memento ) => ((Memento)memento).Restore(this);

		protected class Memento : IMemento<Spirit> {
			public Memento(Spirit spirit) {
				energy = spirit.Energy;
				elements = spirit.Elements.ToArray();
				presence = spirit.Presence.SaveToMemento();
				hand      = spirit.Hand.ToArray();
				purchased = spirit.PurchasedCards.ToArray();
				discarded = spirit.DiscardPile.ToArray();
				available = spirit.availableActions.ToArray();
				usedActions = spirit.usedActions.ToArray();
				usedInnates = spirit.usedInnates.ToArray();
			}
			public void Restore(Spirit spirit) {
				spirit.Energy = energy;
				spirit.Elements.Clear(); foreach(var p in elements) spirit.Elements[p.Key]=p.Value;
				spirit.Presence.LoadFrom(presence);
				spirit.Hand.SetItems( hand );
				spirit.PurchasedCards.SetItems( purchased );
				spirit.DiscardPile.SetItems( discarded );
				spirit.availableActions.SetItems( available );
				spirit.usedActions.SetItems( usedActions );
				spirit.usedInnates.SetItems( usedInnates );
			}
			readonly int energy;
			readonly KeyValuePair<Element,int>[] elements;
			readonly IMemento<SpiritPresence> presence;
			readonly PowerCard[] hand;
			readonly PowerCard[] purchased;
			readonly PowerCard[] discarded;
			readonly IActionFactory[] available;
			readonly IActionFactory[] usedActions;
			readonly InnatePower[] usedInnates;
		}
		#endregion

	}

}