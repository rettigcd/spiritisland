namespace SpiritIsland;

public interface IModifyAvailableActions { 
	void Modify(List<IActionFactory> orig, Phase phase);
}
