namespace SpiritIsland.JaggedEarth;

public class StonesUnyieldingDefiance : Spirit {

	public const string Name = "Stone's Unyielding Defiance";
	public override string Text => Name;

	public override SpecialRule[] SpecialRules => new SpecialRule[] { 
		new SpecialRule("Bestow the Endurance of BedRock", "When blight is added to one of your lands, unless the blight then outnumbers your presence, it does not cascade or destroy presence (yours or others')."), 
		new SpecialRule("Deep Layers Expposed to the Surface", "The first time you uncover each of your +1 Card Play presence spaces, gain a Minor Power.") 
	};

	static Track AddCardPlay => new Track( "PlayExtraCardThisTurn" ) { 
		Action = new DrawMinorOnceAndPlayExtraCardThisTurn(),
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

public StonesUnyieldingDefiance() : base(
		new SpiritPresence(
			new PresenceTrack( Track.Energy2, Track.Energy3, AddCardPlay, Track.Energy4, AddCardPlay, Track.Energy6, AddCardPlay ),
			new PresenceTrack( Track.Card1, Track.MkCard( Element.Earth ), Track.MkCard( Element.Earth ), EarthReclaim, EarthAndAny, Card2Earth )
		)
		,PowerCard.For<JaggedShardsPushFromTheEarth>()
		,PowerCard.For<PlowsShatterOnRockyGround>()
		,PowerCard.For<ScarredAndStonyLand>()
		,PowerCard.For<StubbornSolidity>()
	) {

		this.GrowthTrack = new GrowthTrack(
			new GrowthOption(new ReclaimAll(),new PlacePresence(3,Target.Mountain, Target.Presence ),new GainElements(Element.Earth,Element.Earth)),
			new GrowthOption(new PlacePresence(2), new GainEnergy(3)),
			new GrowthOption(new DrawPowerCard(), new PlacePresence(1))
		);
		InnatePowers = new InnatePower[] {
			InnatePower.For<HoldTheIslandFastWithABulwarkOfWill>(),
			InnatePower.For<LetThemBreakThemselvesAgainstTheStone>()
		};
	}

	protected override void InitializeInternal( Board board, GameState gameState ) {
		// place presence in lowest-numbered Mountain without dahan
		var ss = board.Spaces.Skip(1)
			.Select( s => gameState.Tokens[s] )
			.Where(s=>s.Dahan.Count==0)
			.First();
		Presence.Adjust(ss,1);

		// 1 in an adjacent land that has Blight(if possible)
		var space2 = ss.Adjacent.FirstOrDefault(s=>s[TokenType.Blight]>0)
			// or is Sands(if not)
			?? ss.Adjacent.First(s=>s.Space.IsSand);
		Presence.Adjust(space2,1);

		// Bestow the Endurance of Bedrock
		gameState.ModifyBlightAddedEffect.ForGame.Add(BestowTheEnduranceOfBedrock);
	}

	void BestowTheEnduranceOfBedrock( AddBlightEffect effect ) {
		// When blight is added to one of your lands,
		// if the blight is less than or equal to your presence, 
		if( effect.Space.Blight.Count <= Presence.CountOn( effect.Space ) ){
			// it does not cascade or destroy presence (yours or others')."
			effect.Cascade = false;
			effect.DestroyPresence = false;
		}
	}

}