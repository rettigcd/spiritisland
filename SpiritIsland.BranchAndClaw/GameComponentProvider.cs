namespace SpiritIsland.BranchAndClaw;

public class GameComponentProvider : IGameComponentProvider {
	// Not using reflection because types inside this assembly are static
	// AND
	// It make start-up scanning slow

	static Dictionary<string, Type> Spirits => new() {
		[Keeper.Name] = typeof( Keeper ),
		[SharpFangs.Name] = typeof( SharpFangs ),
	};
	public string[] SpiritNames => Spirits.Keys.ToArray();
	public Spirit MakeSpirit( string spiritName ) {
		return Spirits.ContainsKey( spiritName )
			? (Spirit)Activator.CreateInstance( Spirits[spiritName] )
			: null;
	}

	public string[] AdversaryNames => Array.Empty<string>();
	public IAdversary MakeAdversary( string adversaryName ) => null;


	public PowerCard[] MinorCards => new Type[] {
		typeof(AbsorbCorruption),
		typeof(AnimatedWrackroot),
		typeof(CallToFerocity),
		typeof(CallToTrade),
		typeof(ConfoundingMists),
		typeof(CyclesOfTimeAndTide),
		typeof(DisorientingLandscape),
		typeof(ElusiveAmbushes),
		typeof(FireInTheSky),
		typeof(FleshrotFever),
		typeof(GoldsAllure),
		typeof(HereThereBeMonsters),
		typeof(InfestedAquifers),
		typeof(InflameTheFiresOfLife),
		typeof(PoisonedDew),
		typeof(PortentsOfDisaster),
		typeof(PromisesOfProtection),
		typeof(ProwlingPanthers),
		typeof(RazorSharpUndergrowth),
		typeof(RenewingRain),
		typeof(RitesOfTheLandsRejection),
		typeof(ScourTheLand),
		typeof(SwarmingWasps),
		typeof(GuardianSerpents),
		typeof(PactOfTheJoinedHunt),
		typeof(SkyStretchesToShore),
		typeof(SpurOnWithWordsOfFire),
		typeof(TeemingRivers),
		typeof(TormentingRotFlies),
		typeof(TwilightFogBringsMadness),
		typeof(GrowthThroughSacrifice)
	}.Select( PowerCard.For ).ToArray();

	public PowerCard[] MajorCards => new Type[] {
		typeof(BloodwrackPlague),
		typeof(CastDownIntoTheBrinyDeep),
		typeof(DeathFallsGentlyFromOpenBlossoms),
		typeof(FireAndFlood),
		typeof(GrantHatredARavenousForm),
		typeof(InsatiableHungerOfTheSwarm),
		typeof(InstrumentsOfTheirOwnRuin),
		typeof(ManifestIncarnation),
		typeof(PentUpCalamity),
		typeof(PyroclasticFlow),
		typeof(SavageTransformation),
		typeof(SeaMonsters),
		typeof(SmotheringInfestation),
		typeof(StranglingFirevine),
		typeof(SweepIntoTheSea),
		typeof(FlowLikeWaterReachLikeAir),
		typeof(UnlockTheGatesOfDeepestPower),
		typeof(UnrelentingGrowth),
		typeof(TigersHunting),
		typeof(TwistedFlowersMurmurUltimatums),
		typeof(VolcanicEruption),
	}.Select( PowerCard.For ).ToArray();

	public IFearCard[] FearCards => new IFearCard[] {
		new DahanAttack(),
		new DahanThreaten(),
		new Demoralized(),
		new DepartTheDangerousLand(),
		new Discord(),
		new ExplorersAreReluctant(),
		new FleeThePestilentLand(),
		new ImmigrationSlows(),
		new Panic(),
		new PanickedByWildBeasts(),
		new PlanForDeparture(),
		new Quarantine(),
		new TooManyMonsters(),
		new TreadCarefully(),
		new Unrest(),
	};

	public IBlightCard[] BlightCards => new BlightCard[] {
		new AidFromLesserSpirits(),
		new APallUponTheLand(),
		new BackAgainstTheWall(),
		new DisintegratingEcosystem(),
		new ErosionOfWill(),
		new PromisingFarmlands(),
		new TippingPoint(),
	};

}