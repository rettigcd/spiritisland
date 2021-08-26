using SpiritIsland;

namespace SpiritIsland {

	// ! this could split up into 3 Extension files
	// * GatherExtensions
	// * PushExtesnsions
	// * PlacePresenceExtensions

	public interface IMakeGamestateDecisions {
		Spirit Self { get; }
		GameState GameState { get; }
	}

}