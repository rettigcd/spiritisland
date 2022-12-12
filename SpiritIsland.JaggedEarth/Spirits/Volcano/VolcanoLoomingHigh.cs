namespace SpiritIsland.JaggedEarth;

// !!! Mechanism for keeping presence in Mountain not in place for Power Card use

public class VolcanoLoomingHigh : Spirit {

	public const string Name = "Volcano Looming High";

	public override string Text => Name;

	public override SpecialRule[] SpecialRules => new SpecialRule[]{
		MountainHome, 
		CollapseInABlastOfLAvaAndSteam,
		VolcanicPeaksTowerOverTheLandscape.Rule
	};
	static readonly SpecialRule MountainHome = new SpecialRule("Mountain Home","Your presence may only be added/moved into Mountain.");
	static readonly SpecialRule CollapseInABlastOfLAvaAndSteam = new SpecialRule("Collapse in a Blast of Lava and Steam","When your presense is destroyed, in that land, deal 1 Damage per destroyed presence to both Invaders and to dahan.");

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
			new GrowthOption(new ReclaimAll(), new DrawPowerCard(), new GainEnergy(3)),
			new GrowthOption(new PlacePresence(0,Target.Mountain), new PlacePresence(0,Target.Mountain)),
			new GrowthOption(new DrawPowerCard(), new PlacePresence(4,Target.Mountain), new PlayExtraCardThisTurn(1), new GainEnergy(2))
		);

		InnatePowers = new InnatePower[] {
			InnatePower.For<ExplosiveEruption>(),
			InnatePower.For<PoweredByTheFurnaceOfTheEarth>()
		};

		PowerRangeCalc = new VolcanicPeaksTowerOverTheLandscape();
	}

	protected override void InitializeInternal( Board board, GameState gameState ) {
		// init special growth (note - we don't want this growth in Unit tests, so only add it if we call InitializeInternal())
		this.AddActionFactory(new Setup_PlacePresenceOnMountain());
	}

}

class VolcanoPresence : SpiritPresence {
	public VolcanoPresence(PresenceTrack t1, PresenceTrack t2 ) : base( t1, t2 ) {}

	public override bool CanBePlacedOn( SpaceState s, TerrainMapper tm ) => tm.MatchesTerrain( s, Terrain.Mountain );

	public override void SetSpirit( Spirit spirit ) {
		base.SetSpirit( spirit );
		DestroyBehavior = new DestroyPresence( (VolcanoLoomingHigh)spirit ); // ??? can't we just override instead of plugging this in.
	}

	class DestroyPresence : SpiritPresence.DefaultDestroyBehavior {
		readonly VolcanoLoomingHigh spirit;
		public DestroyPresence( VolcanoLoomingHigh spirit ) { this.spirit = spirit;}

		public override async Task DestroyPresenceApi( SpiritPresence presence, Space space, GameState gs, DestoryPresenceCause actionType, UnitOfWork actionId ) {
			await base.DestroyPresenceApi( presence, space, gs, actionType, actionId );

			// Destroying Volcano presence, causes damage to Dahan and invaders
			await gs.DahanOn(space).Bind(actionId).ApplyDamage(1);
			await gs.Invaders.On(space,actionId).UserSelectedDamage(1,spirit);
		}
	}

}