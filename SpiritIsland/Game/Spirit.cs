using System;
using System.Collections.Generic;
using System.Linq;
using SpiritIsland.Core;

namespace SpiritIsland {

	public abstract class Spirit : IOption {

		#region constructor

		public Spirit( params PowerCard[] initialCards ){
			Hand.AddRange( initialCards );
		}

		#endregion

		#region Elements
		protected virtual IEnumerable<Element> TrackElements() {
			return Enumerable.Empty<Element>();
		}

		// !!! this could be calculated and cached when cards are purchased
		public CountDictionary<Element> Elements => PurchasedCards
			.SelectMany(c=>c.Elements)
			.Concat(TrackElements())
			.GroupBy(c=>c)
			.ToDictionary(grp=>grp.Key,grp=>grp.Count())
			.ToCountDict();

		#endregion

		#region Growth

		public GrowthOption[] GrowthOptions { get; protected set; }

		public virtual void Grow( GameState gameState, int optionIndex ) {

			usedInnates.Clear();

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

			var resolvedActions = GetUnresolvedActionFactories( speed )
				.Select( f => new { Factory = f, Action = f.Bind( this, gameState ) } )
				.Where( pair => pair.Action.IsResolved )
				.ToArray();
			foreach(var x in resolvedActions)
				Resolve( x.Factory, x.Action );

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
		public void Resolve( IActionFactory selectedActionFactory, IAction action ) {
			resolvedActions.Add(new SpiritActionResolved{
				Factory=selectedActionFactory,
				Action=action 
			} );
			RemoveFactory(selectedActionFactory);
		}

		public readonly List<SpiritActionResolved> resolvedActions = new List<SpiritActionResolved>();

		/// <summary>
		/// Removes it from the Unresolved-list
		/// </summary>
		public void RemoveFactory(IActionFactory selectedActionFactory ) {
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
				RemoveFactory( factory );
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

		protected virtual int[] EnergySequence => new int[]{0};
		protected virtual int[] CardSequence => new int[]{0}; 

		public int[] GetEnergySequence() => EnergySequence;
		public int[] GetCardSequence() => CardSequence;

		/// <summary> Energy gain per turn </summary>
		public int EnergyPerTurn => EnergySequence[RevealedEnergySpaces-1];

		public virtual int NumberOfCardsPlayablePerTurn => CardSequence[RevealedCardSpaces-1];

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

		public abstract void Initialize( Board board, GameState gameState );

		#endregion

		public Stack<IDecision> decisions = new();

	}

	public class SpiritActionResolved {
		public IActionFactory Factory { get; set; }
		public IAction Action { get; set; }
	}

	static public class ExtendElements {

		static public bool Has(this CountDictionary<Element> activated, Dictionary<Element, int> needed ) {
			return needed.All( pair => pair.Value <= activated[pair.Key] );
		}

		static public bool Has( this CountDictionary<Element> activated, params Element[] requiredElements ) {
			var required = requiredElements
				.GroupBy( x => x )
				.ToDictionary( grp => grp.Key, grp => grp.Count() );
			return activated.Has( required );
		}

	}

}