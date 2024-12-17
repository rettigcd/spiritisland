namespace SpiritIsland;

/// <summary>This is just a marker to not damage Dahan.</summary>
/// <remarks>Watched by the DahanBinding</remarks>
public interface IAdjustDamageToDahan {
	void Modify( DamagingTokens notification );
}
