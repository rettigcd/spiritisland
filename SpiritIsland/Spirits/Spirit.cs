using SpiritIsland.PowerCards;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland
{

	public abstract class Spirit {

		public Spirit(){
			AvailableCards.Add( new PowerCard( "A", 0, Speed.Fast, Element.Air ) );
			AvailableCards.Add( new PowerCard( "B", 0, Speed.Fast, Element.Air ) );
			AvailableCards.Add( new PowerCard( "C", 0, Speed.Fast, Element.Air ) );
			AvailableCards.Add( new PowerCard( "D", 0, Speed.Fast, Element.Air ) );
		}

		public Spirit(params PowerCard[] initialCards){
			AvailableCards.AddRange( initialCards );
		}


		#region Cards

		public List<PowerCard> AvailableCards = new List<PowerCard>();	// in hand
		public List<PowerCard> ActiveCards = new List<PowerCard>();		// paid for
		public List<PowerCard> PlayedCards = new List<PowerCard>();		// discarded

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
		
		#endregion

		#region intermediate growth states

		public int PowerCardsToDraw;

		#endregion

		public virtual void Grow(GameState gameState, int optionIndex) {
			GrowthOption option = this.GetGrowthOptions(gameState)[optionIndex];
			foreach (var action in option.GrowthActions)
				AddAction(action);

			RemoveResolvedActions();
		}

		void RemoveResolvedActions() {
			var resolved = UnresolvedActions
				.Where(a => a.IsResolved)
				.ToArray();
			foreach (var a in resolved)
				a.Apply();
		}

		public virtual void AddAction(GrowthAction action){
			UnresolvedActions.Add( action );
		}

		public void MarkResolved(GrowthAction action){
			UnresolvedActions.Remove( action );
			if(UnresolvedActions.Count == 0)
				Energy += EnergyPerTurn; // transition 
		}

		public List<GrowthAction> UnresolvedActions = new List<GrowthAction>();


		public abstract GrowthOption[] GetGrowthOptions(GameState gameState);

		public virtual int Elements(Element _) => 0;

		public virtual void PlayAvailableCards(params int[] cards){

		}

		public void SelectPowerCards(params PowerCard[] cards) {

			if( cards.Length > NumberOfCardsPlayablePerTurn )
				throw new InsufficientCardPlaysException();

			int totalCost = cards.Sum(x=>x.Cost);
			if(totalCost > Energy)
				throw new InsufficientEnergyException();

			foreach(var card in cards) {
				if(!AvailableCards.Contains( card ))
					throw new CardNotAvailableException();
				AvailableCards.Remove( card );
			}

			ActiveCards.AddRange( cards );
			Energy -= totalCost;
		}

	}

}