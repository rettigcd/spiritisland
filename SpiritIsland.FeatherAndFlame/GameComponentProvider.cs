namespace SpiritIsland.FeatherAndFlame;

#nullable enable

public class GameComponentProvider : IGameComponentProvider {

	#region Spirits

	public string[] SpiritNames => [.. Spirits.Keys];

	public Spirit? MakeSpirit(string spiritName) {
		return Spirits.TryGetValue(spiritName, out Type? value)
			? (Spirit?)Activator.CreateInstance(value)
			: null;
	}

	static Dictionary<string, Type> Spirits => new() {
		[HeartOfTheWildfire.Name]       = typeof( HeartOfTheWildfire ),
		[SerpentSlumbering.Name]        = typeof( SerpentSlumbering ),
		[DownpourDrenchesTheWorld.Name] = typeof( DownpourDrenchesTheWorld ),
		[FinderOfPathsUnseen.Name]      = typeof( FinderOfPathsUnseen ),
	};

	#endregion Spirits

	#region Aspects

	public AspectConfigKey[] AspectNames => [
		// Heart of the Wildfire
		Transforming.Key,
	];

	public IAspect? MakeAspect(AspectConfigKey aspectName ) => aspectName.Aspect switch {
		// Heart of the Wildfire
		Transforming.Name => new Transforming(),
		_ => null,
	};

	#endregion Aspects

	#region Adversaries

	public string[] AdversaryNames => [Scotland.Name];

	public IAdversaryBuilder? MakeAdversary( string adversaryName ) => adversaryName switch {
		Scotland.Name => new Scotland(),
		_ => null
	};

	#endregion Adversaries

	#region Cards

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

	#endregion Cards

}