namespace SpiritIsland;

public interface IGameComponentProvider {

	string[] SpiritNames { get; }
	Spirit MakeSpirit( string spiritName );

	PowerCard[] MinorCards { get; }
	PowerCard[] MajorCards { get; }
	IFearCard[] FearCards { get; }
	IBlightCard[] BlightCards { get; }
}