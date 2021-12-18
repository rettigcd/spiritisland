using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class TokenGatherer {

		protected readonly TargetSpaceCtx ctx;

		public TokenGatherer(TargetSpaceCtx ctx) { this.ctx = ctx; }

		protected virtual SpaceToken[] GetOptions(TokenCategory[] groups) => ctx.Adjacent
			.SelectMany( a => 
				ctx.GameState.Tokens[a]
					.OfAnyType( groups )
					.Select( t => new SpaceToken( a, t ) ) 
			)
			.ToArray();

		public Task GatherN() => Gather_Inner( Present.Always );
		public Task GatherUpToN() => Gather_Inner( Present.Done );

		async Task Gather_Inner( Present present ) {

			SpaceToken[] options;
			while(0 < (options = GetOptions( RemainingTypes )).Length) {
				var source = await ctx.Decision( Select.TokenFromManySpaces.ToGather( sharedGroupCounts.Sum(), ctx.Space, options, present ) );
				if(source == null) break;
				await ctx.Move( source.Token, source.Space, ctx.Space ); // !!! if moving dahan into frozen land, freeze them
				--sharedGroupCounts[indexLookupByGroup[source.Token.Category]];
			}
		}

		TokenCategory[] RemainingTypes => indexLookupByGroup
			.Where( pair => sharedGroupCounts[pair.Value] > 0)
			.Select( pair => pair.Key )
			.ToArray();

		public TokenGatherer AddGroup( int countToGather, params TokenCategory[] groups ) {
			int countIndex = sharedGroupCounts.Count;
			sharedGroupCounts.Add( countToGather );
			foreach(var group in groups)
				indexLookupByGroup.Add( group, countIndex );
			return this;
		}

		readonly List<int> sharedGroupCounts = new(); // the # we push from each group

		readonly Dictionary<TokenCategory,int> indexLookupByGroup = new(); // map from group back to its count

	}

}