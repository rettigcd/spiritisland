namespace SpiritIsland.JaggedEarth;

public partial class ManyMindsMoveAsOne : Spirit {

	public const string Name = "Many Minds Move as One";

	public override string Text => Name;

	public override SpecialRule[] SpecialRules => new SpecialRule[]{ FlyFastAsThought, AJoiningOfSwarmsAndFlocks };

	static readonly SpecialRule AJoiningOfSwarmsAndFlocks = new SpecialRule(
		"A Joining of Swarms and Flocks",
		"Your Sacred Sites may also count as beast. If something change a beast that is your presence, it affects 2 of your Presence there."
	);
	static readonly SpecialRule FlyFastAsThought = new SpecialRule(
		"Fly Fast as Thought",
		"When you Gather or Push Beast, they may come from or go to lands up to 2 distant."
	);

	static Track CardBoost => new Track( "Pay2ForExtraPlay" ) { 
		Action = new Pay2EnergyToGainAPowerCard(),
		Icon = new IconDescriptor { ContentImg = Img.GainCard,
			Super = new IconDescriptor { BackgroundImg = Img.Coin, Text= "—2" }
		}
	};

	public ManyMindsMoveAsOne()
		:base(
			spirit => new SpiritPresence( spirit,
				new PresenceTrack(Track.Energy0,Track.Energy1,Track.MkEnergyElements(Element.Air),Track.Energy2,Track.MkEnergyElements(Element.Animal),Track.Energy3,Track.Energy4),
				new PresenceTrack(Track.Card1,Track.Card2,CardBoost,Track.Card3,Track.Card3,Track.Card4,Track.Card5),
				new ManyMindsPresenceToken( spirit )
			)
			, new GrowthTrack(
				new GrowthOption( new ReclaimAll(), new GainPowerCard() ),
				new GrowthOption( new PlacePresence( 1 ), new PlacePresence( 0 ) ),
				new GrowthOption( new PlacePresenceAndBeast(), new GainEnergy( 1 ), new Gather1Token( 2, Token.Beast ) )
			)
			, PowerCard.For(typeof(ADreadfulTideOfScurryingFlesh))
			, PowerCard.For(typeof(BoonOfSwarmingBedevilment))
			, PowerCard.For(typeof(EverMultiplyingSwarm))
			, PowerCard.For(typeof(GuideTheWayOnFeatheredWings))
			, PowerCard.For(typeof(PursueWithScratchesPecksAndStings))
		) {

		InnatePowers = new InnatePower[] {
			InnatePower.For(typeof(TheTeemingHostArrives)), 
			InnatePower.For(typeof(BesetAndConfoundTheInvaders))
		};

	}

	protected override void InitializeInternal( Board board, GameState gameState ) {
		// Put 1 presence and 1 beast on yoru starting borad, in a land with beast.
		var land = board.Spaces.Tokens().First( x => x.Beasts.Any );

		land.Adjust(Presence.Token, 1);
		land.Beasts.Init(1);

	}

	public override void InitSpiritAction( ActionScope scope ) {
		ActionScope.Current.Upgrader = x => new ManyMindTokens( x );
	}

}