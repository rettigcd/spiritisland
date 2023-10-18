namespace SpiritIsland.NatureIncarnate;

public class GameComponentProvider : IGameComponentProvider {

	static Dictionary<string, Type> SpiritTypes => new() {
		[ToweringRootsOfTheJungle.Name] = typeof( ToweringRootsOfTheJungle ),
		[BreathOfDarknessDownYourSpine.Name] = typeof( BreathOfDarknessDownYourSpine ),
		[HearthVigil.Name] = typeof( HearthVigil ),
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
		//typeof(VisionsOfFieryDoom),
		//typeof(VoraciousGrowth),
	}.Select( PowerCard.For ).ToArray();

	public PowerCard[] MajorCards => new Type[] {
//		typeof(AcceleratedRot),
	}.Select( PowerCard.For ).ToArray();

	public IFearCard[] FearCards => new IFearCard[] {
//		new AvoidTheDahan(),
	};

	public IBlightCard[] BlightCards => new BlightCard[] {
//		new DownwardSpiral(),
	};

}

