using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class TokenGatherer {
		readonly TargetSpaceCtx ctx;
		public TokenGatherer(TargetSpaceCtx ctx) { this.ctx = ctx; }
		public Task MoveUpTo( int countToGather, params TokenGroup[] groups ) => Move_Inner( countToGather, groups, Present.Done );
		public Task Move( int countToGather, params TokenGroup[] groups ) => Move_Inner( countToGather, groups, Present.Always );

		async Task Move_Inner( int countToGather, TokenGroup[] groups, Present present ) {

			SpaceToken[] GetOptions() => ctx.Adjacent
				.SelectMany( a => 
					ctx.GameState.Tokens[a]
						.OfAnyType( groups )		// !!! stop Freezable dahan
						.Select( t => new SpaceToken( a, t ) ) 
				)
				.ToArray();

			SpaceToken[] options;
			while(0 < countToGather
				&& (options = GetOptions()).Length > 0
			) {
				var source = await ctx.Self.Action.Decision( new Decision.SpaceTokens_ToGather( countToGather, ctx.Space, options, present ) );
				if(source == null) break;
				await ctx.Move( source.Token, source.Space, ctx.Space ); // !!! if moving dahan into frozen land, freeze them
				--countToGather;
			}
		}

	}

}