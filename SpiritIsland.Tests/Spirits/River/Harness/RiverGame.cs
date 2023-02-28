using SpiritIsland.SinglePlayer;

namespace SpiritIsland.Tests.Spirits.River; 

public class RiverGame {

	public RiverGame() {
		spirit = new RiverSurges();
		User = new VirtualRiverUser( spirit );
	}

	protected SinglePlayerGame _game;
	protected readonly Spirit spirit;
	protected readonly VirtualRiverUser User;
}

