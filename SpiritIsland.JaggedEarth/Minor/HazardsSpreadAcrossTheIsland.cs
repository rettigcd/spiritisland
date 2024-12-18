namespace SpiritIsland.JaggedEarth;

public class HazardsSpreadAcrossTheIsland{ 

	[MinorCard("Hazards Spread Across the Island",0,Element.Fire,Element.Air,Element.Earth,Element.Plant),Fast,FromSacredSite(2)]
	[Instructions( "Choose a type of token from Badlands / Beasts / Disease / Strife / Wilds that exists in an adjacent land; choosing Disease costs 1 energy. Add 1 of that type of token to target land." ), Artist( Artists.KatGuevara )]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {

		// Choose a type of token from badlands / beast / disease / strife / wilds that exists in an adjacent land;
		var candidates = FindHazardTokenInAdjacentLand( ctx );
		if(candidates.Length == 0) return;

		var decision = new A.SpaceTokenDecision("Select hazard to add to "+ctx.SpaceSpec.Label, candidates, Present.Always);
		var tokenChoice = (await ctx.SelectAsync(decision))!.Token;

		// choosing disease costs 1 energy.
		if( tokenChoice == Token.Disease )
			ctx.Self.Energy--;

		// Add 1 of that type of token to target land.
		if( tokenChoice is HumanToken ht && 0 < ht.StrifeCount )
			await ctx.AddStrife();
		else
			await ctx.Space.AddAsync( tokenChoice,1);
	}

	static SpaceToken[] FindHazardTokenInAdjacentLand( TargetSpaceCtx ctx ) {
		var tokenTypes = Human.Invader // finds .Any finds strife
			.Plus( Token.Badlands, Token.Beast, Token.Disease, Token.Wilds );
		var candidates = ctx.Adjacent
			.SelectMany( adjState => adjState.OfType<IToken>()
				.Where( IsTokenOfInterest )
				.On( adjState ) //.Select( token => new SpaceToken( adjState.Space, token ) )
			)
			.GroupBy( s => s.Token )
			.Select( grp => grp.First() )
			.ToArray();
		return ctx.Self.Energy == 0
			? candidates.Where( st => st.Token != Token.Disease).ToArray()
			: candidates;
	}

	static readonly ITokenClass[] InterestedTokenTypes = [ Token.Badlands, Token.Beast, Token.Disease, Token.Wilds ];
	static bool IsTokenOfInterest( IToken token ) {
		return InterestedTokenTypes.Contains( token.Class )
			|| token is HumanToken ht && 0<ht.StrifeCount;
	}

}