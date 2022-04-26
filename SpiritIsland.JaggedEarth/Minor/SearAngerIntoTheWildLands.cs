namespace SpiritIsland.JaggedEarth;

public class SearAngerIntoTheWildLands{ 

	[MinorCard("Sear Anger into the Wild Lands",0,Element.Sun,Element.Fire,Element.Plant),Slow,FromPresence(1)]
	static public Task ActAsync( TargetSpaceCtx ctx ){
		return ctx.SelectActionOption(
			Cmd.AddBadlands(1),

			// If wilds and Invaders are present, 1 fear and 1 Damage.
			new SpaceAction( "1 fear and 1 Damage", async ctx => { ctx.AddFear( 1 ); await ctx.DamageInvaders( 1 ); } )
				.Matches( x => x.Wilds.Any && x.HasInvaders )
		);
	}

}