namespace SpiritIsland;

public class TokenGatherer {

	protected readonly TargetSpaceCtx ctx;

	public TokenGatherer(TargetSpaceCtx ctx) { this.ctx = ctx; }

	protected virtual SpaceToken[] GetOptions(TokenClass[] groups) => ctx.Adjacent
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
			var source = await ctx.Decision( Select.TokenFromManySpaces.ToGather( sharedGroupCounts.Where(g=>g.count>0), ctx.Space, options, present ) );
			if(source == null) break;
			await ctx.Move( source.Token, source.Space, ctx.Space );
			--sharedGroupCounts[indexLookupByClass[source.Token.Class]].count;
		}
	}

	TokenClass[] RemainingTypes => indexLookupByClass
		.Where( pair => sharedGroupCounts[pair.Value].count > 0)
		.Select( pair => pair.Key )
		.ToArray();

	public TokenGatherer AddGroup( int countToGather, params TokenClass[] classes ) {
		int countIndex = sharedGroupCounts.Count;
		sharedGroupCounts.Add( new GatherGroup{ count = countToGather, classes = classes } );
		foreach(var tokenClass in classes)
			indexLookupByClass.Add( tokenClass, countIndex );
		return this;
	}

	readonly List<GatherGroup> sharedGroupCounts = new(); // the # we push from each group

	readonly Dictionary<TokenClass,int> indexLookupByClass = new(); // map from group back to its count

}

public class GatherGroup {
	public int count;
	public TokenClass[] classes;
	public override string ToString() => count + " " + classes.Select(c=>c.Label).Join("/");
}
