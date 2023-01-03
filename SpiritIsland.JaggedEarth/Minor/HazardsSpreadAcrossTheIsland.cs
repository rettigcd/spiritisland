namespace SpiritIsland.JaggedEarth;

public class HazardsSpreadAcrossTheIsland{ 

	[MinorCard("Hazards Spread Across the Island",0,Element.Fire,Element.Air,Element.Earth,Element.Plant),Fast,FromSacredSite(2)]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {

		// Choose a type of token from badlands / beast / disease / strife / wilds that exists in an adjacent land;
		var candidates = FindHazardTokenInAdjacentLand( ctx );
		if(candidates.Length == 0) return;

		var tokenChoice = (await ctx.Decision(new Select.TokenFromManySpaces("Select hazard to add to "+ctx.Space.Label, candidates, Present.Always))).Token;

		// choosing disease costs 1 energy.
		if( tokenChoice == TokenType.Disease )
			ctx.Self.Energy--;

		// Add 1 of that type of token to target land.
		if( tokenChoice is HealthToken ht && 0 < ht.StrifeCount )
			await ctx.AddStrife();
		else
			await ctx.Tokens.Add(tokenChoice,1,ctx.ActionCtx);
	}

	static SpaceToken[] FindHazardTokenInAdjacentLand( TargetSpaceCtx ctx ) {
		var tokenTypes = new TokenClass[] { TokenType.Badlands, TokenType.Beast, TokenType.Disease, TokenType.Wilds, Invader.Explorer, Invader.Town, Invader.City };
		var candidates = ctx.Adjacent
			.SelectMany( adjState => adjState.Keys
				.Where( IsTokenOfInterest )
				.Select( token => new SpaceToken( adjState.Space, token ) )
			)
			.GroupBy( s => s.Token )
			.Select( grp => grp.First() )
			.ToArray();
		return ctx.Self.Energy == 0
			? candidates.Where( st => st.Token != TokenType.Disease).ToArray()
			: candidates;
	}

	static readonly TokenClass[] InterestedTokenTypes = new TokenClass[] { TokenType.Badlands, TokenType.Beast, TokenType.Disease, TokenType.Wilds };
	static bool IsTokenOfInterest( Token token ) {
		return InterestedTokenTypes.Contains( token.Class )
			|| token is HealthToken ht && 0<ht.StrifeCount;
	}

}