namespace SpiritIsland.NatureIncarnate;

[InnatePower( "Lost in the Endless Dark" ), Slow, FromPresence(0,Filter.EndlessDark)]
public class LostInTheEndlessDark {

	[InnateTier( "2 moon, 1 air", "1 Fear per Invader (max 4). You may Downgrade 1 Invader.", 0 )]
	static public async Task Option1( IHaveASpirit ctx ) {
		var edCtx = ctx.Self.Target( EndlessDark.Space );

		// 1 fear per invader (max 4)
		int fear = Math.Min( 4, edCtx.Space.SumAny( Human.Invader ) );
		edCtx.AddFear(fear);

		// downgrade up to 1 Invader
		await ReplaceInvader.Downgrade1(edCtx.Self, edCtx.Space, Present.Done,Human.Invader);

	}

	// !!! Add Unit test that Explorer downgraded in Group-0 above is not counted for fear here.  However, a City or town downgraded does.
	[InnateTier( "4 moon, 3 air", "1 Fear per Invader (max 4). Downgrade any number of Invaders.", 1 )]
	static public async Task Option2( IHaveASpirit ctx ) {
		var edCtx = ctx.Self.Target( EndlessDark.Space );

		// 1 fear per invader (max 4)
		int fear = Math.Min( 4, edCtx.Space.SumAny( Human.Invader ) );
		edCtx.AddFear( fear );

		// downgrade any # of invaders
		await ReplaceInvader.DowngradeAll( edCtx.Self, edCtx.Space );
	}

	[InnateTier( "3 moon, 2 animal", "Add 1 Beast", 2 )]
	static public async Task Option3( IHaveASpirit ctx ) {
		var edCtx = ctx.Self.Target( EndlessDark.Space );
		await edCtx.Space.Beasts.AddAsync(1);
	}



}
