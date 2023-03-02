namespace SpiritIsland.Tests.Spirits.Thunder;

public class ThunderCards {

	public ThunderCards() { 

		spirit = new Thunderspeaker { Energy = 20 };
		User = new VirtualUser( spirit );

		// Given: empty board
		a = Board.BuildBoardA();
		gs = new GameState( spirit, a );

		// And: Spirit in spot 1
		spirit.Presence.When_PlacingOn(a[1]);

		action = spirit.Gateway;
	}

	protected readonly Spirit spirit;
	protected readonly VirtualUser User;
	protected readonly Board a;
	protected readonly GameState gs;
	protected readonly UserGateway action;

}
