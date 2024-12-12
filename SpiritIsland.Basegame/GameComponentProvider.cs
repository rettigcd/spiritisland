using SpiritIsland.Basegame.Spirits.Lightning.Aspects;

namespace SpiritIsland.Basegame;

#nullable enable

public class GameComponentProvider : IGameComponentProvider {

	#region Spirits

	public string[] SpiritNames => [.. SpiritTypes.Keys];

	public Spirit? MakeSpirit( string spiritName ) {
		return SpiritTypes.TryGetValue( spiritName, out Type? spiritType ) 
			? (Spirit?)Activator.CreateInstance( spiritType )
			: null;
	}

	static Dictionary<string, Type> SpiritTypes => new() {
		[RiverSurges.Name] = typeof(RiverSurges),
		[LightningsSwiftStrike.Name] = typeof(LightningsSwiftStrike),
		[VitalStrength.Name] = typeof(VitalStrength),
		[Shadows.Name] = typeof(Shadows),
		[ASpreadOfRampantGreen.Name] = typeof(ASpreadOfRampantGreen),
		[Thunderspeaker.Name] = typeof(Thunderspeaker),
		[Ocean.Name] = typeof(Ocean),
		[Bringer.Name] = typeof(Bringer),
	};

	#endregion Spirits

	#region Aspects

	public AspectConfigKey[] AspectNames => [
		// River
		Haven.ConfigKey,
		Sunshine.ConfigKey,
		Travel.ConfigKey,
		// Ocean
		Deeps.ConfigKey,
		// Lightning
		Pandemonium.ConfigKey,
		Sparking.ConfigKey,
		Wind.ConfigKey,
		Immense.ConfigKey,
		// Shadow
		Amorphous.ConfigKey,
		DarkFire.ConfigKey,
		Foreboding.ConfigKey,
		Madness.ConfigKey,
		Reach.ConfigKey,
	];

	public IAspect? MakeAspect(AspectConfigKey aspectName) => aspectName.Aspect switch {
		// River
		Haven.Name => new Haven(),
		Sunshine.Name => new Sunshine(),
		Travel.Name => new Travel(),
		// Ocean
		Deeps.Name => new Deeps(),
		// Lightning
		Pandemonium.Name => new Pandemonium(),
		Sparking.Name => new Sparking(),
		Wind.Name => new Wind(),
		Immense.Name => new Immense(),
		// Shadows
		Amorphous.Name => new Amorphous(),
		DarkFire.Name => new DarkFire(),
		Foreboding.Name => new Foreboding(),
		Madness.Name => new Madness(),
		Reach.Name => new Reach(),
		_ => null,
	};

	#endregion Aspects

	#region Adversaries

	public string[] AdversaryNames => [.. AdversariesTypes.Keys];

	public IAdversaryBuilder? MakeAdversary( string adversaryName ) => adversaryName != null && AdversariesTypes.TryGetValue( adversaryName, out Type? adversaryType ) 
		? (IAdversaryBuilder?) Activator.CreateInstance( adversaryType )
			: null;

	static Dictionary<string, Type> AdversariesTypes => new() {
		[BrandenburgPrussia.Name] = typeof(BrandenburgPrussia),
		[England.Name] = typeof(England),
		[Sweden.Name] = typeof(Sweden),
	};

	#endregion Adversaries

	#region Cards

	public PowerCard[] MinorCards => new Type[] {
		typeof(CallOfTheDahanWays),
		typeof(CallToBloodshed),
		typeof(CallToIsolation),
		typeof(CallToMigrate),
		typeof(CallToTend),
		typeof(DarkAndTangledWoods),
		typeof(DelusionsOfDanger),
		typeof(DevouringAnts),
		typeof(DriftDownIntoSlumber),
		typeof(Drought),
		typeof(EnticingSplendor),
		typeof(EntrancingApparitions),
		typeof(GnawingRootbiters),
		typeof(LandOfHauntsAndEmbers),
		typeof(LureOfTheUnknown),
		typeof(NaturesResilience),
		typeof(PullBeneathTheHungryEarth),
		typeof(PurifyingFlame),
		typeof(QuickenTheEarthsStruggles),
		typeof(RainOfBlood),
		typeof(RouseTheTreesAndStones),
		typeof(SapTheStrengthOfMultitudes),
		typeof(SavageMawbeasts),
		typeof(ShadowsOfTheBurningForest),
		typeof(SongOfSanctity),
		typeof(SteamVents),
		typeof(ElementalBoon),
		typeof(EncompassingWard),
		typeof(GiftOfConstancy),
		typeof(GiftOfLivingEnergy),
		typeof(GiftOfPower),
		typeof(ReachingGrasp),
		typeof(UncannyMelting),
		typeof(VeilTheNightsHunt),
		typeof(VisionsOfFieryDoom),
		typeof(VoraciousGrowth),
	}.Select( PowerCard.For ).ToArray();

	public PowerCard[] MajorCards => new Type[] {
		typeof(AcceleratedRot),
		typeof(CleansingFloods),
		typeof(DissolveTheBondsOfKinship),
		typeof(IndomitableClaim),
		typeof(InfiniteVitality),
		typeof(MistsOfOblivion),
		typeof(ParalyzingFright),
		typeof(PillarOfLivingFlame),
		typeof(PoisonedLand),
		typeof(TalonsOfLightning),
		typeof(BlazingRenewal),
		typeof(EntwinedPower),
		typeof(PowerStorm),
		typeof(TerrifyingNightmares),
		typeof(TheJungleHungers),
		typeof(TheLandThrashesInFuriousPain),
		typeof(TheTreesAndStonesSpeakOfWar),
		typeof(Tsunami),
		typeof(VengeanceOfTheDead),
		typeof(VigorOfTheBreakingDawn),
		typeof(WindsOfRustAndAtrophy),
		typeof(WrapInWingsOfSunlight),
	}.Select( PowerCard.For ).ToArray();

	public IFearCard[] FearCards => [
		new AvoidTheDahan(),
		new BeliefTakesRoot(),
		new DahanEnheartened(),
		new DahanOnTheirGuard(),
		new DahanRaid(),
		new EmigrationAccelerates(),
		new FearOfTheUnseen(),
		new Isolation(),
		new OverseasTradeSeemsSafer(),
		new Retreat(),
		new Scapegoats(),
		new SeekSafety(),
		new TallTalesOfSavagery(),
		new TradeSuffers(),
		new WaryOfTheInterior(),
	];

	public BlightCard[] BlightCards => [
		new DownwardSpiral(),
		new MemoryFadesToDust(),
	];

	#endregion Cards

}