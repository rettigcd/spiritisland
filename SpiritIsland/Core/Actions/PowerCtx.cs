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

		/// <summary> Returns destinations of push </summary>
		public async Task<Space[]> PowerPushUpToNTokens( Space source, int countToPush , params TokenGroup[] generics ) {

			Token[] CalcTokenTypes() => GameState.Tokens[ source ].OfAnyType( generics );

			var pushedToSpaces = new List<Space>();

			var tokens = CalcTokenTypes();
			while(0 < countToPush && 0 < tokens.Length) {
				var token = await Self.Action.Choose( new SelectTokenToPushDecision( source, countToPush, tokens, Present.Done ) );
				if(token == null)
					break;

				var destination = await Self.Action.Choose( new PushTokenDecision( token, source, PowerAdjacents(source), Present.Done ) );
				await GameState.Move(token, source, destination );

				pushedToSpaces.Add(destination);
				--countToPush;
				tokens = CalcTokenTypes();
			}
			return pushedToSpaces.ToArray();
		}

		static public IEnumerable<Space> PowerAdjacents(Space source) => source
			.Adjacent
			.Where( x => x.Terrain != Terrain.Ocean );// $ OCEAN $ Special case for Ocean changing what is adjacent for Power stuff

	}

}