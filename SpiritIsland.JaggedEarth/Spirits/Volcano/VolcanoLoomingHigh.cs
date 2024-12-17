namespace SpiritIsland.JaggedEarth;

public class VolcanoLoomingHigh : Spirit {

	public const string Name = "Volcano Looming High";
	public const string CollapseInABlastOfLavaAndSteam = "Collapse in a Blast of Lava and Steam";
	public const string MountainHome = "Mountain Home";

	public override string SpiritName => Name;

	#region SpecialRules

	static SpecialRule MountainHome_Rule => new SpecialRule( MountainHome, "Your presence may only be added/moved into Mountain.");
	static SpecialRule CollapseInABlastOfLAvaAndSteam_Rule => new SpecialRule( CollapseInABlastOfLavaAndSteam,"When your presense is destroyed, in that land, deal 1 Damage per DestroyedPresence to both Invaders and to dahan.");
	#endregion

	public VolcanoLoomingHigh():base(
		spirit => new VolcanoPresence( spirit,
			new PresenceTrack(Track.Energy1,Track.Energy2,Track.EarthEnergy,Track.Energy3,Track.Energy4,Track.Energy5),
			new PresenceTrack(Track.Card1,Track.MkCard(Element.Fire),Track.MkCard(Element.Earth),Track.Card2, Track.MkCard( Element.Air ), Track.Card3, Track.MkCard( Element.Fire ), Track.Card4)
		)
		,new GrowthTrack(
			new GrowthGroup( new ReclaimAll(), new GainPowerCard(), new GainEnergy( 3 ) ),
			new GrowthGroup( new PlacePresence( 0, Filter.Mountain ), new PlacePresence( 0, Filter.Mountain ) ),
			new GrowthGroup( new GainPowerCard(), new PlacePresence( 4, Filter.Mountain ), new PlayExtraCardThisTurn( 1 ), new GainEnergy( 2 ) )
		)
		,PowerCard.ForDecorated(ExaltationOfMoltenStone.ActAsync)
		,PowerCard.ForDecorated(LavaFlows.ActAsync)
		,PowerCard.ForDecorated(PyroclasticBombardment.ActAsync)
		,PowerCard.ForDecorated(RainOfAsh.ActAsync)
	) {

		InnatePowers = [
			new ExplosiveInnate(typeof(ExplosiveEruption)), 
			InnatePower.For(typeof(PoweredByTheFurnaceOfTheEarth))
		];

		SpecialRules = [
			MountainHome_Rule,
			CollapseInABlastOfLAvaAndSteam_Rule,
			VolcanicPeaksTowerOverTheLandscape.Rule
		];

		PowerRangeCalc = new VolcanicPeaksTowerOverTheLandscape(this);
	}

	protected override void InitializeInternal( Board board, GameState gameState ) {
		// init special growth (note - we don't want this growth in Unit tests, so only add it if we call InitializeInternal())
		this.AddActionFactory(new PlacePresenceOnMountain().ToGrowth());
	}

}
