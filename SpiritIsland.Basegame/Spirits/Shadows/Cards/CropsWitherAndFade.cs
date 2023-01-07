namespace SpiritIsland.Basegame;

public class CropsWitherAndFade {

	[SpiritCard("Crops Wither and Fade",1,Element.Moon,Element.Fire,Element.Plant)]
	[Slow]
	[FromPresence(0)]
	static public Task ActAsync( TargetSpaceCtx ctx ){

		// 2 fear
		ctx.AddFear( 2 );

		return ReplaceInvader.Downgrade( ctx, Present.Always, Invader.Town_City );

	}

}