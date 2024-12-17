namespace SpiritIsland;

/// <summary> Run on a Spirit when TimePasses </summary>
/// <remarks> 
/// Does not automatically remove from spirit space.
/// To remove from spirit, call spirit.Mods.Remove(this)  (instead of IEndWhenTimePasses to keep interface clutter to a minimum)
/// </remarks>
public interface ICleanupSpiritWhenTimePasses : ISpiritMod {
	/// <remarks> To remove from space, call space.Init(this,0); </remarks>
	void CleanupSpirit(Spirit spirit);
}
