using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class PowerTokenPusher : TokenPusher {
		public PowerTokenPusher( IMakeGamestateDecisions ctx, Space source, int countToPush, TokenGroup[] groups, Present present )
			:base(ctx,source,countToPush,groups,present)
		{}

		protected override IEnumerable<Space> GetAdjacents() {
			var mapper = SpaceFilter.ForPowers.TerrainMapper;
			return source.Adjacent.Where(s=>mapper(s) != Terrain.Ocean);
		}
	}

	public class TokenPusher {
		public TokenPusher( IMakeGamestateDecisions ctx, Space source, int countToPush, TokenGroup[] groups, Present present ) {
			this.ctx = ctx;
			this.source = source;
			this.countToPush = countToPush;
			this.groups = groups;
			this.present = present;
		}

		public async Task<Space[]> Exec() {

			IEnumerable<Space> destinationOptions = GetAdjacents();

			var counts = ctx.GameState.Tokens[source];
			Token[] GetTokens() => counts.OfAnyType( groups );
			countToPush = System.Math.Min( countToPush, counts.SumAny( groups ) );

			var pushedToSpaces = new List<Space>();

			var tokens = GetTokens();
			while(0 < countToPush && 0 < tokens.Length) {
				var decision = new SelectTokenToPushDecision( source, countToPush, tokens, present );
				var token = await ctx.Self.Action.Decide( decision );

				if(token == null)
					break;

				var destination = await ctx.Self.Action.Decide( new PushTokenDecision( token, source, destinationOptions, Present.Always ) );
				await ctx.GameState.Move( token, source, destination );

				pushedToSpaces.Add( destination );
				--countToPush;
				tokens = GetTokens();
			}
			return pushedToSpaces.ToArray();
		}

		protected virtual IEnumerable<Space> GetAdjacents()
			=> source.Adjacent.Where( x => x.Terrain != Terrain.Ocean );

		#region private

		readonly IMakeGamestateDecisions ctx;
		protected readonly Space source;
		int countToPush;
		readonly TokenGroup[] groups;
		readonly Present present;

		#endregion

	}

}