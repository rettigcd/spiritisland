namespace SpiritIsland.JaggedEarth;

public class UnnaturalProliferation : BlightCardBase {

	public UnnaturalProliferation():base("Unnatural Proliferation",3) {}

	public override ActionOption<GameState> Immediately 
		=> Cmd.Multiple<GameState>(
			Cmd.EachSpirit(
				// adds 1 presence to a land with their prescense.
				AddPresenceToOwnLand
			),
			Cmd.OnEachBoard( Cmd.Multiple(
				// Add 1 dahan to a land with dahan, and
				AddDahanToDahanLand,
				// 2 cities to the land with fewest town/city (min.1)
				Add2CitiesToLandWithFewest
			) )
		);

	static SelfAction AddPresenceToOwnLand => new SelfAction(
		"Add 1 presence to a land with own presence.", 
		ctx=> ctx.Presence.Place( ctx.Self.Presence.Spaces.ToArray() )
	);

	static ActionOption<BoardCtx> AddDahanToDahanLand => Cmd.AddDahan(1)
		.ToLandOnBoard(  tokens => tokens.Dahan.Any, "a land with dahan." );

	static ActionOption<BoardCtx> Add2CitiesToLandWithFewest => new ActionOption<BoardCtx>(
		"Add 2 cities to the land with fewest town/city.", async ctx => {
			var spaceOptions = ctx.Board.Spaces
				.Where( s=>s.IsInPlay )
				.GroupBy( s=>ctx.GameState.Tokens[s].SumAny(Invader.Town,Invader.City) )
				.OrderBy( grp => grp.Key )
				.First()
				.ToArray();
			TargetSpaceCtx space = await ctx.SelectSpace("Add 2 cities",spaceOptions);
			await space.AddDefault(Invader.City, 2, AddReason.Added);
		}
	);

}