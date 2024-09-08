namespace SpiritIsland;

public interface ICleanupSpaceWhenTimePasses 
	: ISpaceEntity
{
	void EndOfRoundCleanup(Space space);
}

/// <summary>
/// Tokens marked with this interface are automatically removed when Time Passes
/// </summary>
public interface IEndWhenTimePasses : ISpaceEntity { }
