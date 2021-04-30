using System.Collections.Generic;

namespace SpiritIsland {

	public abstract class Spirit {

		public List<PowerCard> AvailableCards = new List<PowerCard>();
		public List<PowerCard> PlayedCards = new List<PowerCard>();

		readonly List<Space> Presence = new List<Space>();
		public List<Space> CanPlacePresenceFrom => Presence;
		public void InitPresence(Space space) => Presence.Add(space);

		public int Energy {get; set; }
		public int NumberOfCardsPlayablePerTurn { get; set; }

		#region intermediate growth states

		public int PowerCardsToDraw;
		public List<IPresenceCriteria> PresenceToPlace = new List<IPresenceCriteria>();

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

	}

}