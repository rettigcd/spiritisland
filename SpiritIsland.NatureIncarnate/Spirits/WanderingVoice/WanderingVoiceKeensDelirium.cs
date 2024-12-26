namespace SpiritIsland.NatureIncarnate;

public class WanderingVoiceKeensDelirium : Spirit {
	public const string Name = "Wandering Voice Keens Delirium";

	static Track SunOrMoon =>  new Track( "Sun/Moon", MultiElements.Build(Element.Sun,Element.Moon) ) {
		Icon = new IconDescriptor { ContentImg = Img.Icon_Sun, ContentImg2 = Img.Icon_Moon },
	};

	static Track PushYourIncarna => new Track("Push Incarna") {
		Icon = new IconDescriptor { BackgroundImg = Img.Land_Push_Incarna },
		Action = new PushIncarna(),
	};

	public WanderingVoiceKeensDelirium()
		:base(spirit=>
			new SpiritPresence( spirit, 
				new PresenceTrack(Track.Energy0,Track.Energy1,SunOrMoon, Track.Energy2, Track.AirEnergy, Track.Energy4, PushYourIncarna), 
				new PresenceTrack(Track.Card1,Track.Card2,Track.Card2,Track.Card3,Track.CardReclaim1,Track.Card4),
				new AClarionVoiceGivenForm(spirit)
			),
			new GrowthTrack(
				new GrowthGroup( new ReclaimAll(), new MoveIncarnaToPresence(), new GainEnergy( 1 ) ),
				new GrowthGroup( new PlacePresence( 3 ), new PlacePresence( 1 ) ),
				new GrowthGroup( new GainPowerCard(), new PlacePresence( 2 ), new GainEnergy( 1 ), new GainAllElements( Element.Air ) )
			),
			PowerCard.ForDecorated(ExhaleConfusionAndDelirium.ActAsync),
			PowerCard.ForDecorated(TwistPerceptions.ActAsync),
			PowerCard.ForDecorated(TurmoilsTouch.ActAsync),
			PowerCard.ForDecorated(FrightfulKeening.ActAsync)
		)
	{

		InnatePowers = [
			InnatePower.For(typeof(InscrutableJourneying)),
			InnatePower.For(typeof(MindShatteringSong))
		];

		SpecialRules = [
			AClarionVoiceGivenForm.Rule,
			SpreadTumultAndDelusion.Rule,
			SenselessRoaming.Rule
		];
	}

	public override string SpiritName => Name;

	protected override void InitializeInternal( Board board, GameState gameState ) {
		Space s6 = board[6].ScopeSpace;
		Space s7 = board[7].ScopeSpace;
		// incanra on land #6
		s6.Init(Presence.Incarna,1);
		// 1 presence in land #6
		s6.Init(Presence.Token,1);
		// 1 presence in land #7
		s7.Init(Presence.Token,1);

		gameState.AddIslandMod( new SpreadTumultAndDelusion( this ) );
		gameState.AddIslandMod( new SenselessRoaming(this));
	}

}
