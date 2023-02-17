namespace SpiritIsland.Basegame;

public class GameComponentProvider : IGameComponentProvider {

	static Dictionary<string, Type> SpiritTypes => new() {
		[RiverSurges.Name]           = typeof( RiverSurges ),
		[LightningsSwiftStrike.Name] = typeof( LightningsSwiftStrike ),
		[VitalStrength.Name]         = typeof( VitalStrength ),
		[Shadows.Name]               = typeof( Shadows ),
		[ASpreadOfRampantGreen.Name] = typeof( ASpreadOfRampantGreen ),
		[Thunderspeaker.Name]        = typeof( Thunderspeaker ),
		[Ocean.Name]                 = typeof( Ocean ),
		[Bringer.Name]               = typeof( Bringer ),
	};
	public string[] SpiritNames => SpiritTypes.Keys.ToArray();
	public Spirit MakeSpirit( string spiritName ) {
		return SpiritTypes.ContainsKey( spiritName )
			? (Spirit)Activator.CreateInstance( SpiritTypes[spiritName] )
			: null;
	}

	static Dictionary<string, Type> AdversariesTypes => new() {
		[BrandenburgPrussia.Name] = typeof( BrandenburgPrussia ),
		[England.Name] = typeof( England ),
		[Sweden.Name] = typeof( Sweden ),
	};

	public string[] AdversaryNames => AdversariesTypes.Keys.ToArray();
	public IAdversary MakeAdversary( string adversaryName ) => adversaryName != null && AdversariesTypes.ContainsKey(adversaryName )
			? (IAdversary) Activator.CreateInstance( AdversariesTypes[adversaryName] )
			: null;

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

	public IFearCard[] FearCards => new IFearCard[] {
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

	public IBlightCard[] BlightCards => new BlightCard[] {
		new DownwardSpiral(),
		new MemoryFadesToDust(),
	};

}