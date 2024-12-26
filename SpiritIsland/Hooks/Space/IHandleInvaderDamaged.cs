namespace SpiritIsland;

public interface IHandleInvaderDamaged {
	void HandleDamage(HumanToken before, HumanToken after, Space space);
}