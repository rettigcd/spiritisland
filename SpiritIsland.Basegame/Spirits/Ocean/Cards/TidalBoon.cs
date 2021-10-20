using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class TidalBoon {

		[SpiritCard("Tidal Boon",1,Element.Moon,Element.Water,Element.Earth),Slow,AnotherSpirit]
		static public async Task Act(TargetSpiritCtx ctx ) {

			// If dahan are pushed to your ocean, you may move them to any costal land instead of drowning them.
			ctx.GameState.Tokens.TokenMoved.ForThisRound( PushDahanOutOfOcean );
			async Task PushDahanOutOfOcean( GameState gs, TokenMovedArgs args ) {
				if(args.Token.Generic != TokenType.Dahan) return;
				if(args.to.Terrain != Terrain.Ocean) return;
				await ctx.Target(args.to).PushUpToNDahan( args.count );
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
}
