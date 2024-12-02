using SpiritIsland.NatureIncarnate;

namespace SpiritIsland.Tests.Spirits.ToweringRoots;

/// <summary> Base class for writing tests against Towering Roots of the Jungle (Board-A)</summary>
public class ToweringRoots_Base {
	public ToweringRoots_Base() {
		_spirit = new ToweringRootsOfTheJungle();
		_presence = _spirit.Presence;
		_board = Boards.A;
		_gs = new SoloGameState( _spirit, _board );
	}

	// Given: spirit with Incarna
	readonly protected ToweringRootsOfTheJungle _spirit;
	readonly protected SpiritPresence _presence;
	readonly protected Board _board;
	readonly protected GameState _gs;

	protected void Given_IncarnaOn( SpaceSpec space ) {
		space.ScopeSpace.Init( _presence.Incarna, 1 );
	}

}
