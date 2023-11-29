using SpiritIsland.SinglePlayer;

namespace SpiritIsland.Tests.Spirits.River; 

public class RiverGame {

	public RiverGame() {
		_spirit = new RiverSurges();
		User = new VirtualRiverUser( _spirit );
	}

	protected readonly Spirit _spirit;
	protected readonly VirtualRiverUser User;
}

