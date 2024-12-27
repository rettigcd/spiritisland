namespace SpiritIsland.Horizons;

public class EyesWatchFromTheTrees : Spirit {

	public const string Name = "Eyes Watch from the Trees";
	public override string SpiritName => Name;

	public EyesWatchFromTheTrees():base(
		spirit => new SpiritPresence( spirit,
			new PresenceTrack(Track.Energy1, Track.Energy1,Track.Energy2,Track.PlantEnergy,Track.Energy3,Track.MoonEnergy,Track.Energy4),
			new PresenceTrack(Track.Card1,Track.Card2,Track.AirEnergy,Track.Card3,Track.PlantEnergy,Track.Card4)
		),
		new GrowthTrack(
			new GrowthGroup( new ReclaimAll(), new GainPowerCard(), new GainEnergy(1) ),
			new GrowthGroup( new PlacePresence(2), new PlacePresence(0) ),
			new GrowthGroup( new GainPowerCard(), new PlacePresence(3), new GainEnergy(1) )
		),
		PowerCard.ForDecorated(BoonOfWatchfulGuarding.ActAsync),
		PowerCard.ForDecorated(MysteriousAbductions.ActAsync),
		PowerCard.ForDecorated(EerieNoisesAndMovingTrees.ActAsync),
		PowerCard.ForDecorated(WhisperedGuidanceThroughTheNight.ActAsync)
	) {
		InnatePowers = [InnatePower.For(typeof(MichiefAndSabotage))];
		SpecialRules = [DahanTrustTheWatchers.Rule];
	}

	protected override void InitializeInternal(Board board, GameState gameState) {
		// Put 2 Presence on your starting board, in the highest-numbered Jungle
		board.Spaces.Last(s=>s.IsJungle).ScopeSpace.Setup(Presence.Token, 2);
	}

	public override void InitSpiritAction(ActionScope scope) {
		base.InitSpiritAction(scope);
		scope.Defender = new DahanTrustTheWatchers(this);
	}


}
