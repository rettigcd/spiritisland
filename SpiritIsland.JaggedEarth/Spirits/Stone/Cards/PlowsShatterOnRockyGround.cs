namespace SpiritIsland.JaggedEarth;

public class PlowsShatterOnRockyGround {

	[SpiritCard("Plows Shatter on Rocky Ground",2,Element.Earth), Slow, FromPresence(1)]
	[Instructions( "1 Damage to each Town / City. Push up to 1 Town. -or- Destroy 1 Town." ), Artist( Artists.MoroRogers )]
	static public Task ActAsync(TargetSpaceCtx ctx ) {
		return ctx.SelectActionOption(
			new SpaceAction("1 damage to each town / city. Push 1 town.", async ctx => {
				// 1 damage to each town / city.
				await ctx.DamageEachInvader(1,Human.Town_City);
				// push up to 1 town.
				await ctx.PushUpTo(1,Human.Town);
			}),
			Cmd.DestroyTown(1)
		);
	}

}