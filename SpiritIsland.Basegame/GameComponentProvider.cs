namespace SpiritIsland.Basegame;

public class GameComponentProvider : IGameComponentProvider {
	// Not using reflection because types inside this assembly are static
	// AND
	// It make start-up scanning slow
	// !! Maybe these should just be types.  Until they are actually selected into the game

	public Type[] Spirits => new Type[] {
		typeof(RiverSurges),
		typeof(LightningsSwiftStrike),
		typeof(VitalStrength),
		typeof(Shadows),
		typeof(ASpreadOfRampantGreen),
		typeof(Thunderspeaker),
		typeof(Ocean),
		typeof(Bringer),
	};

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

	public IFearOptions[] FearCards => new IFearOptions[] {
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
	};

	public IBlightCard[] BlightCards => new BlightCardBase[] {
		new DownwardSpiral(),
		new MemoryFadesToDust(),
	};

}