using System.Collections.Generic;

namespace SpiritIsland {
	public class PlayerState {
		public List<PowerCard> PlayedCards = new List<PowerCard>();
		public List<PowerCard> AvailableCards = new List<PowerCard>();
		public List<BoardSpace> Presence = new List<BoardSpace>();
		public int Energy;

		// intermediate states
		public int PowerCardsToDraw;

		public List<IPresenceCriteria> PresenceToPlace = new List<IPresenceCriteria>();
	}

}
