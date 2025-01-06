using System.Text.RegularExpressions;

namespace SpiritIsland.Tests;

public static partial class SpaceExtentions {

	#region Given_InitSummary("1T@2")

	/// <summary> Inits all tokens listed so the summary will match. </summary>
	public static void Given_InitSummary( this SpaceSpec space, string desiredSummary ) => space.ScopeSpace.Given_InitSummary(desiredSummary);

	/// <summary> Inits all tokens listed so the summary will match. </summary>
	public static void Given_InitSummary( this Space space, string desiredSummary ) {
		CountDictionary<IToken> desiredTokens = ParseTokens( desiredSummary );

		// Remove Undesired
		var tokensToRemove = space.Keys.Except( desiredTokens.Keys ).ToArray();
		foreach(var old in tokensToRemove)
			space.Init( old, 0 );

		// Set Desired
		foreach(var p in desiredTokens)
			space.Init( p.Key, p.Value );

		space.Summary.ShouldBe( desiredSummary == "" ? "[none]" : desiredSummary );
	}

	#endregion Given_InitSummary("1T@2")

	#region Given_HasTokens("2E@1")

	/// <summary> Inits these tokens but leaves the non-listed alone. </summary>
	static public Space Given_HasTokens( this SpaceSpec space, string tokenString ) => space.ScopeSpace.Given_HasTokens( tokenString );

	/// <summary> Inits these tokens but leaves the non-listed alone. </summary>
	static public Space Given_HasTokens( this Space space, string tokenString ) {
		foreach(string part in tokenString.Split( ',' )){
			(int count,IToken token) = ParseTokenCount( part );
			space.Init( token, count );
		}
		return space;
	}

	#endregion Given_HasTokens("2E@1")

	#region Given_ClearTokens()

	static public Space Given_ClearTokens( this SpaceSpec space ) 
		=> space.ScopeSpace.Given_ClearTokens();

	static public Space Given_ClearTokens( this Space space ) {
		foreach(IToken token in space.OfType<IToken>().ToArray())
			space.Init(token, 0);
		return space;
	}

	#endregion Given_ClearTokens()

	static public Space Given_ClearInvaders( this Space space ){
		foreach(IToken token in space.HumanOfTag(TokenCategory.Invader).ToArray())
			space.Init(token, 0);
		return space;
	}

	static public Space Given_ClearAll( this Space space ) {
		foreach(ISpaceEntity t in space.Keys.ToArray()) 
			space.Init( t, 0 );
		return space;
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
			"V" => Token.Vitality,
			"Z" => Token.Disease,
			_ => FindSpiritToken(),
		};
		int count = int.Parse( match.Groups[1].Value );
		return (count,token);

		// Group[4] groups the '@' with the #.  Example:  @2
		HumanToken GetHumanToken( HumanTokenClass tokenClass )
			=> new HumanToken(tokenClass)
				.AddDamage( tokenClass.ExpectedHealthHint-int.Parse( match.Groups[5].Value ) )
				.AddStrife( match.Groups[6].Value.Length );

		IToken FindSpiritToken() {
			var spiritTokens = GameState.Current.Spirits.Select(s => s.Presence.Token).ToArray();
			SpiritPresenceToken matchingPresence = spiritTokens.FirstOrDefault(t => t.SpaceAbreviation == abrev);
			if( matchingPresence is null ) {
				string optionsStr = spiritTokens.Select(x => x.SpaceAbreviation).Join(",");
				throw new Exception($"[{abrev}] not found in [{optionsStr}]");
			}

			string plusMinus = match.Groups[3].Value;
			if(plusMinus == "-") return matchingPresence.Self.Incarna;
			if(plusMinus == "+" ) {
				var incarna = matchingPresence.Self.Incarna;
				incarna.Empowered = true;
				return incarna;
			}
			return matchingPresence;
		}

	}

	#endregion Token-Init Helpers

	#region When Invader Explore/Build/Ravage

	static public InvaderCard BuildInvaderCard(this Space space) => space.SpaceSpec.BuildInvaderCard();
	static public InvaderCard BuildInvaderCard( this SpaceSpec space ) {
		var terrain = new[] { Terrain.Wetland, Terrain.Sands, Terrain.Jungle, Terrain.Mountain }.First( space.Is );
		var card = terrain != Terrain.Ocean ? InvaderCard.Stage1( terrain ) : throw new ArgumentException( "Can't invade oceans" );
		card.Flipped = true;
		return card;
	}

	static public Task When_CardRavages( this Space space) => space.SpaceSpec.BuildInvaderCard().When_Ravaging();
	static public Task When_CardRavages( this SpaceSpec space ) => space.BuildInvaderCard().When_Ravaging();

	static public Task When_CardBuilds(this Space space) => space.SpaceSpec.BuildInvaderCard().When_Building();
	static public Task When_CardBuilds( this SpaceSpec space ) => space.BuildInvaderCard().When_Building();

	static public Task When_CardExplories(this Space space) => space.SpaceSpec.BuildInvaderCard().When_Exploring();
	static public Task When_CardExplores( this SpaceSpec space ) => space.BuildInvaderCard().When_Exploring();

	#endregion When Invader Explore/Build/Ravage

	#region Assert_HasInvaders()

	static public void Assert_HasInvaders( this SpaceSpec space, string expectedInvaderSummary )
		=> space.ScopeSpace.Assert_HasInvaders(expectedInvaderSummary);

	static public void Assert_HasInvaders( this Space space, string expectedInvaderSummary )
		=> space.InvaderSummary().ShouldBe( expectedInvaderSummary );

	#endregion Assert_HasInvaders()

	static public string InvaderSummary( this Space space ) {
		static int Order_CitiesTownsExplorers( HumanToken invader )
			=> -(invader.FullHealth * 10 + invader.RemainingHealth);
		return space.InvaderTokens()
			.OrderBy( Order_CitiesTownsExplorers )
			.Select( invader => space[invader] + invader.ToString() )
			.Join( "," );
	}

	[GeneratedRegex( @"(\d+)(\w+)([+-])?(@(\d+)(\^*))?" )]
	private static partial Regex TokenParserRegex();
}
