namespace SpiritIsland.JaggedEarth;

public class LureOfTheDeepWilderness : Spirit {

	public const string Name = "Lure of the Deep Wilderness";

	public override string Text => Name;

	public override SpecialRule[] SpecialRules => new SpecialRule[] { LurePresence.PlacementRule, EnthrallTheForeignExplorers.Rule };

	public LureOfTheDeepWilderness():base( 
		new LurePresence()
		,PowerCard.For<GiftOfTheUntamedWild>()
		,PowerCard.For<PerilsOfTheDeepestIsland>()
		,PowerCard.For<SoftlyBeckonEverInward>()
		,PowerCard.For<SwallowedByTheWilderness>()
	) {
		GrowthTrack = new GrowthTrack(
			new GrowthOption(new ReclaimAll(),new GainEnergy(1)),
			new GrowthOption(new PlacePresence(4,Target.Inland))
		).Add(
			new GrowthPickGroups(
				new GrowthOption(new Gain1Element(Element.Moon,Element.Air,Element.Plant), new GainEnergy(2)),
				new GrowthOption(new DrawPowerCard())
			)
		);

		InnatePowers = new InnatePower[] {
			InnatePower.For<ForsakeSocietyToChaseAfterDreams>(),
			InnatePower.For<NeverHeardFromAgain>()
		};
	}

	protected override void InitializeInternal( Board board, GameState gs ) {
		// Put 3 presence on your starting board: 2 in land #8, and 1 in land #7.
		gs.Tokens[board[8]].Adjust(Presence.Token, 2);
		gs.Tokens[board[7]].Adjust(Presence.Token, 1);

		// Add 1 beast to land #8
		gs.Tokens[board[8]].Beasts.Init(1);
	}

}
