namespace SpiritIsland;

public interface IHandleActivatedActions : ISpiritMod {
	void ActionActivated(IActionFactory factory);
}
