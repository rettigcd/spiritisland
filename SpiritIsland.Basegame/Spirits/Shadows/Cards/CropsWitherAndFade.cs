namespace SpiritIsland.Basegame;

public class CropsWitherAndFade {

	[SpiritCard("Crops Wither and Fade",1,Element.Moon,Element.Fire,Element.Plant),Slow,FromPresence(0)]
	[Instructions( "2 Fear. Replace 1 Town with 1 Explorer. -or- Replace 1 City with 1 Town." ), Artist( Artists.NolanNasser )]
	static public Task ActAsync( TargetSpaceCtx ctx ){

		// 2 fear
		ctx.AddFear( 2 );

		return ReplaceInvader.Downgrade( ctx, Present.Always, Human.Town_City );

	}

}