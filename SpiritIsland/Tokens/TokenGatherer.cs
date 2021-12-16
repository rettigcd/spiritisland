using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class TokenGatherer {

		protected readonly TargetSpaceCtx ctx;

		public TokenGatherer(TargetSpaceCtx ctx) { this.ctx = ctx; }
		public Task MoveUpTo( int countToGather, params TokenGroup[] groups ) => Move_Inner( countToGather, groups, Present.Done );
		public Task Move( int countToGather, params TokenGroup[] groups ) => Move_Inner( countToGather, groups, Present.Always );

		protected virtual SpaceToken[] GetOptions(TokenGroup[] groups) => ctx.Adjacent
			.SelectMany( a => 
				ctx.GameState.Tokens[a]
					.OfAnyType( groups )
					.Select( t => new SpaceToken( a, t ) ) 
			)
			.ToArray();


		async Task Move_Inner( int countToGather, TokenGroup[] groups, Present present ) {

			SpaceToken[] options;
			while(0 < countToGather
				&& (options = GetOptions(groups)).Length > 0
			) {
				var source = await ctx.Decision( Select.TokenFromManySpaces.ToGather( countToGather, ctx.Space, options, present ) );
				if(source == null) break;
				await ctx.Move( source.Token, source.Space, ctx.Space ); // !!! if moving dahan into frozen land, freeze them
				--countToGather;
			}
		}

	}

}