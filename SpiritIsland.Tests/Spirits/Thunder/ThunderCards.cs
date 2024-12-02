namespace SpiritIsland.Tests.Spirits.Thunder;

public class ThunderCards {

	public ThunderCards() { 

		spirit = new Thunderspeaker { Energy = 20 };
		User = new VirtualUser( spirit );

		// Given: empty board
		a = Boards.A;
		gs = new SoloGameState( spirit, a );

		// And: Spirit in spot 1
		spirit.Given_IsOn( a[1] );

//		_gateway = spirit.Gateway;
	}

	protected readonly Spirit spirit;
	protected readonly VirtualUser User;
	protected readonly Board a;
	protected readonly GameState gs;
//	protected readonly UserGateway _gateway;

}