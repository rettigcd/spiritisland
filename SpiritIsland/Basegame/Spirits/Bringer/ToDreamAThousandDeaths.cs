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

		public override async Task OnInvaderDestroyed( Space space, InvaderSpecific specific ) {
			if(specific.Generic == Invader.City) {
				AddFear( space, 5 );
			} else if(specific.Generic == Invader.Town) {
				AddFear( space, 2 );
				await BringerPushNInvaders( space, 1, Invader.Town ); // !!! wrong, need to push correct hit-points
			} else {
				await BringerPushNInvaders( space, 1, Invader.Explorer );
			}
		}

		async Task BringerPushNInvaders( Space source, int countToPush
			, params Invader[] healthyInvaders
		) {

			InvaderSpecific[] CalcInvaderTypes() => engine.GameState.Invaders.Counts[ source ].FilterBy( healthyInvaders );

			var invaders = CalcInvaderTypes();
			while(0 < countToPush && 0 < invaders.Length) {
				var invader = await engine.Self.Action.Choose( new SelectInvaderToPushDecision( source, countToPush, invaders, Present.Always ) );

				if(invader == null)
					break;

				var destination = await engine.Self.Action.Choose( new TargetSpaceDecision(
					"Push " + invader.Summary + " to",
					source.Adjacent.Where( SpaceFilter.ForCascadingBlight.GetFilter( engine.Self, engine.GameState, Target.Any ) )
				) );
				await engine.GameState.Invaders.Move( invader, source, destination );

				--countToPush;
				invaders = CalcInvaderTypes();
			}
		}

	}

}
