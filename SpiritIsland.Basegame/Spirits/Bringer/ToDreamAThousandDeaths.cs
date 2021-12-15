using System;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class ToDreamAThousandDeaths_DestroyStrategy : DestroyInvaderStrategy {

		readonly SelfCtx ctx;

		public ToDreamAThousandDeaths_DestroyStrategy( Action<FearArgs> addFear, Cause destructionSource, SelfCtx ctx )
			:base(ctx.GameState, addFear,destructionSource) {
			this.ctx = ctx;
		}

		public override async Task OnInvaderDestroyed( Space space, Token token ) {
			if(token.Generic == Invader.City) {
				AddFear( space, 5 );
			} else {
				if(token.Generic == Invader.Town)
					AddFear( space, 2 );
				await BringerPushNInvaders( space, 1, token.Generic );
			}
		}

		async Task BringerPushNInvaders( Space source, int countToPush
				, params TokenGroup[] healthyInvaders
			) {

			// We can't track which original invader is was killed, so let the user choose.

			Token[] CalcInvaderTypes() => ctx.Target(source).Tokens.OfAnyType( healthyInvaders );

			var invaders = CalcInvaderTypes();
			while(0 < countToPush && 0 < invaders.Length) {
				var invader = await ctx.Self.Action.Decision( Decision.TokenOnSpace.TokenToPush( source, countToPush, invaders, Present.Always ) );

				if(invader == null)
					break;

				var destination = await ctx.Self.Action.Decision( new Decision.TargetSpace(
					"Push " + invader.Summary + " to",
					source.Adjacent.Where( s=>ctx.Target(s).IsInPlay )
					, Present.Always
				) );
				await ctx.Move( invader, source, destination );

				--countToPush;
				invaders = CalcInvaderTypes();
			}
		}

	}

}
