using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class TidalBoon {

		[SpiritCard("Tidal Boon",1,Speed.Slow,Element.Moon,Element.Water,Element.Earth)]
		[TargetSpirit]
		static public async Task Act(TargetSpiritCtx ctx ) {

			// target spirit gains 2 energy 
			ctx.Target.Energy += 2;

			// and may push 1 town and up to 2 dahan from one of their lands.
			var pushLand = await ctx.Target.Action.Decide( new TargetSpaceDecision( "Select land to push town and 2 dahan", ctx.Target.Presence.Spaces, Present.Done ));
			if(pushLand==null) return;

			var spaceCtx = new TargetSpaceCtx(ctx.Target,ctx.GameState,pushLand);
			await spaceCtx.PushUpToNTokens(1,Invader.Town);
			await spaceCtx.PushUpToNTokens(2,TokenType.Dahan);

			// If dahan are pushed to your ocean, you may move them to any costal land instead of drowning them.
			ctx.GameState.Tokens.TokenMoved.ForRound.Add( PushDahanOutOfOcean );

			async Task PushDahanOutOfOcean( GameState gs, TokenMovedArgs args ) {
				if(args.Token.Generic != TokenType.Dahan) return;
				if(args.to.Terrain != Terrain.Ocean) return;
				await ctx.Self.MakeDecisionsFor(gs).PushUpToNTokens(args.to,args.count,TokenType.Dahan);
			}

		}


	}
}
