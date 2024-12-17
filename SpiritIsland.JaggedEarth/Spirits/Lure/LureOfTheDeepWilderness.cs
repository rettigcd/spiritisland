namespace SpiritIsland.JaggedEarth;

public class LureOfTheDeepWilderness : Spirit {

	public const string Name = "Lure of the Deep Wilderness";

	public override string SpiritName => Name;

	public LureOfTheDeepWilderness():base( 
		x => new LurePresence(x)

		, new GrowthTrack(
			new GrowthGroup( new ReclaimAll(), new GainEnergy( 1 ) ),
			new GrowthGroup( new PlacePresence( 4, Filter.Inland ) )
		).Add(
			new GrowthPickGroups(
				new GrowthGroup( new Gain1Element( Element.Moon, Element.Air, Element.Plant ), new GainEnergy( 2 ) ),
				new GrowthGroup( new GainPowerCard() )
			)
		)
		,PowerCard.ForDecorated(GiftOfTheUntamedWild.ActAsync)
		,PowerCard.ForDecorated(PerilsOfTheDeepestIsland.ActAsync)
		,PowerCard.ForDecorated(SoftlyBeckonEverInward.ActAsync)
		,PowerCard.ForDecorated(SwallowedByTheWilderness.ActAsync)
	) {

		InnatePowers = [
			InnatePower.For(typeof(ForsakeSocietyToChaseAfterDreams)),
			InnatePower.For(typeof(NeverHeardFromAgain))
		];
		SpecialRules = [LurePresence.PlacementRule, EnthrallTheForeignExplorers.Rule];
	}

	protected override void InitializeInternal( Board board, GameState gs ) {
		// Put 3 presence on your starting board: 2 in land #8, and 1 in land #7.
		board[8].ScopeSpace.Setup(Presence.Token, 2);
		board[7].ScopeSpace.Setup(Presence.Token, 1);

		// Add 1 beast to land #8
		board[8].ScopeSpace.Beasts.Init(1);
	}

}
