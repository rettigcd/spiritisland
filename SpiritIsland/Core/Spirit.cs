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

		public readonly CountDictionary<Element> PreparedElements = new CountDictionary<Element>();

		CountDictionary<Element> actionElements; // null unless we are in the middle of an action

		public async Task PrepareElement() {
			// This is only used by Shifting Memories
			var el = await this.SelectElement("Prepare Element", ElementList.AllElements);
			PreparedElements[el]++;
		}

		public async Task<CountDictionary<Element>> DiscardElements(int totalNumToRemove ) {
			var discarded = new CountDictionary<Element>();

			int index = totalNumToRemove;
			while(index++ < totalNumToRemove) {
				Element el = await this.SelectElement($"Select element to discard for card play ({index} of {totalNumToRemove})",PreparedElements.Keys, Present.Done);
				if( el == default ) break;
				PreparedElements[el]--;
				discarded[el]++;
			}
			return discarded;
		}

		public async Task<bool> HasElements( CountDictionary<Element> subset ) {
			if( actionElements == null ) 
				actionElements = Elements.Clone();
			if( actionElements.Contains( subset ) ) return true;

			// Check if we have prepared element markers to fill the missing elements
			if(PreparedElements.Any()) {
				var missing = subset.Except(Elements);
				if(PreparedElements.Contains(missing) && await this.UserSelectsFirstText($"Meet elemental threshold:"+subset.ToString(), "Yes, use prepared elements", "No, I'll pass.")) {
					foreach(var pair in missing)
						PreparedElements[pair.Key] -= pair.Value;
					return true;
				}
			}

			return false;
		}

		public bool CouldHaveElements( CountDictionary<Element> subset ) {
			var els = PreparedElements.Any() ? Elements.Union(PreparedElements): Elements;
			return els.Contains(subset);
		}

		#endregion

		#region Growth

		public GrowthOption[] GrowthOptions => growthOptionGroup.Options;
		protected GrowthOptionGroup growthOptionGroup;

		public virtual async Task DoGrowth(GameState gameState) {

			int count = growthOptionGroup.SelectionCount;
			List<GrowthOption> remainingOptions = growthOptionGroup.Options.ToList();

			// !!! there is a bug here.  Somehow, count can exceed the # of options

			while(count-- > 0) {
				var currentOptions = remainingOptions.Where( o => o.GainEnergy + Energy >= 0 ).ToArray();
				GrowthOption option = (GrowthOption)await this.Select( "Select Growth Option", currentOptions, Present.Always );
				remainingOptions.Remove( option );

				await GrowAndResolve( option, gameState );
			}

			await ApplyRevealedPresenceTracks( gameState );

		}

		public Task GrowAndResolve( GrowthOption option, GameState gameState ) {
			var ctx = new SpiritGameStateCtx(this,gameState,Cause.Growth);

			if( option.AutoSelectSingle && option.GrowthActions.Length == 1 )
				return option.GrowthActions[0].ActivateAsync( ctx );
			
			Grow( option );
			return ResolveActions( ctx );

		}

		public void Grow( GrowthOption option ) {
			foreach(GrowthActionFactory action in option.GrowthActions)
				AddActionFactory( action );
		}

		/// <remarks>override in a test.  Could be refactored to not need to be virtual</remarks>
		public virtual GrowthOptionGroup GetGrowthOptions() => growthOptionGroup;

		public async Task ApplyRevealedPresenceTracks(GameState gs) {

			var ctx = new SpiritGameStateCtx(this,gs,Cause.Growth);

			foreach(var actions in Presence.RevealedActions)
				await actions.ActivateAsync( ctx );

			// Energy
			Energy += EnergyPerTurn;
			// Elements
			Presence.AddElements( Elements );

			int anyCount = Elements[Element.Any];
			Elements[Element.Any] = 0; // we can't draw these in our activated element list
			if(anyCount > 0)
				AddActionFactory( new SelectAnyElements( anyCount ) );

		}

		// !!! Seems like this should be private / protected and not called from outside.
		public async Task ResolveActions( SpiritGameStateCtx ctx ) {
			Phase speed = ctx.GameState.Phase;
			Present present = ctx.GameState.Phase == Phase.Growth ? Present.Always : Present.Done;

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

				await TakeAction( (IActionFactory)option, ctx );
			}

		}

		#endregion

		#region Cards

		public List<PowerCard> Hand = new List<PowerCard>();	// in hand
		public List<PowerCard> PurchasedCards = new List<PowerCard>();		// paid for
		public List<PowerCard> DiscardPile = new List<PowerCard>(); // Cards are not transferred to discard pile until end of turn because we need to keep track of their elements.

		readonly List<IActionFactory> availableActions = new List<IActionFactory>();
		readonly HashSet<IActionFactory> usedActions = new HashSet<IActionFactory>();
		readonly List<InnatePower>       usedInnates = new List<InnatePower>();

		// so spirits can replay used cards or collect them instead of discard
		public IEnumerable<IActionFactory> UsedActions => usedActions;

		public virtual IEnumerable<IActionFactory> GetAvailableActions(Phase speed) {
			foreach(var action in AvailableActions) {
				if( IsActiveDuring( speed, action ) )
					yield return action;
			}
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

		public virtual async Task ForgetPowerCard() {
			var options = PurchasedCards.Union( Hand ).Union( DiscardPile )
				.ToArray();
			PowerCard cardToForget = await this.SelectPowerCard( "Select power card to forget", options, CardUse.Forget, Present.Always );
			Forget( (PowerCard)cardToForget );
		}

		public virtual void Forget( PowerCard cardToRemove ) {
			// A card can be in one of 3 places
			// (1) Purchased / Active
			if(PurchasedCards.Contains( cardToRemove )) {
				foreach(var el in cardToRemove.Elements) 
					Elements[el.Key]-=el.Value;// lose elements from forgotten card
				PurchasedCards.Remove( cardToRemove );
			}
			// (2) Unpurchased, still in hand
			Hand.Remove( cardToRemove );
			// (3) used, discarded
			DiscardPile.Remove( cardToRemove );
		}

		public bool IsActiveDuring(Phase speed, IActionFactory actionFactory) => actionFactory.CouldActivateDuring( speed, this );

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


		public void AddActionFactory( IActionFactory factory ) {
			availableActions.Add( factory );
		}

		public Spirit UsePowerProgression() {
			CardDrawer = GetPowerProgression();
			return this;
		}

		protected virtual async Task TakeAction(IActionFactory factory, SpiritGameStateCtx ctx) {
			actionElements = null; // make sure these are cleared out for every action
			var oldActionGuid = CurrentActionId; // capture old
			CurrentActionId = Guid.NewGuid(); // set new
			try {
				await factory.ActivateAsync( ctx );
				RemoveFromUnresolvedActions( factory );
			} finally {
				CurrentActionId = oldActionGuid; // restore
				actionElements = null;
			}
		}

		protected virtual PowerProgression GetPowerProgression() => throw new NotImplementedException();

		#endregion

		#region presence

		public virtual IEnumerable<Space> SacredSites => Presence.Placed
			.GroupBy(x=>x)
			.Where(grp=>grp.Count()>1)
			.Select(grp=>grp.Key);

		/// <summary> # of coins in the bank. </summary>
		public int Energy { get; set; }

		public SpiritPresence Presence {get; }

		/// <summary> Energy gain per turn </summary>
		public int EnergyPerTurn => Presence.Energy.Revealed.Where( x => x.Energy.HasValue ).Last().Energy.Value;
		public virtual int NumberOfCardsPlayablePerTurn => Presence.CardPlayCount + tempCardPlayBoost;

		public int tempCardPlayBoost = 0;


		#endregion

		public abstract string Text { get; }

		abstract public SpecialRule[] SpecialRules { get; }

		public virtual InnatePower[] InnatePowers { get; set; } = Array.Empty<InnatePower>();

		public void Initialize( Board board, GameState gameState ){
			gameState.TimePasses_WholeGame += On_TimePassed;
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

		// pluggable, draw power card, or powerprogression
		#region Draw Card

		static readonly IPowerCardDrawer DefaultCardDrawer = new DrawFromDeck();
		public IPowerCardDrawer CardDrawer { get; set; } = DefaultCardDrawer; // !!! public so tests can set it - find another way to set so we can make the set private

		public Task<PowerCard> Draw( GameState gameState, Func<List<PowerCard>, Task> handleNotUsed ) 
			=> CardDrawer.Draw(this,gameState,handleNotUsed);

		public Task<PowerCard> DrawMinor( GameState gameState ) 
			=> CardDrawer.DrawMinor( this, gameState, null );

		public Task<PowerCard> DrawMajor( GameState gameState, int numberToDraw=4, bool forgetCard=true ) 
			=> CardDrawer.DrawMajor( this, gameState, null, forgetCard, numberToDraw ); // Instead of passing in null, could return Tupple with discard cards in it()

		#endregion

		// Used by Flame's Fury to detect new actions
		// Used by Observe The Ever-Changing World to distinguish between actions
		public Guid CurrentActionId; // !!! this might not work when we go to multi-player

		#region Purchase Cards

		// Purchase 1: select full # of cards from hand
		public async Task PurchasePlayableCards() {
			await PurchaseCardsFromHand( NumberOfCardsPlayablePerTurn );
			tempCardPlayBoost = 0;
		}

		// Purchase 2: select specified # of cards from hand
		// Called for both normal buy-cards & from select Power cards that allow puchasing additional
		public async Task PurchaseCardsFromHand( int canPurchase ) {
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
					PurchaseCard( card );
					--canPurchase;
				} else
					canPurchase = 0;

			}
		}

		// Purchase 3: purchase specified cards from hand  (from Test)
		// !!! this Purchase is only called from Test - replace it with Call PurchasePlayableCards();
		public virtual void PurchaseAvailableCards_Test( params PowerCard[] cards ) {
			if(cards.Length > NumberOfCardsPlayablePerTurn)
				throw new InsufficientCardPlaysException();

			foreach(var card in cards)
				PurchaseCard( card );

			tempCardPlayBoost = 0; // makes test pass, but Rampant Green test is testing wrong thing
		}

		void PurchaseCard( PowerCard card ) {
			if(!Hand.Contains( card )) throw new CardNotAvailableException();
			if(card.Cost > Energy) throw new InsufficientEnergyException();

			Hand.Remove( card );
			PurchasedCards.Add( card );
			Energy -= card.Cost;
			foreach(var el in card.Elements )
				Elements[el.Key] += el.Value;

			AddActionFactory( card );
		}

		#endregion

		#region Save/Load Memento
		public virtual IMemento<Spirit> SaveToMemento() => new Memento(this);
		public virtual void LoadFrom( IMemento<Spirit> memento ) => ((Memento)memento).Restore(this);

		protected class Memento : IMemento<Spirit> {
			public Memento(Spirit spirit) {
				energy = spirit.Energy;
				elements = spirit.Elements.ToArray();
				preparedElements = spirit.PreparedElements.ToArray();
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
				InitFromArray( spirit.Elements, elements);
				InitFromArray( spirit.PreparedElements, preparedElements);
				spirit.Presence.LoadFrom(presence);
				spirit.Hand.SetItems( hand );
				spirit.PurchasedCards.SetItems( purchased );
				spirit.DiscardPile.SetItems( discarded );
				spirit.availableActions.SetItems( available );
				spirit.usedActions.SetItems( usedActions );
				spirit.usedInnates.SetItems( usedInnates );
			}
			static void InitFromArray(CountDictionary<Element> dict, KeyValuePair<Element,int>[] array ) {
				dict.Clear(); 
				foreach(var p in array) dict[p.Key]=p.Value;
			}
			readonly int energy;
			readonly KeyValuePair<Element,int>[] elements;
			readonly KeyValuePair<Element,int>[] preparedElements;
			readonly IMemento<SpiritPresence> presence;
			readonly PowerCard[] hand;
			readonly PowerCard[] purchased;
			readonly PowerCard[] discarded;
			readonly IActionFactory[] available;
			readonly IActionFactory[] usedActions;
			readonly InnatePower[] usedInnates;
		}
		#endregion

		#region Power Plug-ins

		public TargetLandApi TargetLandApi = new TargetLandApi(); // Replace by: Reaching Grasp, Entwined Power, Shadows

		public IDamageApplier CustomDamageStrategy = null; // Fires Fury Plugs in a custom +1 bonus damage

		public virtual InvaderGroup BuildInvaderGroupForPowers( GameState gs, Space space ) {
			var invaderCounts = gs.Tokens[ space ];
			return new InvaderGroup( 
				invaderCounts,
				new DestroyInvaderStrategy( gs.Fear.AddDirect, Cause.Power ),
				CustomDamageStrategy
			);
		}

		// overriden by Bringer, Bringer's BuildInvaderGroupForPower uses this.
		public virtual Task DestroyInvaderForPowers( GameState gs, Space space, int count, Token token ) {
			return gs.Tokens.DestroyToken(space,count,token, Cause.Power);
		}

		/// <summary>Hook for Grinning Trickster to add additional strife for power</summary>
		public virtual Task AddStrife( TargetSpaceCtx ctx, Token invader ) {
			ctx.Tokens.AddStrifeTo( invader );
			return Task.CompletedTask;
		}

		// Overriden by Trickster because it costs them presence
		public virtual async Task RemoveBlight( TargetSpaceCtx ctx ) {
			if(ctx.Blight.Any)
				await ctx.GameState.AddBlight( ctx.Space, -1 );
		}

		public virtual TokenPusher PushFactory( TargetSpaceCtx ctx ) => new TokenPusher( ctx );
		public virtual TokenGatherer GatherFactory( TargetSpaceCtx ctx ) => new TokenGatherer( ctx );

		#endregion

	}

}