namespace SpiritIsland.NatureIncarnate;

[InnatePower( "Lost in the Endless Dark" ), Slow, FromPresence(0,Target.EndlessDark)]
public class LostInTheEndlessDark {

	[InnateOption( "2 moon, 1 air", "1 Fear per Invader (max 4). You may Downgrade 1 Invader.", 0 )]
	static public async Task Option1( SelfCtx ctx ) {
		var edCtx = ctx.Target( EndlessDark.Space );

		// 1 fear per invader (max 4)
		int fear = Math.Min( 4, edCtx.Tokens.SumAny( Human.Invader ) );
		edCtx.AddFear(fear);

		// downgrade up to 1 Invader
		await ReplaceInvader.Downgrade1(edCtx,Present.Done,Human.Invader);

	}

	[InnateOption( "4 moon, 3 air", "1 Fear per Invader (max 4). Downgrade any number of Invaders.", 1 )]
	static public async Task Option2( SelfCtx ctx ) {
		var edCtx = ctx.Target( EndlessDark.Space );

		// 1 fear per invader (max 4)
		int fear = Math.Min( 4, edCtx.Tokens.SumAny( Human.Invader ) );
		edCtx.AddFear( fear );

		// downgrade any # of invaders
		await ReplaceInvader.DowngradeAnyNumber( edCtx );
	}

	[InnateOption( "3 moon, 2 animal", "Add 1 Beast", 2 )]
	static public async Task Option3( SelfCtx ctx ) {
		var edCtx = ctx.Target( EndlessDark.Space );
		await edCtx.Tokens.Beasts.Add(1);
	}



}
