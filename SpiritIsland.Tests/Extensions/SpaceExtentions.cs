using System.Text.RegularExpressions;

namespace SpiritIsland.Tests;

public static partial class SpaceExtentions {

	public static void Given_InitSummary( this Space space, string desiredSummary ) => space.ScopeTokens.Given_InitSummary(desiredSummary);
	/// <summary> Inits all tokens listed so the summary will match. </summary>
	public static void Given_InitSummary( this SpaceState spaceState, string desiredSummary ) {
		CountDictionary<IToken> desiredTokens = ParseTokens( desiredSummary );

		// Remove Undesired
		var tokensToRemove = spaceState.Keys.Except( desiredTokens.Keys ).ToArray();
		foreach(var old in tokensToRemove)
			spaceState.Init( old, 0 );

		// Set Desired
		foreach(var p in desiredTokens)
			spaceState.Init( p.Key, p.Value );

		spaceState.Summary.ShouldBe( desiredSummary == "" ? "[none]" : desiredSummary );
	}

	/// <summary> Inits these tokens but leaves the non-listed alone. </summary>
	static public SpaceState Given_HasTokens( this Space space, string tokenString ) => space.ScopeTokens.Given_HasTokens( tokenString );

	/// <summary> Inits these tokens but leaves the non-listed alone. </summary>
	static public SpaceState Given_HasTokens( this SpaceState tokens, string tokenString ) {
		foreach(string part in tokenString.Split( ',' )){
			(int count,IToken token) = ParseTokenCount( part );
			tokens.Init( token, count );
		}
		return tokens;
	}

	static public SpaceState Given_ClearTokens( this Space space ) 
		=> space.ScopeTokens.Given_ClearTokens();

	static public SpaceState Given_ClearTokens( this SpaceState spaceState ) {
		foreach(IToken token in spaceState.OfType<IToken>().ToArray())
			spaceState.Init(token, 0);
		return spaceState;
	}

	static public SpaceState Given_ClearInvaders( this SpaceState spaceState ){
		foreach(IToken token in spaceState.HumanOfTag(TokenCategory.Invader).ToArray())
			spaceState.Init(token, 0);
		return spaceState;
	}

	static public SpaceState Given_ClearAll( this SpaceState spaceState ) {
		foreach(ISpaceEntity t in spaceState.Keys.ToArray()) 
			spaceState.Init( t, 0 );
		return spaceState;
	}

	#region Token-Init Helpers

	static CountDictionary<IToken> ParseTokens( string expectedInvaderSummary ) {
		CountDictionary<IToken> desiredTokens = [];
		if(!string.IsNullOrEmpty( expectedInvaderSummary )) {
			foreach(string part in expectedInvaderSummary.Split( ',' )) {
				(int count,IToken token) = ParseTokenCount(part);
				desiredTokens.Add( token, count );
			}
		}

		return desiredTokens;
	}

	static readonly Regex tokenParser = TokenParserRegex();

	static (int,IToken) ParseTokenCount( string part ) {
		var match = tokenParser.Match( part );
		if(!match.Success) throw new FormatException( $"Unrecognized token [{part}] Example: 1T@2." );

		string abrev = match.Groups[2].Value;
		IToken token = abrev switch {
			"C" => GetHumanToken( Human.City     ),
			"T" => GetHumanToken( Human.Town     ),
			"E" => GetHumanToken( Human.Explorer ).SetAttack( GameState.Current.Tokens.GetDefault( Human.Explorer ).Attack ), // Russia
			"D" => GetHumanToken( Human.Dahan    ),
			"A" => Token.Beast,
			"B" => Token.Blight,
			"Z" => (DiseaseToken)Token.Disease,
			_ => FindSpiritToken(),
		};
		int count = int.Parse( match.Groups[1].Value );
		return (count,token);

		HumanToken GetHumanToken( HumanTokenClass tokenClass )
			=> new HumanToken(tokenClass)
				.AddDamage( tokenClass.ExpectedHealthHint-int.Parse( match.Groups[4].Value ) )
				.AddStrife( match.Groups[5].Value.Length );

		 SpiritPresenceToken FindSpiritToken() {
			var spiritTokens = GameState.Current.Spirits.Select( s => s.Presence.Token ).ToArray();
			SpiritPresenceToken match = spiritTokens.FirstOrDefault( t => t.SpaceAbreviation == abrev );
			if(match is not null) return match;
			string optionsStr = spiritTokens.Select(x=>x.SpaceAbreviation).Join(",");
			throw new Exception( $"[{abrev}] not found in [{optionsStr}]" );
		}

	}

	#endregion Token-Init Helpers

	#region When Invader Explore/Build/Ravage

	static public InvaderCard BuildInvaderCard( this Space space ) {
		var terrain = new[] { Terrain.Wetland, Terrain.Sands, Terrain.Jungle, Terrain.Mountain }.First( space.Is );
		return terrain != Terrain.Ocean ? InvaderCard.Stage1( terrain ) : throw new ArgumentException( "Can't invade oceans" );
	}

	static public Task When_Ravaging( this Space space ) => space.BuildInvaderCard().When_Ravaging();

	static public Task When_Building( this Space space ) => space.BuildInvaderCard().When_Building();

	static public Task When_Exploring( this Space space ) => space.BuildInvaderCard().When_Exploring();

	#endregion When Invader Explore/Build/Ravage

	static public void Assert_HasInvaders( this Space space, string expectedInvaderSummary )
		=> space.ScopeTokens.Assert_HasInvaders(expectedInvaderSummary);

	static public void Assert_HasInvaders( this SpaceState spaceState, string expectedInvaderSummary )
		=> spaceState.InvaderSummary().ShouldBe( expectedInvaderSummary );

	static public void Assert_DreamingInvaders( this Space space, string expectedString )
		=> space.ScopeTokens.Assert_DreamingInvaders( expectedString );

	static public void Assert_DreamingInvaders( this SpaceState tokens, string expectedString ) {
		static int Order_CitiesTownsExplorers( HumanToken invader )
			=> -(invader.FullHealth * 10 + invader.RemainingHealth);
		string dreamerSummary = tokens.HumanOfTag(TokenCategory.Invader)
			.Where( x => x.HumanClass.Variant == TokenVariant.Dreaming )
			.OrderBy( Order_CitiesTownsExplorers )
			.Select( invader => tokens[invader] + invader.ToString() )
			.Join( "," );
		dreamerSummary.ShouldBe( expectedString );
	}

	static public string InvaderSummary( this SpaceState spaceState ) {
		static int Order_CitiesTownsExplorers( HumanToken invader )
			=> -(invader.FullHealth * 10 + invader.RemainingHealth);
		return spaceState.InvaderTokens()
			.OrderBy( Order_CitiesTownsExplorers )
			.Select( invader => spaceState[invader] + invader.ToString() )
			.Join( "," );
	}

	[GeneratedRegex( @"(\d+)(\w+)(@(\d+)(\^*))?" )]
	private static partial Regex TokenParserRegex();
}
