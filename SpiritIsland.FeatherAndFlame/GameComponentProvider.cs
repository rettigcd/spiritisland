namespace SpiritIsland.FeatherAndFlame;

public class GameComponentProvider : IGameComponentProvider {

	static Dictionary<string, Type> Spirits => new() {
		[HeartOfTheWildfire.Name]       = typeof( HeartOfTheWildfire ),
		[SerpentSlumbering.Name]        = typeof( SerpentSlumbering ),
		[DownpourDrenchesTheWorld.Name] = typeof( DownpourDrenchesTheWorld ),
		[FinderOfPathsUnseen.Name]      = typeof( FinderOfPathsUnseen ),
	};
	public string[] SpiritNames => Spirits.Keys.ToArray();
	public Spirit MakeSpirit( string spiritName ) {
		return Spirits.TryGetValue( spiritName, out Type value ) 
			? (Spirit)Activator.CreateInstance( value )
			: null;
	}

	public string[] AdversaryNames => new string[] { Scotland.Name };
	public IAdversary MakeAdversary( string adversaryName ) => adversaryName switch {
		Scotland.Name => new Scotland(),
		_ => null
	};


	public PowerCard[] MinorCards => Array.Empty<PowerCard>();

	public PowerCard[] MajorCards => Array.Empty<PowerCard>();

	public IFearCard[] FearCards => new IFearCard[] {
		new AngryMobs(),
		new CommunitiesInDisarray(),
		new Depopulation(),
		new MimicTheDahan(),
		new SpreadingTimidity()
	};

	public IBlightCard[] BlightCards => Array.Empty<BlightCard>();

}