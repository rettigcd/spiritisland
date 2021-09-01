using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class PowerCtx : IMakeGamestateDecisions {

		public Spirit Self { get; }

		public GameState GameState { get; }

		public PowerCtx( Spirit self, GameState gameState ) {
			Self = self;
			GameState = gameState;
		}

		public async Task<int> PowerPushUpToNInvaders( Space source, int countToPush , params Invader[] generics ) {

			InvaderSpecific[] CalcInvaderTypes() => GameState.Invaders.Counts[ source ].FilterBy( generics );

			int pushed = 0;

			var invaders = CalcInvaderTypes();
			while(0 < countToPush && 0 < invaders.Length) {
				var invader = await Self.Action.Choose( new SelectInvaderToPushDecision( source, countToPush, invaders, Present.Done ) );
				if(invader == null)
					break;

				var destination = await Self.Action.Choose( new PushInvaderDecision( invader, source, PowerAdjacents(source), Present.Done ) );
				await GameState.Invaders.Move(invader, source, destination );

				++pushed;
				--countToPush;
				invaders = CalcInvaderTypes();
			}
			return pushed;
		}

		public async Task<Space[]> PowerPushUpToNDahan( Space source, int dahanToPush ) {
			HashSet<Space> pushedToLands = new HashSet<Space>();
			dahanToPush = System.Math.Min( dahanToPush, GameState.Dahan.GetCount( source ) );
			while(0 < dahanToPush) {
				Space destination = await Self.Action.Choose(new PushDahanDecision(
					source
					, PowerAdjacents(source)
					, Present.Done
				));
				if(destination == null) break;
				pushedToLands.Add( destination );
				await GameState.Dahan.Move( source, destination );
				--dahanToPush;
			}
			return pushedToLands.ToArray();
		}


		static public IEnumerable<Space> PowerAdjacents(Space source) => source
			.Adjacent
			.Where( x => x.Terrain != Terrain.Ocean );// $ OCEAN $ Special case for Ocean changing what is adjacent for Power stuff

	}

}