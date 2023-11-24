namespace SpiritIsland;

public interface IRunAfterInvaderPhase {
	Task ActAsync(SpaceState space);
}