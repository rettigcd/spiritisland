namespace SpiritIsland.Horizons;

public class FathomlessMudOfTheSwamp : Spirit {

	public const string Name = "Fathomless Mud of the Swamp";
	public override string SpiritName => Name;

	public FathomlessMudOfTheSwamp():base(
		spirit => new SpiritPresence( spirit,
			new PresenceTrack(Track.Energy1,Track.PlantEnergy,Track.Energy2,Track.Energy3,Track.WaterEnergy,Track.Energy4,Track.Energy5),
			new PresenceTrack(Track.Card1,Track.Card2,Track.EarthEnergy,Track.Card3,Track.MoonEnergy,Track.Card4)
		),
		new GrowthTrack(
			new GrowthGroup( new ReclaimAll(), new PlacePresence(0) ),
			new GrowthGroup( new PlacePresence(1), new PlacePresence(1) ),
			new GrowthGroup( new GainPowerCard(), new PlacePresence(2), new GainEnergy(2) )
		),
		PowerCard.ForDecorated(IntractableThicketsAndThorns.ActAsync),	// fast
		PowerCard.ForDecorated(ExaltationOfTangledGrowth.ActAsync),
		PowerCard.ForDecorated(FoulVaporsAndFetidMuck.ActAsync),
		PowerCard.ForDecorated(OpenShiftingWaterways.ActAsync)
	) {
		InnatePowers = [InnatePower.For(typeof(SpreadingAndDreadfulMire))];
		SpecialRules = [OfferNoFirmFoundations.Rule];
	}

	protected override void InitializeInternal(Board board, GameState gameState) {
		// Put 2 Presence on your starting board, in the lowest-numbered Wetland.
		board.Spaces.First(s => s.IsWetland).ScopeSpace.Setup(Presence.Token, 2);

		OfferNoFirmFoundations.Init(this,gameState);
	}

}
