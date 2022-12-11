namespace SpiritIsland;

public interface IGameComponentProvider {
	Type[] Spirits { get; }
	PowerCard[] MinorCards { get; }
	PowerCard[] MajorCards { get; }
	IFearCard[] FearCards { get; }
	IBlightCard[] BlightCards { get; }
}