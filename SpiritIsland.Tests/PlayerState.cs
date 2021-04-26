using System.Collections.Generic;

namespace SpiritIsland {

	public class PlayerState {

		protected Spirit Spirit {get;}

		public PlayerState(Spirit spirit){
			this.Spirit = spirit;
		}

		public List<PowerCard> AvailableCards = new List<PowerCard>();
		public List<PowerCard> PlayedCards = new List<PowerCard>();

		public List<Space> Presence = new List<Space>();
		public int Energy {get; set; }

		public int NumberOfCardsPlayablePerTurn { get; set; }


		// intermediate states
		public int PowerCardsToDraw;
		public List<IPresenceCriteria> PresenceToPlace = new List<IPresenceCriteria>();

		public virtual void PlayAvailableCards(params int[] cards){

		}

		public void Grow(int option) => Spirit.Grow(this, option );

	}

}
