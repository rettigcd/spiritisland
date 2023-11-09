namespace SpiritIsland;

/// <summary>This is just a marker to not damage Dahan.</summary>
/// <remarks>Watched by the DahanBinding</remarks>
public interface IModifyDahanDamage {
	void Modify( DamagingTokens notification );
}

/// <summary>This is just a marker to not damage Invaders.</summary>
/// <remarks>Watched by the InvaderBinding</remarks>
public interface IStopInvaderDamage { }