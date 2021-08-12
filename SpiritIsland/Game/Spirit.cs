using System;
using System.Collections.Generic;
using System.Linq;
using SpiritIsland.Core;

namespace SpiritIsland {

	public abstract class Spirit : IOption {

		#region constructor

		public Spirit( 
			Track[] energyTrack
			, Track[] cardTrack
			, params PowerCard[] initialCards 
		){
			EnergyTrack = energyTrack;
			CardTrack = cardTrack;

			foreach(var card in initialCards)
				RegisterNewCard(card);
		}

		public void RegisterNewCard( PowerCard card ){
			Hand.Add(card);
			if(card is TargetSpace_PowerCard ts)
				ts.TargetedSpace += (args) => TargetedSpace?.Invoke(args);
		}

		#endregion

		#region Elements
		protected IEnumerable<Element> TrackElements() {
			var energyElements = EnergyTrack.Take(RevealedEnergySpaces)
				.Where(t=>t.Element.HasValue)
				.Select(t=>t.Element.Value);
			var cardElements = CardTrack.Take( RevealedCardSpaces )
				.Where( t => t.Element.HasValue )
				.Select( t => t.Element.Value );
			return energyElements.Concat(cardElements);
		}

		// !!! this could be calculated and cached when cards are purchased
		public CountDictionary<Element> Elements{ get { 
			var fromCards = PurchasedCards.SelectMany( c => c.Elements ).ToArray();
			var fromTrack = TrackElements().ToArray();
			return fromCards.Concat(fromTrack)
					.GroupBy(c=>c)
				.ToDictionary( grp => grp.Key, grp => grp.Count() )
				.ToCountDict();
		} }

		#endregion

		#region Growth

		public GrowthOption[] GrowthOptions { get; protected set; }

		public virtual void Grow( GameState gameState, int optionIndex ) {

			GrowthOption option = this.GetGrowthOptions()[optionIndex];
			foreach(var action in option.GrowthActions)
				AddActionFactory( action );

			RemoveResolvedActions( gameState, Speed.Growth );

		}

		public void CollectEnergy() => Energy += EnergyPerTurn;

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
				var action = new BaseAction(engine);
				if(action.IsResolved)
					Resolve(factory);
				else
					decisions.Clear(); // clean unresolved action decisions out
			}
		}

		public void Forget( PowerCard cardToRemove ) {
			PurchasedCards.Remove( cardToRemove );
			Hand.Remove( cardToRemove );
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

			if(_unresolvedActionFactories.Count == 0 && selectedActionFactory is GrowthActionFactory)
				Energy += EnergyPerTurn;
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
		}

		#endregion

		#region presence

		public readonly List<Space> Presence = new List<Space>();
		public int PresenceOn(Space space) => Presence.Count(s=>s==space);
		public virtual IEnumerable<Space> SacredSites => Presence
			.GroupBy(x=>x)
			.Where(grp=>grp.Count()>1)
			.Select(grp=>grp.Key);

		#endregion

		#region Presence Tracks

		public virtual int RevealedEnergySpaces { get; set; } = 1;

		public virtual int RevealedCardSpaces { get; set; } = 1;

		/// <summary> # of coins in the bank. </summary>
		public int Energy { get; set; }

		public Track[] EnergyTrack {get; }

		public Track[] CardTrack { get; }

		public bool HasMoreEnergyPresence => RevealedEnergySpaces < EnergyTrack.Length;
		public Track NextEnergyPresence => EnergyTrack[RevealedEnergySpaces];

		public bool HasMoreCardPresence => RevealedCardSpaces < CardTrack.Length;
		public Track NextCardPresence => CardTrack[RevealedCardSpaces];

		/// <summary> Energy gain per turn </summary>
		public int EnergyPerTurn => EnergyTrack.Take( RevealedEnergySpaces ).Where( x => x.Energy.HasValue ).Last().Energy.Value;

		public virtual int NumberOfCardsPlayablePerTurn => CardTrack.Take(RevealedCardSpaces).Where(x=>x.CardPlay.HasValue).Last().CardPlay.Value;

		public abstract string Text { get; }

		#endregion

		#region intermediate growth states

		public int PowerCardsToDraw; // temporary...

		#endregion

		#region abstract

		public virtual InnatePower[] InnatePowers { get; set; } = Array.Empty<InnatePower>();

		public virtual PowerCardApi GetPowerCardApi( ActionEngine engine ) {
			return new PowerCardApi( engine );
		}

		public virtual void Initialize( Board board, GameState gameState ){
			gameState.TimePassed += On_TimePassed;
		}

		void On_TimePassed(GameState _ ) {
			// !!! Need a 2-turn unit test that makes sure these things get cleared each turn
			// because it is easy to forget to call base.Initialize(board,gamestate)
			DiscardPile.AddRange( PurchasedCards );
			PurchasedCards.Clear();
			usedInnates.Clear();
		}

		#endregion

		public Stack<IDecision> decisions = new();

		public event SpaceTargetedEvent TargetedSpace;

	}

	public class SpiritActionResolved {
		public IActionFactory Factory { get; set; }
		public IAction Action { get; set; }
	}

}