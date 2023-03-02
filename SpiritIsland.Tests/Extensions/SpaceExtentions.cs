using System.Text.RegularExpressions;

namespace SpiritIsland.Tests;

public static class SpaceExtentions {

	/// <summary>
	/// Inits these tokens but leaves the non-listed alone.
	/// </summary>
	static public SpaceState Given_HasTokens( this SpaceState tokens, string tokenString ) {

		foreach(var part in tokenString.Split( ',' )) {
			var (count, token) = ParseToken( part );
			tokens.Init( token, count );
		}

		return tokens;
	}

	static public SpaceState Given_ClearTokens( this SpaceState space ) {
		foreach(IToken token in space.OfType<IToken>().ToArray())
			space.Init(token, 0);
		return space;
	}

	static public SpaceState Given_HasTokens( this Space space, string tokenString ) => space.Tokens.Given_HasTokens( tokenString );
	static public SpaceState Given_NoTokens( this Space space ) => space.Tokens.Given_ClearTokens();

	/// <summary>
	/// Inits these tokens but leaves the non-listed alone.
	/// </summary>
	static public SpaceState Clear( this SpaceState tokens ) {
		foreach(var t in tokens.Keys.ToArray()) tokens.Init( t, 0 );
		return tokens;
	}


	static readonly Regex tokenParser = new Regex( @"(\d+)(\w)(@(\d+)(\^*))?" );
	static (int, ISpaceEntity) ParseToken( string part ) {
		var match = tokenParser.Match( part );
		if(!match.Success) throw new FormatException( $"Unrecognized token [{part}] Example: 1T@2." );
		IToken token = match.Groups[2].Value switch {
			"C" => GetHumanToken( match, Human.City ),
			"T" => GetHumanToken( match, Human.Town ),
			"E" => GetHumanToken( match, Human.Explorer ),
			"D" => GetHumanToken( match, Human.Dahan ),
			"A" => Token.Beast,
			"B" => Token.Blight,
			"Z" => (DiseaseToken)Token.Disease,
			_ => throw new Exception( $"Invalid TokenClass [{match.Groups[2].Value}]" ),
		};
		int count = int.Parse( match.Groups[1].Value );
		return (count, token);
	}

	private static HumanToken GetHumanToken( Match match, HumanTokenClass tokenClass) {
		int fullHealth = tokenClass.ExpectedHealthHint;
		var token = new HumanToken(
			tokenClass,
			defaultPenalty,
			fullHealth,
			fullHealth - int.Parse( match.Groups[4].Value ), // damage
			match.Groups[5].Value.Length // strife
		);
		return token;
	}

	static readonly IHaveHealthPenaltyPerStrife defaultPenalty = new NoStrifePenalty();
	class NoStrifePenalty : IHaveHealthPenaltyPerStrife {
		public int HealthPenaltyPerStrife => 0;
	}

}
