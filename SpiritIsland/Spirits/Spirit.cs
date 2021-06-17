using SpiritIsland.PowerCards;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {

	public abstract class Spirit :IOption {

		public Spirit(){
			AvailableCards.Add( new PowerCard( "A", 0, Speed.Fast, Element.Air ) );
			AvailableCards.Add( new PowerCard( "B", 0, Speed.Fast, Element.Air ) );
			AvailableCards.Add( new PowerCard( "C", 0, Speed.Fast, Element.Air ) );
			AvailableCards.Add( new PowerCard( "D", 0, Speed.Fast, Element.Air ) );
		}

		public Spirit( params PowerCard[] initialCards ){
			AvailableCards.AddRange( initialCards );
		}

		public virtual InnatePower[] InnatePowers {get; set;} = new InnatePower[0]; // !!! eventually init in constructor

		#region Cards

		public List<PowerCard> AvailableCards = new List<PowerCard>();	// in hand
		public List<PowerCard> ActiveCards = new List<PowerCard>();		// paid for
		public List<PowerCard> PlayedCards = new List<PowerCard>();		// discarded
		public List<IActionFactory> UnresolvedActions = new List<IActionFactory>();

		#endregion

		public readonly List<Space> Presence = new List<Space>();

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

		void RemoveResolvedActions(GameState gameState) {
			var resolvedActions = UnresolvedActions
				.Select(f=>f.Bind(this,gameState))
				.Where(a => a.IsResolved)
				.ToArray();
			foreach (var a in resolvedActions)
				a.Apply();
		}

		public virtual void AddAction(IActionFactory action){
			UnresolvedActions.Add( action );
		}

		public void MarkResolved(GrowthAction action){
			UnresolvedActions.Remove( action );
			if(UnresolvedActions.Count == 0)
				Energy += EnergyPerTurn; // transition 
		}

		public abstract GrowthOption[] GetGrowthOptions();

		public virtual int Elements(Element _) => 0;

		public virtual void BuyAvailableCards(params PowerCard[] cards) {
			if (cards.Length > NumberOfCardsPlayablePerTurn) 
				throw new InsufficientCardPlaysException();

			foreach (var card in cards)
				ActivateCard(card);

			QueueActions(Speed.Fast); // !!!

			// add innate
			foreach (var innate in InnatePowers)
				AddAction(innate);

		}

		void ActivateCard(PowerCard card) {
			if (!AvailableCards.Contains(card)) throw new CardNotAvailableException();
			if (card.Cost > Energy) throw new InsufficientEnergyException();

			AvailableCards.Remove(card);
			ActiveCards.Add(card);
			Energy -= card.Cost;
		}

		void QueueActions(Speed speed) {
			foreach (var card in ActiveCards)
				if (card.Speed == speed)
					AddAction(card);
		}
	}

}