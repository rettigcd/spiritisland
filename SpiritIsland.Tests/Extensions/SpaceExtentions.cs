using System.Text.RegularExpressions;

namespace SpiritIsland.Tests;

public static class SpaceExtentions {

	/// <summary>
	/// Inits these tokens but leaves the non-listed alone.
	/// </summary>
	static public SpaceState InitTokens( this SpaceState tokens, string tokenString ) {

		foreach(var part in tokenString.Split( ',' )) {
			var (count, token) = ParseToken( part );
			tokens.Init( token, count );
		}

		return tokens;
	}

	/// <summary>
	/// Inits these tokens but leaves the non-listed alone.
	/// </summary>
	static public SpaceState Clear( this SpaceState tokens ) {
		foreach(var t in tokens.Keys.ToArray()) tokens.Init( t, 0 );
		return tokens;
	}


	static readonly Regex tokenParser = new Regex( @"(\d+)(\w)@(\d+)(\^*)" );
	static (int, ISpaceEntity) ParseToken( string part ) {
		var match = tokenParser.Match( part );
		if(!match.Success) throw new FormatException( $"Unrecognized token [{part}] Example: 1T@2." );
		var tokenClass = match.Groups[2].Value switch {
			"C" => Human.City,
			"T" => Human.Town,
			"E" => Human.Explorer,
			"D" => Human.Dahan,
			_ => throw new Exception( $"Invalid TokenClass [{match.Groups[2].Value}]" ),
		};
		int fullHealth = tokenClass.ExpectedHealthHint;
		var token = new HumanToken(
			tokenClass,
			defaultPenalty,
			fullHealth,
			fullHealth - int.Parse( match.Groups[3].Value ), // damage
			match.Groups[4].Value.Length // strife
		);
		int count = int.Parse( match.Groups[1].Value );
		return (count, token);
	}

	static readonly IHaveHealthPenaltyPerStrife defaultPenalty = new NoStrifePenalty();
	class NoStrifePenalty : IHaveHealthPenaltyPerStrife {
		public int HealthPenaltyPerStrife => 0;
	}

}
