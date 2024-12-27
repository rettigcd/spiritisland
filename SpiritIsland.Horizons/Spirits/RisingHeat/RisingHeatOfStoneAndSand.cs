namespace SpiritIsland.Horizons;

public class RisingHeatOfStoneAndSand : Spirit {

	public const string Name = "Rising Heat of Stone and Sand";
	public override string SpiritName => Name;

	#region custom Tracks

	static Track FiveCardPlaysFire => new Track("5 cardplay,fire", Element.Fire) { CardPlay=5};

	#endregion cusome Tracks

	public RisingHeatOfStoneAndSand():base(
		spirit => new SpiritPresence( spirit,
			new PresenceTrack(Track.Energy1,Track.EarthEnergy,Track.Energy2,Track.Energy3,Track.FireEnergy,Track.Energy4,Track.Energy5),
			new PresenceTrack(Track.Card1, Track.Card2, Track.Card2, Track.Card3, Track.Card4, FiveCardPlaysFire),
			new BlisteringHeat(spirit)
		),
		new GrowthTrack(
			new GrowthGroup( new ReclaimAll(), new GainPowerCard(), new GainEnergy(1) ),
			new GrowthGroup( new PlacePresence(3, Filter.Mountain,Filter.Sands), new PlacePresence(3, Filter.Mountain, Filter.Sands) ),
			new GrowthGroup( new GainPowerCard(), new PlacePresence(1), new GainEnergy(2) )
		),
		PowerCard.ForDecorated(SwelteringExhaustion.ActAsync),
		PowerCard.ForDecorated(GiftOfSearingHeat.ActAsync),
		PowerCard.ForDecorated(StingingSandstorm.ActAsync),
		PowerCard.ForDecorated(CallOnHerdersForAid.ActAsync)
	) {
		InnatePowers = [InnatePower.For(typeof(ScorchWithWavesOfHeat))];
		SpecialRules = [BlisteringHeat.Rule];
	}

	protected override void InitializeInternal(Board board, GameState gameState) {
		// Put 2 Presence on your starting board, in the highest-numbered Sands.
		board.Spaces.Last(s => s.IsSand).ScopeSpace.Setup(Presence.Token, 2);
	}
}
