namespace SpiritIsland.Basegame;

public class Ocean : Spirit {

	public const string Name = "Ocean's Hungry Grasp";

	readonly SpecialRule OceanInPlay = new SpecialRule(
		"Ocean In Play",
		"You may add/move Presence into Oceans, but may not add/move Presence into Inland lands. On boards where you have 1 or more Presence, Oceans are treated as Coastal Wetlands for Spirit Powers/Special Rules and Blight. You Drown any Invaders or Dahan moved to those Oceans."
	);

	public Ocean():base(
		spirit => new OceanPresence( spirit,
			new PresenceTrack( Track.Energy0, Track.MoonEnergy, Track.WaterEnergy, Track.Energy1, Track.EarthEnergy, Track.WaterEnergy, Track.Energy2 ),
			new PresenceTrack( Track.Card1, Track.Card2, Track.Card2, Track.Card3, Track.Card4, Track.Card5 )
		),
		new GrowthTrack(
			// Option 1 - reclaim, +1 power, gather 1 presense into EACH ocean, +2 energy
			new GrowthGroup(
				new ReclaimAll(),
				new GainPowerCard(),
				new GatherPresenceIntoOcean(),
				new GainEnergy( 2 )
			),
			// Option 2 - +1 presence range any ocean, +1 presense in any ociean, +1 energy
			new GrowthGroup(
				new GainEnergy( 1 ),
				new PlaceInOcean(),
				new PlaceInOcean()
			),
			// Option 3 - gain power card, push 1 presense from each ocean,  add presense on coastal land range 1
			new GrowthGroup(
				new PushPresenceFromOcean(),
				new GainPowerCard(),
				new PlacePresence( 1, Filter.Coastal )
			)
		),
		PowerCard.ForDecorated(CallOfTheDeeps.ActAsync),
		PowerCard.ForDecorated(GraspingTide.ActAsync),
		PowerCard.ForDecorated(SwallowTheLandDwellers.ActAsync),
		PowerCard.ForDecorated(TidalBoon.ActAsync)
	) {
		InnatePowers = [
			InnatePower.For(typeof(OceanBreaksTheShore)),
			InnatePower.For(typeof(PoundShipsToSplinters))
		];
		SpecialRules = [OceanInPlay, Drowning.Rule];
	}

	public override string SpiritName => Name;

	protected override void InitializeInternal( Board board, GameState gameState ) {
		// Swap out Terrain evaluator for power
		gameState.ReplaceTerrain( old=>new OceanTerrainForPower( old,this ), ActionCategory.Spirit_Power );
		gameState.Terrain_ForBlight = new OceanTerrainForPower( gameState.Terrain_ForBlight, this );

		// Place in Ocean
		board.Ocean.ScopeSpace.Setup(Presence.Token,1);

		AddActionFactory( new AddBagPresenceToCostal().ToGrowth() );

		var drownMod = new Drowning(this);
		foreach(Board b in gameState.Island.Boards)
			b.Ocean.ScopeSpace.Adjust( drownMod, 1 );
	}

	// ! Hook for Tidal Boon
	// !!! Move this all to the Drowning mod
	static public void EnableSavingDahan() { ActionScope.Current[SaveDahan] = true; }
	static public bool ShouldSaveDahan() => ActionScope.Current.ContainsKey( SaveDahan );
	const string SaveDahan = "SaveDahanFromDrowning";
}
