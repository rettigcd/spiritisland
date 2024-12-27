namespace SpiritIsland.Horizons;

public class DevouringTeethLurkUnderfoot : Spirit {

	public const string Name = "Devouring Teeth Lurk Underfoot";
	public override string SpiritName => Name;

	public DevouringTeethLurkUnderfoot():base(
		spirit => new SpiritPresence( spirit,
			new PresenceTrack(Track.Energy2,Track.FireEnergy,Track.Energy3,Track.Energy4,Track.AnimalEnergy,Track.Energy6,Track.Energy7),
			new PresenceTrack(Track.Card1, Track.Card2, Track.AnimalEnergy, Track.FireEnergy, Track.Card3, Track.EarthEnergy, Track.Card4)
		),
		new GrowthTrack(
			new GrowthGroup( new ReclaimAll(), new PlacePresence(0) ),
			new GrowthGroup( new GainPowerCard(), new PlacePresence(1) ),
			new GrowthGroup( new PlacePresence(2), new GainEnergy(3) )
		),
		PowerCard.ForDecorated(GiftOfFuriousMight.ActAsync),				// fast
		PowerCard.ForDecorated(MarkTerritoryWithScarsAndTeeth.ActAsync),	// fast
		PowerCard.ForDecorated(FerociousRampage.ActAsync),					// slow
		PowerCard.ForDecorated(HerdTowardsTheLurkingMaw.ActAsync)			// slow
	) {
		InnatePowers = [InnatePower.For(typeof(DeathApproachesFromBeneathTheSurface))];
		SpecialRules = [TerritorialAggression];
		BonusDamage = 1;
	}

	protected override void InitializeInternal(Board board, GameState gameState) {
		// Put 1 Presence on your starting board, in land #5.
		board[5].ScopeSpace.Setup(Presence.Token, 1);
	}

	static SpecialRule TerritorialAggression => new SpecialRule("Territorial Aggression","Your Damage-Dealing Powers do +1 Damage.");
}
