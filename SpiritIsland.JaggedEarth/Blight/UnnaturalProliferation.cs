namespace SpiritIsland.JaggedEarth;

public class UnnaturalProliferation : BlightCard {

	public UnnaturalProliferation():base("Unnatural Proliferation","Immediately: Each Spirit adds 1 presence to a land with their presence.  On Each Board: Add 1 dahan to a land with dahan, and 2 city to the land with fewest town/city (min.1).",3) {}

	public override DecisionOption<GameCtx> Immediately 
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

	static SelfAction AddPresenceToOwnLand => new SelfAction(
		"Add 1 presence to a land with own presence.", 
		ctx=> ctx.Self.PlacePresenceOn1( ctx.Self.Presence.Spaces.Tokens().ToArray() )
	);

	static IExecuteOn<BoardCtx> AddDahanToDahanLand => Cmd.AddDahan(1)
		.To().OneLandPerBoard().Which( Has.Dahan );

	static DecisionOption<BoardCtx> Add2CitiesToLandWithFewest => new DecisionOption<BoardCtx>(
		"Add 2 cities to the land with fewest town/city.", async ctx => {
//			var terrainMapper = ctx.GameState.Island.Terrain;
			var spaceOptions = ctx.Board.Spaces.Tokens()
				.GroupBy( ss=>ss.SumAny(Human.Town_City) )
				.OrderBy( grp => grp.Key )
				.First()
				.ToArray();
			TargetSpaceCtx space = await ctx.SelectSpace("Add 2 cities",spaceOptions.Downgrade());
			await space.AddDefault(Human.City, 2, AddReason.Added);
		}
	);

}