namespace SpiritIsland.JaggedEarth;

// Boards
//S:	C Can pair up Dahan.Access Coast.Town in starting land.  C8 is only weekness. (C2>C4>C3)
//S:	G Can pair up Dahan. (G2>G6>)

//A:	D Can't get to D3, Dahan in Growth lands
//A:	E E4 is prob, if comes up 1st need to play Plow share

//B:	F Can't pair Dahan on F3
//B:	A Can't pair Dahan on A6
//B:	H Can't pair Dahan on H3
//B:	B Can't pair Dahan on B1 (Worse board)


public class StonesUnyieldingDefiance : Spirit {

	public const string Name = "Stone's Unyielding Defiance";
	public override string SpiritName => Name;

	static readonly SpecialRule DeepLayers = new SpecialRule("Deep Layers Exposed to the Surface", "The first time you uncover each of your +1 Card Play presence spaces, gain a Minor Power.");

	#region special Tracks

	static Track AddCardPlay => new Track("Energy+DrawMinor+CardPlay") {
		OnRevealAsync = (track, spirit) => spirit.DrawMinor(),
		Action = new PlayExtraCardThisTurn(1),

		Icon = new IconDescriptor {
			ContentImg = Img.CardPlayPlusN,
			Text = "+1",
			Super = new IconDescriptor { BackgroundImg = Img.Stone_Minor }
		}
	};

	static Track EarthReclaim => new Track( "earth card", Element.Earth ) { 
		Action = new ReclaimN(),
		Icon = new IconDescriptor {
			BackgroundImg = Img.Token_Earth,
			Sub = new IconDescriptor { BackgroundImg = Img.Reclaim1 }
		}
	};

	static Track EarthAndAny => new Track( "earth any", Element.Earth, Element.Any ) {
		Icon = new IconDescriptor {
			BackgroundImg = Img.Token_Earth,
			Sub = new IconDescriptor { BackgroundImg = Img.Token_Any }
		}
	};

	static Track Card2Earth => new Track( "2 cardplay,earth", Element.Earth ) { 
		CardPlay = 2,
		Icon = new IconDescriptor {
			BackgroundImg = Img.CardPlay, Text = "2",
			Sub = new IconDescriptor { BackgroundImg = Img.Token_Earth }
		}
	};
	
	#endregion special Tracks

	public StonesUnyieldingDefiance() : base(
		spirit => new SpiritPresence( spirit,
			new PresenceTrack( Track.Energy2, Track.Energy3, AddCardPlay, Track.Energy4, AddCardPlay, Track.Energy6, AddCardPlay ),
			new PresenceTrack( Track.Card1, Track.MkCard( Element.Earth ), Track.MkCard( Element.Earth ), EarthReclaim, EarthAndAny, Card2Earth ),
			new BestowTheEnduranceOfBedrockPresenceToken( spirit )
		)
		, new GrowthTrack(
			new GrowthGroup(
				new ReclaimAll(),
				new PlacePresence( 3, Filter.Mountain, Filter.Presence ),
				new GainAllElements( Element.Earth, Element.Earth )
			),
			new GrowthGroup(
				new PlacePresence( 2 ),
				new GainEnergy( 3 )
			),
			new GrowthGroup(
				new GainPowerCard(),
				new PlacePresence( 1 )
			)
		)
		,PowerCard.For(typeof(JaggedShardsPushFromTheEarth))
		,PowerCard.For(typeof(PlowsShatterOnRockyGround))
		,PowerCard.For(typeof(ScarredAndStonyLand))
		,PowerCard.For(typeof(StubbornSolidity))
	) {
		InnatePowers = [
			InnatePower.For(typeof(HoldTheIslandFastWithABulwarkOfWill)), 
			InnatePower.For(typeof(LetThemBreakThemselvesAgainstTheStone))
		];
		SpecialRules = [BestowTheEnduranceOfBedrockPresenceToken.Rule, DeepLayers];
	}

	protected override void InitializeInternal( Board board, GameState gameState ) {
		// place presence in lowest-numbered Mountain without dahan
		var ss = board.Spaces
			.Where( s=>s.IsMountain )
			.ScopeTokens()
			.Where( s=>s.Dahan.CountAll==0 )
			.First();
		ss.Setup(Presence.Token,1);

		// 1 in an adjacent land that has Blight(if possible) or is Sands(if not)
		Space adjacentWithBlight = ss.Adjacent.FirstOrDefault(s=>s[SpiritIsland.Token.Blight]>0);
		Space adjacentWithSand = ss.Adjacent.FirstOrDefault( s => s.SpaceSpec.IsSand );

		(adjacentWithBlight ?? adjacentWithSand).Setup(Presence.Token,1);

	}

}

public class BestowTheEnduranceOfBedrockPresenceToken(Spirit spirit) 
	: SpiritPresenceToken(spirit)
	, IModifyAddingToken
{

	static public SpecialRule Rule => new SpecialRule(
		"Bestow the Endurance of BedRock", 
		"When blight is added to one of your lands, unless the blight then outnumbers your presence, it does not cascade or destroy presence (yours or others')."
	);

	public Task ModifyAddingAsync(AddingTokenArgs args) {
		if( args.Token == Token.Blight && args.To.Blight.Count < args.To[this] ) {
			// it does not cascade or destroy presence (yours or others').
			BlightToken.ScopeConfig.ShouldCascade = false;
			BlightToken.ScopeConfig.DestroyPresence = false;
		}
		return Task.CompletedTask;
	}
}
