namespace SpiritIsland.FeatherAndFlame;

public class SerpentSlumbering : Spirit {

	public const string Name = "Serpent Slumbering Beneath the Island";

	public override string SpiritName => Name;

	static readonly SpecialRule DeepSlumber = new SpecialRule("Deep Slumber", "You start off limited to 5 presence on the tisland.  Raise this with your 'Absorb Essence' Power Card.  Each use covers the lowest revealed number; your presence limit is the lowest uncovered number.");

	public SerpentSlumbering() :base (
		spirit => new SerpentPresence( spirit )
		, new GrowthTrack( 2,
			new GrowthGroup( new ReclaimAll(), new MovePresence( 1 ) ),
			new GrowthGroup( new GainPowerCard(), new GainEnergy( 1 ) ),
			new GrowthGroup( new GainEnergy( 4 ) ),
			new GrowthGroup( new PlacePresence( 3, Filter.NoBlight ) )
		)
		, PowerCard.For(typeof(ElementalAegis))
		,PowerCard.For(typeof(AbsorbEssence))
		,PowerCard.For(typeof(GiftOfFlowingPower))
		,PowerCard.For(typeof(GiftOfThePrimordialDeeps))	
	) {
		InnatePowers = [
			InnatePower.For(typeof(SerpentWakesInPower)),
			InnatePower.For(typeof(SerpentRousesInAnger))
		];

		SpecialRules = [ DeepSlumber ];
	}

	protected override void InitializeInternal( Board board, GameState gameState ) {
		// Setup: put 1 presence on #5
		board[5].ScopeSpace.Setup(Presence.Token, 1);
	}

}