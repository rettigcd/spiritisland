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

		public override async Task OnInvaderDestroyed( Space space, Token specific ) {
			if(specific.Generic == Invader.City) {
				AddFear( space, 5 );
			} else {
				if(specific.Generic == Invader.Town)
					AddFear( space, 2 );
				await BringerPushNInvader( space, 1, specific );
			}
		}

		async Task BringerPushNInvader( Space source, int countToPush ,Token invader ) {

			var destination = await engine.Self.Action.Decide( new TargetSpaceDecision(
				"Push " + invader.Summary + " to",
				source.Adjacent.Where( SpaceFilter.ForPowers.GetFilter( engine.Self, engine.GameState, Target.Any ) )
			) );
			await engine.GameState.Move( invader, source, destination );

		}

	}

}
