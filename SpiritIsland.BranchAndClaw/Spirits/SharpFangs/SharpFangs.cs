namespace SpiritIsland.BranchAndClaw;

public class SharpFangs : Spirit {

	// https://spiritislandwiki.com/index.php?title=Sharp_Fangs_Behind_the_Leaves

	public const string Name = "Sharp Fangs Behind the Leaves";

	public override string SpiritName => Name;

	public SharpFangs():base(
		spirit => new SpiritPresence( spirit,
			new PresenceTrack( Track.Energy1, Track.AnimalEnergy, Track.PlantEnergy, Track.Energy2, Track.AnimalEnergy, Track.Energy3, Track.Energy4 ),
			new PresenceTrack( Track.Card2, Track.Card2, Track.Card3, Track.CardReclaim1, Track.Card4, Track.Card5Reclaim1 ),
			new AllyOfTheBeasts(spirit)
		),
		new GrowthTrack( 2,
			new GrowthGroup( new ReclaimAll(), new GainEnergy( -1 ), new GainPowerCard() ) { GainEnergy = -1 },
			new GrowthGroup( new PlacePresence( 3, Filter.Beast, Filter.Jungle ) ),
			new GrowthGroup( new GainPowerCard(), new GainEnergy( 1 ) ) { GainEnergy = 1 },
			new GrowthGroup( new GainEnergy( 3 ) ) { GainEnergy = 3 }
		).Add(
			new GrowthPickGroups(new GrowthGroup(new CallForthPredators()))
		),
		PowerCard.ForDecorated(PreyOnTheBuilders.ActAsync),
		PowerCard.ForDecorated(TeethGleamFromDarkness.ActAsync),
		PowerCard.ForDecorated(TerrifyingChase.ActAsync),
		PowerCard.ForDecorated(TooNearTheJungle.ActAsync)
	) {
		InnatePowers = [
			InnatePower.For(typeof(RangingHunt)), // Fast
			InnatePower.For(typeof(FrenziedAssult)), // Slow
		];
		SpecialRules = [AllyOfTheBeasts.Rules,CallForthPredators.Rule];
	}

	protected override void InitializeInternal( Board board, GameState gs ) {

		// Put 1 Presence and 1 Beasts on your starting board in the highest-numbered Jungle.
		var highestJungle = board.Spaces.Where(x => x.IsJungle).Last().ScopeSpace;
		highestJungle.Setup(Presence.Token,1);
		highestJungle.Beasts.Adjust(1);

		// Put 1 Presence in a land of your choice with Beasts anywhere on the island.
		if( SetupAction is not null ) // (note - we don't want this growth in Unit tests, so only add it if we call InitializeInternal())
			AddActionFactory(SetupAction);
	}

	public IActionFactory SetupAction = new PlacePresenceOnBeastLand().ToGrowth();

}