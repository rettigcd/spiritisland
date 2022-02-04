namespace SpiritIsland.BranchAndClaw;

public class GameComponentProvider : IGameComponentProvider {
	// Not using reflection because types inside this assembly are static
	// AND
	// It make start-up scanning slow

	public Type[] Spirits => new Type[] {
		typeof(Keeper),
		typeof(SharpFangs),
	};

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

	public IFearOptions[] FearCards => new IFearOptions[] {
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

	public IBlightCard[] BlightCards => new BlightCardBase[] {
		new AidFromLesserSpirits(),
		new APallUponTheLand(),
		new BackAgainstTheWall(),
		new DisintegratingEcosystem(),
		new ErosionOfWill(),
		new PromisingFarmlands(),
		new TippingPoint(),
	};

}