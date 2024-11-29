namespace SpiritIsland.NatureIncarnate;

public class GameComponentProvider : IGameComponentProvider {

	#region Spirits

	public string[] SpiritNames => [.. SpiritTypes.Keys];

	public Spirit? MakeSpirit( string spiritName ) {
		return SpiritTypes.TryGetValue( spiritName, out Type? spiritType ) 
			? (Spirit?)Activator.CreateInstance( spiritType )
			: null;
	}

	static Dictionary<string, Type> SpiritTypes => new() {
		[ToweringRootsOfTheJungle.Name] = typeof(ToweringRootsOfTheJungle),
		[BreathOfDarknessDownYourSpine.Name] = typeof(BreathOfDarknessDownYourSpine),
		[HearthVigil.Name] = typeof(HearthVigil),
		[WoundedWatersBleeding.Name] = typeof(WoundedWatersBleeding),
		[DancesUpEarthquakes.Name] = typeof(DancesUpEarthquakes),
		[RelentlessGazeOfTheSun.Name] = typeof(RelentlessGazeOfTheSun),
		[EmberEyedBehemoth.Name] = typeof(EmberEyedBehemoth),
		[WanderingVoiceKeensDelirium.Name] = typeof(WanderingVoiceKeensDelirium),
	};

	#endregion Spirits

	#region Aspects

	public AspectConfigKey[] AspectNames => [];

	public IAspect? MakeAspect(AspectConfigKey aspectName ) {
		return null;
	}

	#endregion Aspects

	#region Adversaries

	public string[] AdversaryNames => [.. AdversariesTypes.Keys];

	public IAdversaryBuilder? MakeAdversary( string adversaryName ) => adversaryName != null 
		&& AdversariesTypes.TryGetValue( adversaryName, out Type? adversaryType ) 
		? (IAdversaryBuilder?)Activator.CreateInstance( adversaryType )
			: null;

	static Dictionary<string, Type> AdversariesTypes => new() {
		[HabsburgMiningExpedition.Name] = typeof(HabsburgMiningExpedition),
	};

	#endregion Adversaries

	#region Cards

	public PowerCard[] MinorCards => new Type[] {
		typeof(RoilingBogAndSnaggingThorn),
		
	}.Select( PowerCard.For ).ToArray();

	public PowerCard[] MajorCards => new Type[] {
		typeof(BargainOfCoursingPaths),
		typeof(BombardWithBouldersAndStingingSeeds),
		typeof(ExaltationOfTheIncandescentSky),
		typeof(FlockingRedTalons),
		typeof(FragmentsOfYesterYear),
		typeof(InspireTheReleaseOfStolenLands),
		typeof(RumblingEarthquakes),
		typeof(SolidifyEchoesOfMajestyPast),
		typeof(TransformativeSacrifice),
		typeof(UnearthABeastOfWrathfulStone),
		typeof(RavagedUndergrowthSlithersBackToLife),
		typeof(PlagueShipsSailToDistantPorts),
	}.Select( PowerCard.For ).ToArray();

	public IFearCard[] FearCards => [
		new CivilUnrest(),
		new DahanGainTheEdge(),
		new DauntedByTheDahan(),
		new DistractedByLocalTroubles(),
		new Restlessness(),
		new SeekCompany(),
		new StrugglesOverFarmland(),
		new SupplyChainsAbandoned(),
		new Unsettled(),
	];

	public BlightCard[] BlightCards => [
		new AttenuatedEssence(),
		new BlightCorrodesTheSpirit(),
		new BurnBrightestBeforeTheEnd(),
		new IntensifyingExploitation(),
		new ShatteredFragmentsOfPower(),
		new SlowDissolutionOfWill(),
		new TheBorderOfLifeAndDeath(),
		new ThrivingCrops()
	];

	#endregion Cards

}