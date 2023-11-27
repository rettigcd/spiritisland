namespace SpiritIsland.NatureIncarnate;

public class GameComponentProvider : IGameComponentProvider {

	static Dictionary<string, Type> SpiritTypes => new() {
		[ToweringRootsOfTheJungle.Name] = typeof( ToweringRootsOfTheJungle ),
		[BreathOfDarknessDownYourSpine.Name] = typeof( BreathOfDarknessDownYourSpine ),
		[HearthVigil.Name] = typeof( HearthVigil ),
		[WoundedWatersBleeding.Name] = typeof( WoundedWatersBleeding ),
		[DancesUpEarthquakes.Name] = typeof(DancesUpEarthquakes ),
		[RelentlessGazeOfTheSun.Name] = typeof(RelentlessGazeOfTheSun ),
		[EmberEyedBehemoth.Name] = typeof( EmberEyedBehemoth ),
		[WanderingVoiceKeensDelirium.Name] = typeof( WanderingVoiceKeensDelirium ),
	};

	public string[] SpiritNames => SpiritTypes.Keys.ToArray();
	public Spirit? MakeSpirit( string spiritName ) {
		return SpiritTypes.ContainsKey( spiritName )
			? (Spirit?)Activator.CreateInstance( SpiritTypes[spiritName] )
			: null;
	}

	static Dictionary<string, Type> AdversariesTypes => new() {
		//[BrandenburgPrussia.Name] = typeof( BrandenburgPrussia ),
		//[England.Name] = typeof( England ),
		//[Sweden.Name] = typeof( Sweden ),
	};

	public string[] AdversaryNames => AdversariesTypes.Keys.ToArray();
	public IAdversary? MakeAdversary( string adversaryName ) => adversaryName != null && AdversariesTypes.ContainsKey( adversaryName )
			? (IAdversary?)Activator.CreateInstance( AdversariesTypes[adversaryName] )
			: null;

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
	}.Select( PowerCard.For ).ToArray();

	public IFearCard[] FearCards => new IFearCard[] {
//		new AvoidTheDahan(),
	};

	public IBlightCard[] BlightCards => new BlightCard[] {
//		new DownwardSpiral(),
	};

}


