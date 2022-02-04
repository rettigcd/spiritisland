namespace SpiritIsland.Basegame;

public class CropsWitherAndFade {

	[SpiritCard("Crops Wither and Fade",1,Element.Moon,Element.Fire,Element.Plant)]
	[Slow]
	[FromPresence(0)]
	static public Task ActAsync( TargetSpaceCtx ctx ){

		// 2 fear
		ctx.AddFear( 2 );

		return ctx.SelectActionOption(
			new SpaceAction("replace town with explorer", ctx => ReplaceInvader.Downgrade(ctx,Invader.Town)),
			new SpaceAction("replace city with town", ctx => ReplaceInvader.Downgrade(ctx,Invader.City))
		);

	}

}