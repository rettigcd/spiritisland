namespace SpiritIsland;

/// <summary>
/// Used by the Generic Pickers to find the decision making spirit in BoardCtx, TargetSpaceCtx, TargetSpiritCtx
/// </summary>
public interface IHaveSpirit {
	Spirit Self { get; }
}