namespace SpiritIsland.JaggedEarth;

public class UnnaturalProliferation : BlightCard {

	public UnnaturalProliferation():base("Unnatural Proliferation","Immediately: Each Spirit adds 1 presence to a land with their presence.  On Each Board: Add 1 dahan to a land with dahan, and 2 city to the land with fewest town/city (min.1).",3) {}

	public override BaseCmd<GameCtx> Immediately 
		=> Cmd.Multiple<GameCtx>(
			Cmd.ForEachSpirit(
				// adds 1 presence to a land with their prescense.
				AddPresenceToOwnLand
			),
			Cmd.ForEachBoard( Cmd.Multiple(
				// Add 1 dahan to a land with dahan, and
				AddDahanToDahanLand,
				// 2 cities to the land with fewest town/city (min.1)
				Add2CitiesToLandWithFewest
			) )
		);

	static SpiritAction AddPresenceToOwnLand => Cmd.PlacePresenceWithin(0);

	static IActOn<BoardCtx> AddDahanToDahanLand => Cmd.AddDahan(1)
		.To().OneLandPerBoard().Which( Has.Dahan );

	static BaseCmd<BoardCtx> Add2CitiesToLandWithFewest => new BaseCmd<BoardCtx>(
		"Add 2 cities to the land with fewest town/city.", async ctx => {
			var spaceOptions = ctx.Board.Spaces.Tokens()
				.GroupBy( ss=>ss.SumAny(Human.Town_City) )
				.OrderBy( grp => grp.Key )
				.First()
				.ToArray();
			Space space = await ctx.Self.SelectSpaceAsync("Add 2 cities",spaceOptions.Downgrade(),Present.Always);
			await space.Tokens.AddDefault(Human.City, 2, AddReason.Added);
		}
	);

}