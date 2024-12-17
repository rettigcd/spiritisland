namespace SpiritIsland;

public interface IModifyAvailableActions : ISpiritMod { 
	void Modify(List<IActionFactory> orig, Phase phase);
}
