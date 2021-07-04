using System;
using System.Collections.Generic;
using System.Linq;
using SpiritIsland.Core;

namespace SpiritIsland {

	public abstract class Spirit :IOption {

		public GrowthOption[] GrowthOptions { get; protected set; }

		public Spirit(){
			Hand.Add( new PowerCard( "A", 0, Speed.Fast, Element.Air ) );
			Hand.Add( new PowerCard( "B", 0, Speed.Fast, Element.Air ) );
			Hand.Add( new PowerCard( "C", 0, Speed.Fast, Element.Air ) );
			Hand.Add( new PowerCard( "D", 0, Speed.Fast, Element.Air ) );
		}

		public Spirit( params PowerCard[] initialCards ){
			Hand.AddRange( initialCards );
		}

		#region Elements

		public CountDictionary<Element> AllElements => PurchasedCards
			.SelectMany(c=>c.Elements)
			.GroupBy(c=>c)
			.ToDictionary(grp=>grp.Key,grp=>grp.Count())
			.ToCountDict();

		public virtual int Elements(Element element) => AllElements[element];

		#endregion

		public virtual InnatePower[] InnatePowers {get; set;} = System.Array.Empty<InnatePower>(); // !!! eventually init in constructor

		#region Cards

		public List<PowerCard> Hand = new List<PowerCard>();	// in hand
		public List<PowerCard> PurchasedCards = new List<PowerCard>();		// paid for
		public List<PowerCard> DiscardPile = new List<PowerCard>();		// discarded

		// Holds Fast and Slow actions,
		// depends on Fast/Slow phase to only select the actions that are appropriate
		public IEnumerable<IActionFactory> UnresolvedActionFactories => _unresolvedActionFactories;

		readonly List<IActionFactory> _unresolvedActionFactories = new List<IActionFactory>(); // public for testing

		public void Resolve( IActionFactory selectedActionFactory ) {
			int index = _unresolvedActionFactories.IndexOf( selectedActionFactory );
			if(index == -1) throw new InvalidOperationException("can't remove factory that isn't there.");
	
			_unresolvedActionFactories.RemoveAt(index);

			if(_unresolvedActionFactories.Count == 0 && selectedActionFactory is GrowthActionFactory)
				Energy += EnergyPerTurn;	
		}


		#endregion

		public readonly List<Space> Presence = new List<Space>();
		public int PresenceOn(Space space) => Presence.Count(s=>s==space);
		public virtual IEnumerable<Space> SacredSites => Presence
			.GroupBy(x=>x)
			.Where(grp=>grp.Count()>1)
			.Select(grp=>grp.Key);

		/// <summary> # of coins in the bank. </summary>
		public int Energy { get; set; }

		#region Presence Tracks
		public int RevealedEnergySpaces { get; set; } = 1;
		public int RevealedCardSpaces { get; set; } = 1;

		// This is River...
		protected virtual int[] EnergySequence => new int[]{0};
		protected virtual int[] CardSequence => new int[]{0}; 

		/// <summary> Energy gain per turn </summary>
		public int EnergyPerTurn => EnergySequence[RevealedEnergySpaces-1];

		public virtual int NumberOfCardsPlayablePerTurn => CardSequence[RevealedCardSpaces-1];

		public abstract string Text { get; }

		#endregion

		#region intermediate growth states

		public int PowerCardsToDraw; // temporary...

		#endregion

		public virtual void Grow(GameState gameState, int optionIndex) {
			GrowthOption option = this.GetGrowthOptions()[optionIndex];
			foreach (var action in option.GrowthActions)
				AddAction(action);

			RemoveResolvedActions(gameState);
		}

		protected void RemoveResolvedActions(GameState gameState) {

			var resolvedActions = UnresolvedActionFactories
				.Select(f=>new{Factory=f,Action=f.Bind(this,gameState)})
				.Where(pair => pair.Action.IsResolved)
				.ToArray();
			foreach(var x in resolvedActions){
				x.Action.Apply();
				Resolve(x.Factory);
			}
		}

		public virtual void InitializePresence( Board board ) {
			// !!! once all spirits have implemented this,
			// switch this over to pure abstract
			throw new System.NotImplementedException();
		}

		public virtual void AddAction(IActionFactory factory){
			_unresolvedActionFactories.Add( factory );
		}

		public void CollectEnergy() => Energy += EnergyPerTurn;

		public virtual GrowthOption[] GetGrowthOptions() => GrowthOptions;

		public virtual void BuyAvailableCards(params PowerCard[] cards) {
			if (cards.Length > NumberOfCardsPlayablePerTurn) 
				throw new InsufficientCardPlaysException();

			foreach (var card in cards)
				ActivateCard(card);

			foreach (var card in PurchasedCards)
				AddAction(card);

//			foreach (var innate in InnatePowers)
//				AddAction(innate);

		}

		void ActivateCard(PowerCard card) {
			if (!Hand.Contains(card)) throw new CardNotAvailableException();
			if (card.Cost > Energy) throw new InsufficientEnergyException();

			Hand.Remove(card);
			PurchasedCards.Add(card);
			Energy -= card.Cost;
		}

	}

}