namespace SpiritIsland;
public interface IDestroyPresenceBehavour {
	public Task DestroyPresenceApi(SpiritPresence presence, Space space, GameState gs, int count, DestoryPresenceCause actionType, UnitOfWork actionId );
}
