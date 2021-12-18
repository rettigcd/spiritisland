using System.Threading.Tasks;

namespace SpiritIsland {
	public interface IDestroyPresenceBehavour {
		public Task DestroyPresenceApi(SpiritPresence presence, Space space, GameState gs, ActionType actionType );
	}

}