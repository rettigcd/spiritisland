namespace SpiritIsland.NatureIncarnate;

public class WanderingVoiceKeensDelirium : Spirit {
	public const string Name = "Wandering Voice Keens Delirium";

	// Draw growth/icons???
	// Images in Cache directory

	static Track SunOrMoon =>  new Track( "Sun/Moon" ) {
		Icon = new IconDescriptor { ContentImg = Img.Icon_Sun, ContentImg2 = Img.Icon_Moon },
		Action = new Gain1Element(Element.Sun,Element.Moon),
	};

	static Track PushYourIncarna => new Track("Push Incarna") {
		Icon = new IconDescriptor { BackgroundImg = Img.Land_Push_Incarna },
		Action = new PushIncarna(),
	};

	public WanderingVoiceKeensDelirium()
		:base(spirit=>
			new IncarnaPresence( spirit, 
				new PresenceTrack(Track.Energy0,Track.Energy1,SunOrMoon, Track.Energy2, Track.AirEnergy, Track.Energy4, PushYourIncarna), 
				new PresenceTrack(Track.Card1,Track.Card2,Track.Card2,Track.Card3,Track.CardReclaim1,Track.Card4),
				new VoiceIncarna(spirit)
			),
			PowerCard.For(typeof(ExhaleConfusionAndDelirium)),
			PowerCard.For(typeof(TwistPerceptions)),
			PowerCard.For(typeof(TurmoilsTouch)),
			PowerCard.For(typeof(FrightfulKeening))
        )
	{
		GrowthTrack = new(
			new GrowthOption(
				new ReclaimAll(),
				new AddOrMoveIncarnaToPresence(),
				new GainEnergy(1)
			),
			new GrowthOption(
				new PlacePresence( 3 ),
				new PlacePresence( 1 )
			),
			new GrowthOption( 
				new GainPowerCard(),
				new PlacePresence( 2 ),
				new GainEnergy( 1 ),
				new GainElements( Element.Air )
			)
		);

		InnatePowers = new InnatePower[]{
			InnatePower.For(typeof(InscrutableJourneying)),
			InnatePower.For(typeof(MindShatteringSong))
		};
	}

	public override string Text => Name;

	public override SpecialRule[] SpecialRules => new SpecialRule[]{
		new SpecialRule(
			"A Clarion Voice Given Form",
			"You have an Incarna.  If empowered , it Isolates its land."
		),
		new SpecialRule(
			"Spread Tumult and Delusion",
			"When your Actions add/move Incarna to a land with Invaders, Add 1 Strife in the destination land. " +
			"In lands with or adjacent to Incarna: if Strife is present, Dahan do not participate in Ravage."
		),
		new SpecialRule(
			"Senseless Roaming",
			"When your Actions add Strife to an Explorer/Invader, you may Push it."
		),
	};

	protected override void InitializeInternal( Board board, GameState gameState ) {
		SpaceState s6 = board[6].Tokens;
		SpaceState s7 = board[7].Tokens;
		// incanra on land #6
		s6.Init(((IncarnaPresence)Presence).Incarna,1);
		// 1 presence in land #6
		s6.Init(Presence.Token,1);
		// 1 presence in land #7
		s7.Init(Presence.Token,1);

		GameState.Current.AddIslandMod( new DahanSitOutRavage( ((IHaveIncarna)Presence).Incarna ) );
	}

	public override SelfCtx BindMyPowers( Spirit spirit ) {
		ActionScope.Current.Upgrader = (x) => new SenslessRoamingTokens( this, x );
		return new SelfCtx( spirit );
	}

	public override SelfCtx BindDefault( Spirit spirit ) {
		ActionScope.Current.Upgrader = (x) => new SenslessRoamingTokens( this, x );
		return new SelfCtx( spirit );
	}

}
