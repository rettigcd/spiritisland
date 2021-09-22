using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public abstract class Spirit : IOption {

		#region constructor

		public Spirit( MyPresence presence, params PowerCard[] initialCards ){
			Presence = presence;

			foreach(var card in initialCards)
				AddCardToHand(card);

			Action = new BaseAction();
		}
		public BaseAction Action { get; }

		public void AddCardToHand( PowerCard card ){
			Hand.Add(card);
		}

		#endregion

		#region Elements

		public readonly CountDictionary<Element> Elements = new CountDictionary<Element>();

		#endregion

		#region Growth



		public GrowthOption[] GrowthOptions { get; protected set; }


		public virtual void Grow( GameState gameState, int optionIndex ) { // overrident by Keeper and Sharp Fangs

			var (growthOptions,_) = this.GetGrowthOptions();

			GrowthOption option = growthOptions[optionIndex];
			foreach(GrowthActionFactory action in option.GrowthActions)
				AddActionFactory( action );

		}

		public virtual void Grow( GameState gameState, GrowthOption option ) { // overrident by Keeper and Sharp Fangs
			foreach(GrowthActionFactory action in option.GrowthActions)
				AddActionFactory( action );
		}

		public virtual (GrowthOption[],int) GetGrowthOptions() => (GrowthOptions, growthOptionSelectionCount);
		protected int growthOptionSelectionCount = 1;

		#endregion

		#region Cards

		public List<PowerCard> Hand = new List<PowerCard>();	// in hand
		public List<PowerCard> PurchasedCards = new List<PowerCard>();		// paid for
		public List<PowerCard> DiscardPile = new List<PowerCard>();     // discarded
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
				foreach(var action in availableActions) {
					action.UpdateFromSpiritState( this.Elements );
					yield return action;
				}
				foreach(var innate in InnatePowers) {
					if( usedInnates.Contains( innate ) ) continue; // explude played
					innate.UpdateFromSpiritState( this.Elements ); // update status
					if(innate.IsTriggered)
						yield return innate;
				}
			}
		}

		// so spirits can replay used cards or collect them instead of discard
		public IEnumerable<IActionFactory> UsedActions => usedActions.Distinct();  // distinct incase played twice

		public virtual IEnumerable<IActionFactory> GetAvailableActions(Speed speed) {
			foreach( var action in AvailableActions )
				action.UpdateFromSpiritState( this.Elements );
			return AvailableActions.Where( GetFilter( speed ) );
		}

		static protected Func<IActionFactory, bool> GetFilter( Speed speed ) {
			return speed switch {
				Speed.Fast => ( x ) => x.Speed == Speed.Fast || x.Speed == Speed.FastOrSlow,
				Speed.Slow => ( x ) => x.Speed == Speed.Slow || x.Speed == Speed.FastOrSlow,
				_ => x => x.Speed == speed
			};
		}

		/// <summary>
		/// Removes it from the Unresolved-list
		/// </summary>
		public void RemoveUnresolvedActions(IActionFactory selectedActionFactory ) {
			if(selectedActionFactory is InnatePower ip) { // Could reverse this and instead of listing the used, create a not-used list that we remove them from when used
				usedInnates.Add( ip );
				return;
			}

			int index = availableActions.IndexOf( selectedActionFactory );
			if(index == -1) 
				throw new InvalidOperationException( "can't remove factory that isn't there." );
			usedActions.Add(availableActions[index]);
			availableActions.RemoveAt( index );

			//if(availableActions.Count == 0 && selectedActionFactory is GrowthActionFactory
			//	&& !(selectedActionFactory is Reclaim1 || selectedActionFactory is SelectAnyElements)
			//)
			//	TriggerEnergyElementsAndReclaims();

		}

		public async Task TriggerEnergyElementsAndReclaims() {

			// Energy
			Energy += EnergyPerTurn;
			// Elements
			Elements.AddRange( Presence.Energy.Revealed.Where( t => t.Element.HasValue ).Select( t => t.Element.Value ) );
			Elements.AddRange( Presence.CardPlays.Revealed.Where( t => t.Element.HasValue ).Select( t => t.Element.Value ) );

			int anyCount = Elements[Element.Any];
			Elements[Element.Any] = 0; // we can't draw these in our activated element list
			if(anyCount>0)
				AddActionFactory( new SelectAnyElements( anyCount ) );

			// Reclaims-1
			int reclaim1Count = Math.Min(
				DiscardPile.Count,  // Reclaim-all will make this 0, disabling any reclaim-1
				Presence.CardPlays.Revealed.Count( x => x.ReclaimOne )
			);
			while(reclaim1Count-- > 0)
				await new Reclaim1().ActivateAsync(this,null);

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
			await factory.ActivateAsync( this, gameState );
			RemoveUnresolvedActions( factory );
		}

		public int Flush( Speed speed ) {
			var toFlush = AvailableActions
				.Where( x=>x.Speed == speed )
				.ToArray();
			foreach(var factory in toFlush)
				RemoveUnresolvedActions( factory );
			return toFlush.Length 
				+ (speed == Speed.Slow ? Flush(Speed.FastOrSlow) : 0 );
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

		public MyPresence Presence {get; }

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
		public Task<PowerCard> DrawMinor( GameState gameState ) => CardDrawer.DrawMinor( this, gameState, null );
		/// <summary> short cut to CardDrawer.DrawMajor </summary>
		public Task<PowerCard> DrawMajor( GameState gameState ) => CardDrawer.DrawMajor( this, gameState, null );

		#endregion

		public TargetLandApi PowerApi = new TargetLandApi(); // Replace by: Reaching Grasp, Entwined Power, Shadows

		public virtual InvaderGroup BuildInvaderGroupForPowers( GameState gs, Space space ) {
			var invaderCounts = gs.Tokens[ space ];
			return new InvaderGroup( invaderCounts ) {
				DestroyInvaderStrategy = new DestroyInvaderStrategy( gs.Fear.AddDirect, Cause.Power ),
			};
		}

		public virtual Task DestroyDahanForPowers( GameState gs, Space space, int count, Token dahanToken ) {
			return gs.DahanDestroy(space,count,dahanToken, Cause.Power);
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
				var card = await this.SelectPowerCard( prompt, powerCardOptions, Present.Done );
				if(card != null) {
					PurchaseAvailableCards( card );
					--canPurchase;
				} else
					canPurchase = 0;

			}
		}

	}

}