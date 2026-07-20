namespace SpiritIsland.Horizons;

public class GameComponentProvider : IGameComponentProvider {

	#region Spirits

	public string[] SpiritNames => [.. Spirits.Keys];

	public Spirit? MakeSpirit(string spiritName) {
		return Spirits.TryGetValue(spiritName, out Type? spiritType)
			? (Spirit?)Activator.CreateInstance(spiritType)
			: null;
	}

	static Dictionary<string, Type> Spirits => new() {
		[DevouringTeethLurkUnderfoot.Name] = typeof(DevouringTeethLurkUnderfoot),
		[EyesWatchFromTheTrees.Name]       = typeof(EyesWatchFromTheTrees),
		[FathomlessMudOfTheSwamp.Name]     = typeof(FathomlessMudOfTheSwamp),
		[RisingHeatOfStoneAndSand.Name]    = typeof(RisingHeatOfStoneAndSand),
		[SunBrightWhirlwind.Name]          = typeof(SunBrightWhirlwind),
	};

	#endregion Spirits

	#region Aspects

	public AspectConfigKey[] AspectNames => [
//		Lair.ConfigKey,
	];

	public IAspect? MakeAspect(AspectConfigKey aspectName) => aspectName.Aspect switch {
//		Lair.Name => new Lair(),
		_ => null
	};

	#endregion Aspects

	#region Adversaries

	public string[] AdversaryNames => [];

	public IAdversaryBuilder? MakeAdversary( string adversaryName ) => adversaryName switch {
		_ => null
	};

	#endregion Adversaries

	#region Cards

	public PowerCard[] MinorCards => new Type[] {
//		typeof(WeepForWhatIsLost),
	}.Select( PowerCard.For ).ToArray();

	public PowerCard[] MajorCards => new Type[] {
//		typeof(WeaveTogetherTheFabricOfPlace),
	}.Select( PowerCard.For ).ToArray();

	public IFearCard[] FearCards => [
//		new TheologicalStrife()
	];

	public BlightCard[] BlightCards => [
//		new UntendedLandCrumbles(),
	];
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