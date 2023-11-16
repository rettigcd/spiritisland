namespace SpiritIsland.JaggedEarth;

public class VolcanoLoomingHigh : Spirit {

	public const string Name = "Volcano Looming High";
	public const string CollapseInABlastOfLavaAndSteam = "Collapse in a Blast of Lava and Steam";
	public const string MountainHome = "Mountain Home";

	public override string Text => Name;

	#region SpecialRules
	public override SpecialRule[] SpecialRules => new SpecialRule[]{
		MountainHome_Rule,
		CollapseInABlastOfLAvaAndSteam_Rule,
		VolcanicPeaksTowerOverTheLandscape.Rule
	};
	static SpecialRule MountainHome_Rule => new SpecialRule( MountainHome, "Your presence may only be added/moved into Mountain.");
	static SpecialRule CollapseInABlastOfLAvaAndSteam_Rule => new SpecialRule( CollapseInABlastOfLavaAndSteam,"When your presense is destroyed, in that land, deal 1 Damage per destroyed presence to both Invaders and to dahan.");
	#endregion

	public VolcanoLoomingHigh():base(
		new VolcanoPresence(
			new PresenceTrack(Track.Energy1,Track.Energy2,Track.EarthEnergy,Track.Energy3,Track.Energy4,Track.Energy5),
			new PresenceTrack(Track.Card1,Track.MkCard(Element.Fire),Track.MkCard(Element.Earth),Track.Card2, Track.MkCard( Element.Air ), Track.Card3, Track.MkCard( Element.Fire ), Track.Card4)
		)
		,PowerCard.For<ExaltationOfMoltenStone>()
		,PowerCard.For<LavaFlows>()
		,PowerCard.For<PyroclasticBombardment>()
		,PowerCard.For<RainOfAsh>()
	) {
		GrowthTrack = new GrowthTrack(
			new GrowthOption(new ReclaimAll(), new GainPowerCard(), new GainEnergy(3)),
			new GrowthOption(new PlacePresence(0,Target.Mountain), new PlacePresence(0,Target.Mountain)),
			new GrowthOption(new GainPowerCard(), new PlacePresence(4,Target.Mountain), new PlayExtraCardThisTurn(1), new GainEnergy(2))
		);

		InnatePowers = new InnatePower[] {
			InnatePower.For(typeof(ExplosiveEruption)), 
			InnatePower.For(typeof(PoweredByTheFurnaceOfTheEarth))
		};

		PowerRangeCalc = new VolcanicPeaksTowerOverTheLandscape(this);
	}

	protected override void InitializeInternal( Board board, GameState gameState ) {
		// init special growth (note - we don't want this growth in Unit tests, so only add it if we call InitializeInternal())
		this.AddActionFactory(new PlacePresenceOnMountain().ToInit());
	}

	public override async Task<IDrawableInnateTier> SelectInnateTierToActivate( IEnumerable<IDrawableInnateTier> innateOptions ) {

		IDrawableInnateTier match = null;
		int destroyedThisAction = VolcanoPresence.GetPresenceDestroyedThisAction();
		foreach(var option in innateOptions.OrderBy( o => o.Elements.Total )) {
			if(option is ExplosiveInnateOptionAttribute ex && destroyedThisAction < ex.DestroyedPresenceThreshold)
				continue;

			if(await HasElements( option.Elements ))
				match = option;
		}
		return match;
	}


}
