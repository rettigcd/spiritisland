namespace SpiritIsland;

/// <summary>
/// True while the engine is only working out where powers COULD target (to offer actions together with their targets), not
/// letting the user choose one. Anything that remembers what a targeting calculation offered, to charge or mark something
/// when the user then picks from it (Reach Through the Ephemeral Distance), must not remember it during a preview.
/// </summary>
public static class TargetPreview {

	static readonly AsyncLocal<bool> _active = new();

	public static bool IsActive { get => _active.Value; internal set => _active.Value = value; }

}
