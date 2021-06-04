using System.Collections.Generic;
using System.Linq;

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
		protected virtual int[] EnergySequence => new int[]{0};
		protected virtual int[] CardSequence => new int[]{0}; 
		public int EnergyGrowth => EnergySequence[RevealedEnergySpaces-1];
		public virtual int NumberOfCardsPlayablePerTurn => CardSequence[RevealedCardSpaces-1];
		
		#endregion

		#region intermediate growth states

		public int PowerCardsToDraw;

		#endregion

		public virtual void PlayAvailableCards(params int[] cards){

		}

		public void Grow(GameState gameState, int optionIndex){
			GrowthOption option = this.GetGrowthOptions(gameState)[optionIndex];

			var actions = option.GrowthActions.ToList();

			// process actions that are already resolved
			var resolved = option.GrowthActions
				.Where(x=>x.IsResolved)
				.ToArray();

			foreach(var action in resolved){
				action.Apply();
				actions.Remove(action);
			}

			UnresolvedActions.AddRange( actions );

		}

		public List<GrowthAction> UnresolvedActions = new List<GrowthAction>();


		public abstract GrowthOption[] GetGrowthOptions(GameState gameState);

		public virtual int Elements(Element _) => 0;

	}

}