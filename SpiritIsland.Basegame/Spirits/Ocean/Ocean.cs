namespace SpiritIsland.Basegame;

public class Ocean : Spirit {

	public const string Name = "Ocean's Hungry Grasp";

	public override SpecialRule[] SpecialRules => new SpecialRule[] { OceanInPlay, Drowning.Rule };

	readonly SpecialRule OceanInPlay = new SpecialRule(
		"Ocean In Play",
		"You may add/move Presence into Oceans, but may not add/move Presence into Inland lands. On boards where you have 1 or more Presence, Oceans are treated as Coastal Wetlands for Spirit Powers/Special Rules and Blight. You Drown any Invaders or Dahan moved to those Oceans."
	);

	public Ocean():base(
		spirit => new OceanPresence( spirit,
			new PresenceTrack( Track.Energy0, Track.MoonEnergy, Track.WaterEnergy, Track.Energy1, Track.EarthEnergy, Track.WaterEnergy, Track.Energy2 ),
			new PresenceTrack( Track.Card1, Track.Card2, Track.Card2, Track.Card3, Track.Card4, Track.Card5 )
		),
		PowerCard.For(typeof(CallOfTheDeeps)),
		PowerCard.For(typeof(GraspingTide)),
		PowerCard.For(typeof(SwallowTheLandDwellers)),
		PowerCard.For(typeof(TidalBoon))
	) {

		GrowthTrack = new(
			// Option 1 - reclaim, +1 power, gather 1 presense into EACH ocean, +2 energy
			new GrowthOption(
				new ReclaimAll(),
				new GainPowerCard(),
				new GatherPresenceIntoOcean(),
				new GainEnergy(2)
			), 
			// Option 2 - +1 presence range any ocean, +1 presense in any ociean, +1 energy
			new GrowthOption(
				new GainEnergy(1),
				new PlaceInOcean(),
				new PlaceInOcean()
			), 
			// Option 3 - gain power card, push 1 presense from each ocean,  add presense on coastal land range 1
			new GrowthOption( 
				new PushPresenceFromOcean(),
				new GainPowerCard(),
				new PlacePresence(1, Target.Coastal )
			)
		);

		InnatePowers = new InnatePower[]{
			InnatePower.For(typeof(OceanBreaksTheShore)),
			InnatePower.For(typeof(PoundShipsToSplinters))
		};

	}

	public override string Text => Name;

	protected override void InitializeInternal( Board board, GameState gameState ) {
		// Swap out Terrain evaluator for power
		gameState.ReplaceTerrain( old=>new OceanTerrainForPower( old,this ), ActionCategory.Spirit_Power );
		gameState.Terrain_ForBlight = new OceanTerrainForPower( gameState.Terrain_ForBlight, this );

		// Place in Ocean
		board.Ocean.Tokens.Adjust(Presence.Token,1);

		AddActionFactory( new PlacePresenceInCostal().ToInit() );

		var drownMod = new Drowning(this);
		foreach(Board b in gameState.Island.Boards)
			b.Ocean.Tokens.Adjust( drownMod, 1 );
	}

	// ! Hook for Tidal Boon
	static public void EnableSavingDahan() { ActionScope.Current[SaveDahan] = true; }
	static public bool ShouldSaveDahan() => ActionScope.Current.ContainsKey( SaveDahan );
	const string SaveDahan = "SaveDahanFromDrowning";
}
