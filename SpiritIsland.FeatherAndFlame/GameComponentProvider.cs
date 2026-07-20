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
		// Serpent
		Locus.Key,
	];

	public IAspect? MakeAspect(AspectConfigKey aspectName ) => aspectName.Aspect switch {
		// Heart of the Wildfire
		Transforming.Name => new Transforming(),
		// Serpent
		Locus.Name => new Locus(),
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

	[ModuleInitializer]
	internal static void RegisterSerialization() {
		var provider = new GameComponentProvider();
		foreach( PowerCard card in provider.MinorCards ) PowerCardRegistry.Register( card );
		foreach( PowerCard card in provider.MajorCards ) PowerCardRegistry.Register( card );
		foreach( string spiritName in provider.SpiritNames ) {
			Spirit spirit = provider.MakeSpirit( spiritName )!;
			foreach( PowerCard card in spirit.Hand ) PowerCardRegistry.Register( card );
			foreach( InnatePower innate in spirit.InnatePowers ) InnatePowerRegistry.Register( innate );
		}
		GameComponentProviderSeeding.RegisterAspectExclusiveCards( provider );
		foreach( string adversaryName in provider.AdversaryNames ) AdversaryRegistry.Register( provider.MakeAdversary( adversaryName )! );
	}

}