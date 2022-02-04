using System;
using System.Linq;

namespace SpiritIsland.JaggedEarth {

	public class GameComponentProvider : IGameComponentProvider {
		// Not using reflection because types inside this assembly are static
		// AND
		// It make start-up scanning slow

		public Type[] Spirits => new Type[] {
			typeof(FracturedDaysSplitTheSky),
			typeof(GrinningTricksterStirsUpTrouble),
			typeof(LureOfTheDeepWilderness),
			typeof(ManyMindsMoveAsOne),
			typeof(ShiftingMemoryOfAges),
			typeof(ShroudOfSilentMist),
			typeof(StarlightSeeksItsForm),
			typeof(StonesUnyieldingDefiance),
			typeof(VengeanceAsABurningPlague),
			typeof(VolcanoLoomingHigh),
		};

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
			typeof(TheWoundedWildReturnsOnItsAssailants),
			typeof(ThicketsEruptWithEveryTouchOfBreeze),
			typeof(TreesRadiateCelestialBrilliance),
			typeof(UtterACurseOfDreadAndBone),
			typeof(VanishSoftlyAwayForgottonByAll),
			typeof(VoiceOfCommand),
			typeof(WallsOfRockAndThorn),
			typeof(WeaveTogetherTheFabricOfPlace),
		}.Select( PowerCard.For ).ToArray();

		public IFearOptions[] FearCards => new IFearOptions[] {
			new BesetByManyTroubles(),
			new DahanReclaimFishingGrounds(),
			new FleeFromDangerousLands(),
			new NervesFray(),
			new SenseOfDread(),
			new ThologicalStrife()
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

}
