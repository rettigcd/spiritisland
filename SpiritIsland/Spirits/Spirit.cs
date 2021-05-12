using System.Collections.Generic;

namespace SpiritIsland {

	public abstract class Spirit {

		#region Cards

		public List<PowerCard> AvailableCards = new List<PowerCard>();
		public List<PowerCard> PlayedCards = new List<PowerCard>();

		#endregion

		public readonly List<Space> Presence = new List<Space>();

		/// <summary> # of coins in the bank. </summary>
		public int Energy { get; set; }

		#region Presence Tracks

		public int RevealedEnergySpaces { get; set; } = 1;
		public int RevealedCardSpaces { get; set; } = 1;

		// This is River...
		public virtual int EnergyGrowth { get{
			return new int[]{1, 2, 2, 3, 4, 4, 5 }[RevealedEnergySpaces-1];
		} } // set up for River
		// This is River...
		public virtual int NumberOfCardsPlayablePerTurn { get{
			return new int[]{1, 2, 2, 3, 3, 4, 5 }[RevealedCardSpaces-1];
		}}
		
		#endregion

		#region intermediate growth states

		public int PowerCardsToDraw;

		#endregion

		public virtual void PlayAvailableCards(params int[] cards){

		}

		public void Grow(GameState gameState, int optionIndex, params IResolver[] resolvers){
			GrowthOption option = this.GetGrowthOptions(gameState)[optionIndex];
			
			// modify the growth option to resolve incomplete states
			foreach(var resolver in resolvers)
				resolver.Apply(option);

			option.Apply();
		}

		public abstract GrowthOption[] GetGrowthOptions(GameState gameState);

		public virtual int Elements(Element _) => 0;

	}

	public enum Element{ None, Air, Fire, Sun };

}