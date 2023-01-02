namespace SpiritIsland.JaggedEarth;

public class GameComponentProvider : IGameComponentProvider {

	public string[] AdversaryNames => new string[]{
		Russia.Name
	};

	public IAdversary MakeAdversary( string adversaryName ) => adversaryName switch {
		Russia.Name => new Russia(),
		_ => null
	};

	static Dictionary<string,Type> Spirits => new(){
		[FracturedDaysSplitTheSky.Name]        = typeof( FracturedDaysSplitTheSky),
		[GrinningTricksterStirsUpTrouble.Name] = typeof( GrinningTricksterStirsUpTrouble),
		[LureOfTheDeepWilderness.Name]         = typeof(LureOfTheDeepWilderness),
		[ManyMindsMoveAsOne.Name]              = typeof(ManyMindsMoveAsOne),
		[ShiftingMemoryOfAges.Name]            = typeof(ShiftingMemoryOfAges),
		[ShroudOfSilentMist.Name]              = typeof(ShroudOfSilentMist),
		[StarlightSeeksItsForm.Name]           = typeof(StarlightSeeksItsForm),
		[StonesUnyieldingDefiance.Name]        = typeof(StonesUnyieldingDefiance),
		[VengeanceAsABurningPlague.Name]       = typeof(VengeanceAsABurningPlague),
		[VolcanoLoomingHigh.Name]              = typeof(VolcanoLoomingHigh),
	};
	public string[] SpiritNames => Spirits.Keys.ToArray();
	public Spirit MakeSpirit( string spiritName ) {
		return Spirits.ContainsKey( spiritName )
			? (Spirit)Activator.CreateInstance( Spirits[spiritName] )
			:null;
	}

	public PowerCard[] MinorCards => new Type[] {
		typeof(BatsScoutForRaidsByDarkness),
		typeof(BirdsCryWarning),
		typeof(BloodDrawsPredators),
		typeof(CallToGuard),
		typeof(CarapacedLand),
		typeof(DesiccatingWinds),
		typeof(DireMetamorphosis),
		typeof(DomesticatedAnimalsGoBerserk),
		typeof(DryWoodExplodesInSmolderingSplinters),
		typeof(EntrapTheForcesOfCorruption),
		typeof(FavorOfTheSunAndStarlitDark),
		typeof(FlowDownriverBlowDownwind),
		typeof(HauntedByPrimalMemories),
		typeof(HazardsSpreadAcrossTheIsland),
		typeof(LikeCallsToLike),
		typeof(MesmerizedTranquility),
		typeof(SearAngerIntoTheWildLands),
		typeof(SetThemOnAnEverTwistingTrail),
		typeof(SkiesHeraldTheSeasonOfReturn),
		typeof(GiftOfNaturesConnection),
		typeof(GiftOfTwinnedDays),
		typeof(RenewingBoon),
		typeof(ScreamDiseaseIntoTheWind),
		typeof(StrongAndConstantCurrents),
		typeof(SuckingOoze),
		typeof(SunsetsFireFlowsAcrossTheLand),
		typeof(TerritorialStrife),
		typeof(TerrorTurnsToMadness),
		typeof(TheShoreSeethesWithHatred),
		typeof(ThrivingChokefungus),
		typeof(TreacherousWaterways),
		typeof(UnquenchableFlames),
		typeof(WeepForWhatIsLost),
	}.Select( PowerCard.For ).ToArray();

	public PowerCard[] MajorCards => new Type[] {
		typeof(AngryBears),
		typeof(BargainsOfPowerAndProtection),
		typeof(DrawTowardsAConsumingVoid),
		typeof(DreamOfTheUntouchedLand),
		typeof(FocusTheLandsAnguish),
		typeof(ForestsOfLivingObsidian),
		typeof(InfestationOfVenomousSpiders),
		typeof(IrresistibleCall),
		typeof(MeltEarthIntoQuicksand),
		typeof(SleepAndNeverWaken),
		typeof(SpillBitternessIntoTheEarth),
		typeof(SettleIntoHuntingGrounds),
		typeof(TransformToAMurderousDarkness),
		typeof(UnleashATorrentOfTheSelfsOwnEssence),
		typeof(StormSwath),
		typeof(TheWoundedWildTurnsOnItsAssailants),
		typeof(ThicketsEruptWithEveryTouchOfBreeze),
		typeof(TreesRadiateCelestialBrilliance),
		typeof(UtterACurseOfDreadAndBone),
		typeof(VanishSoftlyAwayForgottonByAll),
		typeof(VoiceOfCommand),
		typeof(WallsOfRockAndThorn),
		typeof(WeaveTogetherTheFabricOfPlace),
	}.Select( PowerCard.For ).ToArray();

	public IFearCard[] FearCards => new IFearCard[] {
		new BesetByManyTroubles(),
		new DahanReclaimFishingGrounds(),
		new FleeFromDangerousLands(),
		new NervesFray(),
		new SenseOfDread(),
		new TheologicalStrife()
	};

	public IBlightCard[] BlightCards => new BlightCardBase[] {
		new AllThingsWeaken(),
		new InvadersFindTheLandToTheirLiking(),
		new PowerCorrodesTheSpirit(),
		new StrongEarthShattersSlowly(),
		new ThrivingCommunitites(),
		new UnnaturalProliferation(),
		new UntendedLandCrumbles(),
	};

}