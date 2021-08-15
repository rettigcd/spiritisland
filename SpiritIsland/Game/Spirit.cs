using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland {

	public abstract class Spirit : IOption {

		#region constructor

		public Spirit( 
			Track[] energyTrack
			, Track[] cardTrack
			, params PowerCard[] initialCards 
		){
			Presence = new MyPresence(
				new PresenceTrack( energyTrack ),
				new PresenceTrack( cardTrack )
			);

			foreach(var card in initialCards)
				RegisterNewCard(card);

			Action = new BaseAction(decisions);
		}
		public BaseAction Action { get; }

		public void RegisterNewCard( PowerCard card ){
			Hand.Add(card);
			if(card is TargetSpace_PowerCard ts)
				ts.TargetedSpace += (args) => TargetedSpace?.Invoke(args);
		}

		#endregion

		#region Elements

		public readonly CountDictionary<Element> Elements = new CountDictionary<Element>();

		#endregion

		#region Growth

		public GrowthOption[] GrowthOptions { get; protected set; }

		public virtual void Grow( GameState gameState, int optionIndex ) {

			GrowthOption option = this.GetGrowthOptions()[optionIndex];
			foreach(var action in option.GrowthActions)
				AddActionFactory( action );

			RemoveResolvedActions( gameState, Speed.Growth );

		}

		public virtual GrowthOption[] GetGrowthOptions() => GrowthOptions;

		#endregion

		#region Cards

		public List<PowerCard> Hand = new List<PowerCard>();	// in hand
		public List<PowerCard> PurchasedCards = new List<PowerCard>();		// paid for
		public List<PowerCard> DiscardPile = new List<PowerCard>();     // discarded
		readonly List<IActionFactory> _unresolvedActionFactories = new List<IActionFactory>(); // public for testing
		readonly List<InnatePower> usedInnates = new List<InnatePower>();

		protected void RemoveResolvedActions( GameState gameState, Speed speed ) {

			var factories = GetUnresolvedActionFactories( speed ).ToArray();
			var engine = new ActionEngine(this,gameState);
			foreach(var factory in factories) {
				factory.Activate(engine);
				if(Action.IsResolved)
					Resolve(factory);
				else
					decisions.Clear(); // clean unresolved action decisions out
			}
		}

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
		protected IEnumerable<IActionFactory> GetUnresolvedActionFactories() {
			foreach(var action in _unresolvedActionFactories) yield return action;
			foreach(var innate in this.InnatePowers)
				if(!usedInnates.Contains(innate) && innate.PowersActivated(this)>0)
					yield return innate;		
		} 

		public IEnumerable<IActionFactory> GetUnresolvedActionFactories(Speed speed) {
			return GetUnresolvedActionFactories()
				.Where(f=>f.Speed == speed);
		} 

		/// <summary>
		/// Removes from list.  Triggers Energy when last growth removed
		/// </summary>
		public void Resolve( IActionFactory selectedActionFactory) {
			RemoveUnresolvedFactory(selectedActionFactory); // collapse this method
		}

		/// <summary>
		/// Removes it from the Unresolved-list
		/// </summary>
		public void RemoveUnresolvedFactory(IActionFactory selectedActionFactory ) {
			if(selectedActionFactory is InnatePower ip) {
				usedInnates.Add( ip );
				return;
			}

			int index = _unresolvedActionFactories.IndexOf( selectedActionFactory );
			if(index == -1) 
				throw new InvalidOperationException( "can't remove factory that isn't there." );

			_unresolvedActionFactories.RemoveAt( index );

			if(_unresolvedActionFactories.Count == 0 && selectedActionFactory is GrowthActionFactory growthFactory) {
				// Energy
				Energy += EnergyPerTurn;
				// Elements
				Elements.AddRange( Presence.Energy.Revealed.Where( t => t.Element.HasValue ).Select( t => t.Element.Value ) );
				Elements.AddRange( Presence.CardPlays.Revealed.Where( t => t.Element.HasValue ).Select( t => t.Element.Value ) );

				// Reclaims-1
				if(!(selectedActionFactory is Reclaim1)) { // prevent retriggering following Reclaim1
					int reclaim1Count = Math.Min( 
						DiscardPile.Count,	// Reclaim-all will make this 0, disabling any reclaim-1
						Presence.CardPlays.Revealed.Count( x => x.ReclaimOne ) 
					);
					while(reclaim1Count-->0)
						AddActionFactory( new Reclaim1() );
				}
			}
		}

		public virtual void AddActionFactory( IActionFactory factory ) {
			_unresolvedActionFactories.Add( factory );
		}

		public virtual void PurchaseAvailableCards( params PowerCard[] cards ) {
			if(cards.Length > NumberOfCardsPlayablePerTurn)
				throw new InsufficientCardPlaysException();

			foreach(var card in cards)
				PurchaseCard( card );

			foreach(var card in PurchasedCards)
				AddActionFactory( card );

		}

		public int Flush( Speed speed ) {
			var toFlush = GetUnresolvedActionFactories()
				.Where( f => f.Speed == speed )
				.ToArray();
			foreach(var factory in toFlush)
				RemoveUnresolvedFactory( factory );
			return toFlush.Length;
		}

		void PurchaseCard( PowerCard card ) {
			if(!Hand.Contains( card )) throw new CardNotAvailableException();
			if(card.Cost > Energy) throw new InsufficientEnergyException();

			Hand.Remove( card );
			PurchasedCards.Add( card );
			Energy -= card.Cost;
			Elements.AddRange( card.Elements );
		}

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

		#region intermediate growth states

		public int PowerCardsToDraw; // temporary...

		#endregion

		#region abstract

		public virtual InnatePower[] InnatePowers { get; set; } = Array.Empty<InnatePower>();

		public PowerCardApi PowerCardApi {get; set;} = new PowerCardApi();

		public virtual void Initialize( Board board, GameState gameState ){
			gameState.TimePassed += On_TimePassed;
		}

		void On_TimePassed(GameState _ ) {
			// !!! Need a 2-turn unit test that makes sure these things get cleared each turn
			// because it is easy to forget to call base.Initialize(board,gamestate)
			DiscardPile.AddRange( PurchasedCards );
			PurchasedCards.Clear();
			usedInnates.Clear();
			Elements.Clear();
		}

		#endregion

		static Task DefaultDrawPowerCard(ActionEngine engine, string majorMinor ) {
			engine.Self.PowerCardsToDraw++;
			return Task.CompletedTask;
		}

		// pluggable, draw power card, or powerprogression
		// string should be "major", "minor" or ""
		public Func<ActionEngine,string, Task> DrawPowerCard = DefaultDrawPowerCard;

		public Stack<IDecision> decisions = new();

		public event SpaceTargetedEvent TargetedSpace;

	}

}