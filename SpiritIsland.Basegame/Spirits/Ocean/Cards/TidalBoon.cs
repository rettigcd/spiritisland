namespace SpiritIsland.Basegame;

public class TidalBoon {

	[SpiritCard("Tidal Boon",1,Element.Moon,Element.Water,Element.Earth),Slow,AnotherSpirit]
	static public async Task Act(TargetSpiritCtx ctx ) {

		// If dahan are pushed to your ocean, you may move them to any costal land instead of drowning them.
		ctx.GameState.Tokens.TokenMoved.ForRound.Add( PushDahanOutOfOcean );
		async Task PushDahanOutOfOcean( TokenMovedArgs args ) {
			if(args.Token.Class != TokenType.Dahan) return;
			if(!args.AddedTo.IsOcean) return;
			await ctx.Target(args.AddedTo).PushUpToNDahan( args.Count );
		}

		// target spirit gains 2 energy 
		ctx.Other.Energy += 2;

		// and may push 1 town and up to 2 dahan from one of their lands.
		var pushLand = await ctx.OtherCtx.TargetLandWithPresence( "Select land to push town and 2 dahan" );

		await pushLand
			.Pusher
			.AddGroup(1,Invader.Town)
			.AddGroup(2,TokenType.Dahan)
			.MoveUpToN();
	}

}
