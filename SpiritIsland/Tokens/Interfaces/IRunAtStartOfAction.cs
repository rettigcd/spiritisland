namespace SpiritIsland;

public interface IRunAtStartOfAction {
	Task Start(ActionScope current);
}