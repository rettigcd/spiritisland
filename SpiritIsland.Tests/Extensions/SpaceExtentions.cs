using System.Text.RegularExpressions;

namespace SpiritIsland.Tests;

public static class SpaceExtentions {


	/// <summary> Inits these tokens but leaves the non-listed alone. </summary>
	static public SpaceState Given_HasTokens( this Space space, string tokenString ) => space.Tokens.Given_HasTokens( tokenString );
	static public SpaceState Given_HasTokens( this SpaceState tokens, string tokenString ) {
		foreach(string part in tokenString.Split( ',' ))
			InitToken( tokens, part );
		return tokens;
	}

	static public SpaceState Given_ClearTokens( this Space space ) => space.Tokens.Given_ClearTokens();
	static public SpaceState Given_ClearTokens( this SpaceState space ) {
		foreach(IToken token in space.OfType<IToken>().ToArray())
			space.Init(token, 0);
		return space;
	}

	static public SpaceState Clear( this SpaceState tokens ) {
		foreach(ISpaceEntity t in tokens.Keys.ToArray()) tokens.Init( t, 0 );
		return tokens;
	}

	static readonly Regex tokenParser = new Regex( @"(\d+)(\w+)(@(\d+)(\^*))?" );
	static void InitToken( SpaceState tokens, string part ) {
		var match = tokenParser.Match( part );
		if(!match.Success) throw new FormatException( $"Unrecognized token [{part}] Example: 1T@2." );
		string abrev = match.Groups[2].Value;
		IToken token = abrev switch {
			"C" => GetHumanToken( match, Human.City ),
			"T" => GetHumanToken( match, Human.Town ),
			"E" => GetHumanToken( match, Human.Explorer ).SetAttack( tokens.GetDefault( Human.Explorer ).AsHuman().Attack ), // Russia
			"D" => GetHumanToken( match, Human.Dahan ),
			"A" => Token.Beast,
			"B" => Token.Blight,
			"Z" => (DiseaseToken)Token.Disease,
			_ => FindSpiritToken(abrev),
		};
		int count = int.Parse( match.Groups[1].Value );
		tokens.Init( token, count );
	}
	static SpiritPresenceToken FindSpiritToken( string abrev ) {
		var spiritTokens = GameState.Current.Spirits.Select( s => s.Presence.Token ).ToArray();
		SpiritPresenceToken match = spiritTokens.FirstOrDefault( t => t.SpaceAbreviation == abrev );
		if(match is not null) return match;
		string optionsStr = spiritTokens.Select(x=>x.SpaceAbreviation).Join(",");
		throw new Exception( $"[{abrev}] not found in [{optionsStr}]" );
	}

	static HumanToken GetHumanToken( Match match, HumanTokenClass tokenClass) {
		int fullHealth = tokenClass.ExpectedHealthHint;
		int presentHealth = int.Parse( match.Groups[4].Value );
		int damage = fullHealth-presentHealth;
		int strife = match.Groups[5].Value.Length;
		var token = new HumanToken(tokenClass,fullHealth)
			.AddDamage(damage)
			.AddStrife(strife);
		return token;
	}

}
