using System;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class ToDreamAThousandDeaths_DestroyStrategy : DestroyInvaderStrategy {

		readonly IMakeGamestateDecisions engine;

		public ToDreamAThousandDeaths_DestroyStrategy( Action<FearArgs> addFear, Cause destructionSource, IMakeGamestateDecisions engine )
			:base(addFear,destructionSource) {
			this.engine = engine;
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

			Token[] CalcInvaderTypes() => engine.GameState.Tokens[source].OfAnyType( healthyInvaders );

			var invaders = CalcInvaderTypes();
			while(0 < countToPush && 0 < invaders.Length) {
				var invader = await engine.Self.Action.Decision( new Decision.TokenToPush( source, countToPush, invaders, Present.Always ) );

				if(invader == null)
					break;

				var destination = await engine.Self.Action.Decision( new Decision.TargetSpace(
					"Push " + invader.Summary + " to",
					source.Adjacent.Where( SpaceFilter.ForPowers.GetFilter( engine.Self, engine.GameState, Target.Any ) )
				) );
				await engine.GameState.Move( invader, source, destination );

				--countToPush;
				invaders = CalcInvaderTypes();
			}
		}

	}

}
