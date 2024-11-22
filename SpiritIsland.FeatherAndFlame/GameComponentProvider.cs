namespace SpiritIsland.FeatherAndFlame;

public class GameComponentProvider : IGameComponentProvider {

	static Dictionary<string, Type> Spirits => new() {
		[HeartOfTheWildfire.Name]       = typeof( HeartOfTheWildfire ),
		[SerpentSlumbering.Name]        = typeof( SerpentSlumbering ),
		[DownpourDrenchesTheWorld.Name] = typeof( DownpourDrenchesTheWorld ),
		[FinderOfPathsUnseen.Name]      = typeof( FinderOfPathsUnseen ),
	};
	public string[] SpiritNames => [.. Spirits.Keys];
	public Spirit MakeSpirit( string spiritName ) {
		return Spirits.TryGetValue( spiritName, out Type value ) 
			? (Spirit)Activator.CreateInstance( value )
			: null;
	}

	public string[] AdversaryNames => [Scotland.Name];
	public IAdversary MakeAdversary( string adversaryName ) => adversaryName switch {
		Scotland.Name => new Scotland(),
		_ => null
	};


	public PowerCard[] MinorCards => [];

	public PowerCard[] MajorCards => [];

	public IFearCard[] FearCards => [
		new AngryMobs(),
		new CommunitiesInDisarray(),
		new Depopulation(),
		new MimicTheDahan(),
		new SpreadingTimidity()
	];

	public BlightCard[] BlightCards => [];

}