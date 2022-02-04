namespace SpiritIsland;

public interface IGameComponentProvider {
	Type[] Spirits { get; }
	PowerCard[] MinorCards { get; }
	PowerCard[] MajorCards { get; }
	IFearOptions[] FearCards { get; }
	IBlightCard[] BlightCards { get; }
}