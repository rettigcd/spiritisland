namespace SpiritIsland.PromoPack1;

public class GameComponentProvider : IGameComponentProvider {

	static Dictionary<string, Type> Spirits => new() {
		[HeartOfTheWildfire.Name]       = typeof( HeartOfTheWildfire ),
		[SerpentSlumbering.Name]        = typeof( SerpentSlumbering ),
		[DownpourDrenchesTheWorld.Name] = typeof( DownpourDrenchesTheWorld ),
		[FinderOfPathsUnseen.Name]      = typeof( FinderOfPathsUnseen ),
	};
	public string[] SpiritNames => Spirits.Keys.ToArray();
	public Spirit MakeSpirit( string spiritName ) {
		return Spirits.ContainsKey( spiritName )
			? (Spirit)Activator.CreateInstance( Spirits[spiritName] )
			: null;
	}

	public string[] AdversaryNames => Array.Empty<string>();
	public IAdversary MakeAdversary( string adversaryName ) => null;


	public PowerCard[] MinorCards => Array.Empty<PowerCard>();

	public PowerCard[] MajorCards => Array.Empty<PowerCard>();

	public IFearCard[] FearCards => Array.Empty<IFearCard>();

	public IBlightCard[] BlightCards => Array.Empty<BlightCardBase>();

}