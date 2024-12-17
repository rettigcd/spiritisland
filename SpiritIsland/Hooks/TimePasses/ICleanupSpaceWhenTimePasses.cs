namespace SpiritIsland;

/// <summary> Run on a space when TimePasses </summary>
/// <remarks> 
/// Does not automatically remove from space.
/// To remove from space, call space.Init(this,0) (instead of IEndWhenTimePasses to keep interface clutter to a minimum)
/// </remarks>
public interface ICleanupSpaceWhenTimePasses : ISpaceEntity {
	/// <remarks> To remove from space, call space.Init(this,0); </remarks>
	void CleanupSpace(Space space);
}
