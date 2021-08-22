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

		public async Task PowerPushUpToNInvaders( Space source, int countToPush , params Invader[] generics ) {

			InvaderSpecific[] CalcInvaderTypes() => GameState.InvadersOn( source ).FilterBy( generics );

			var invaders = CalcInvaderTypes();
			while(0 < countToPush && 0 < invaders.Length) {
				var invader = await Self.SelectInvader( "Select invader to push", invaders, Present.Done );
				if(invader == null)
					break;

				var destination = await Self.SelectSpace( "Push " + invader.Summary + " to", PowerAdjacents(source) );
				await GameState.MoveInvader(invader, source, destination );

				--countToPush;
				invaders = CalcInvaderTypes();
			}
		}

		public async Task<Space[]> PowerPushUpToNDahan( Space source, int dahanToPush ) {
			HashSet<Space> pushedToLands = new HashSet<Space>();
			dahanToPush = System.Math.Min( dahanToPush, GameState.DahanCount( source ) );
			while(0 < dahanToPush) {
				Space destination = await Self.SelectSpace( "Select destination for dahan"
					, PowerAdjacents(source)
					, Present.Done
				);
				if(destination == null) break;
				pushedToLands.Add( destination );
				await GameState.MoveDahan( source, destination );
				--dahanToPush;
			}
			return pushedToLands.ToArray();
		}


		static public IEnumerable<Space> PowerAdjacents(Space source) => source
			.Adjacent
			.Where( x => x.Terrain != Terrain.Ocean );// $ OCEAN $ Special case for Ocean changing what is adjacent for Power stuff

	}

}