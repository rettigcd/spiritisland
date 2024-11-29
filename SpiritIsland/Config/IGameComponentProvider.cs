namespace SpiritIsland;

#nullable enable

public interface IGameComponentProvider {

	string[] SpiritNames { get; }
	Spirit? MakeSpirit( string spiritName );

	AspectConfigKey[] AspectNames { get; }
	IAspect? MakeAspect(AspectConfigKey aspectName);

	string[] AdversaryNames { get; }
	IAdversaryBuilder? MakeAdversary(string adversaryName);

	PowerCard[] MinorCards { get; }
	PowerCard[] MajorCards { get; }
	IFearCard[] FearCards { get; }
	BlightCard[] BlightCards { get; }

}