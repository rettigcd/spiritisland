namespace SpiritIsland.JaggedEarth;

public class SearAngerIntoTheWildLands{ 

	[MinorCard("Sear Anger Into the Wild Lands",0,Element.Sun,Element.Fire,Element.Plant),Slow,FromPresence(1)]
	[Instructions( "Add 1 Badlands. -or- If Wilds and Invaders are present, 1 Fear and 1 Damage." ), Artist( Artists.KatGuevara )]
	static public Task ActAsync( TargetSpaceCtx ctx ){
		return ctx.SelectActionOption(
			Cmd.AddBadlands(1),

			// If wilds and Invaders are present, 1 fear and 1 Damage.
			new SpaceAction( "1 fear and 1 Damage", async ctx => { await ctx.AddFear( 1 ); await ctx.DamageInvaders( 1 ); } )
				.OnlyExecuteIf( x => x.Wilds.Any && x.HasInvaders )
		);
	}

}