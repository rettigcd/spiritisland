using SpiritIsland.Basegame;
using System.Linq;

namespace SpiritIsland.Tests.Basegame.Spirits.Thunder {

	public class ThunderCards {

		public ThunderCards() { 

			spirit = new Thunderspeaker { Energy = 20 };
			User = new VirtualUser( spirit );

			// Given: empty board
			a = Board.BuildBoardA();
			gs = new GameState( spirit, a );

			// And: Spirit in spot 1
			spirit.Presence.PlaceOn( a[1] );

			action = spirit.Action;
		}

		protected void When_ActivateCard( string cardName ) {
			spirit.Hand.Single( x => x.Name == cardName ).ActivateAsync( spirit, gs);
		}

		protected readonly Spirit spirit;
		protected readonly VirtualUser User;
		protected readonly Board a;
		protected readonly GameState gs;
		protected readonly ActionGateway action;

	}
}
