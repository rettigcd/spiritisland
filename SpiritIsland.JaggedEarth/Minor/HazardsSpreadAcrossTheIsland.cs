using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {
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
			if( 0 < tokenChoice.Strife() )
				await ctx.AddStrife();
			else
				ctx.Tokens.Adjust(tokenChoice,1); // !!! ?? does this trigger an AddToken event?
		}

		static SpaceToken[] FindHazardTokenInAdjacentLand( TargetSpaceCtx ctx ) {
			var tokenTypes = new TokenGroup[] { TokenType.Badlands.Generic, TokenType.Beast.Generic, TokenType.Disease.Generic, TokenType.Wilds.Generic, Invader.Explorer, Invader.Town, Invader.City };
			var candidates = ctx.Adjacent
				.SelectMany( s =>
					ctx.GameState.Tokens[s].Keys
						.Where( IsTokenOfInterest )
						.Select( token => new SpaceToken( s, token ) )
				)
				.GroupBy( s => s.Token )
				.Select( grp => grp.First() )
				.ToArray();
			return ctx.Self.Energy == 0
				? candidates.Where( st => st.Token != TokenType.Disease).ToArray()
				: candidates;
		}

		static readonly TokenGroup[] InterestedTokenTypes = new TokenGroup[] { TokenType.Badlands.Generic, TokenType.Beast.Generic, TokenType.Disease.Generic, TokenType.Wilds.Generic };
		static bool IsTokenOfInterest( Token token ) {
			return InterestedTokenTypes.Contains( token.Generic )
				|| token.Strife()>0;
		}
	}



}
