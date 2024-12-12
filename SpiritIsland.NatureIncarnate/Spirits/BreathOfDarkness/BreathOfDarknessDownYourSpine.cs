namespace SpiritIsland.NatureIncarnate;

public class BreathOfDarknessDownYourSpine : Spirit {

	public const string Name = "Breath of Darkness Down Your Spine";

	public override string SpiritName => Name;

	#region Custom Presence

	static Track EmpowerIncarnaTrack => new Track("Empower"){
		Action = new EmpowerIncarna(),
		Icon = new IconDescriptor {  BackgroundImg = Img.BoDDYS_Incarna_Empowered },
	};

	static Track MovePresence => new Track( "Moveonepresence.png" ) {
		Action = new MovePresence(1),
		Elements = [Element.Air],
		Icon = new IconDescriptor { BackgroundImg = Img.MovePresence }
	};

	static Track Card4Air => new Track("4 cardplay,air", Element.Air ) {
		Icon = new IconDescriptor {
			BackgroundImg = Img.CardPlay,
			Text = "4",
			Sub = new IconDescriptor { BackgroundImg = Element.Air.GetTokenImg() }
		}
	};

	#endregion Custom Presence

	public BreathOfDarknessDownYourSpine() : base(
		spirit => new SpiritPresence( spirit,
			new PresenceTrack( Track.Energy1, Track.Energy2, Track.MoonEnergy, Track.Energy3, EmpowerIncarnaTrack, Track.MkEnergy(4,Element.Animal), Track.MkEnergy(5,Element.Air) ),
			new PresenceTrack( Track.Card2, MovePresence, Track.Card3, Track.MoonEnergy, Track.CardReclaim1, Card4Air ),
			new Incarna(spirit,"BoDDYS", Img.BoDDYS_Incarna_Empowered, Img.BoDDYS_Incarna ) { }
		)
		, new GrowthTrack(
			new GrowthGroup( new ReclaimAll(), new GainPowerCard(), new MoveIncarnaAnywhere(), new PiecesEscape( int.MaxValue ) ),
			new GrowthGroup( new GainPowerCard(), new PlacePresence( 3 ), new PiecesEscape( 2 ) ),
			new GrowthGroup( new PlacePresence( 1 ), new AddOrMoveIncarnaToPresence(), new PiecesEscape( 1 ), new GainEnergyEqualToCardPlays() )
		)
		, PowerCard.For(typeof(ReachFromTheInfiniteDarkness))	// fast
		, PowerCard.For(typeof(SwallowedByTheEndlessDark))		// fast
		, PowerCard.For(typeof(EmergeFromTheDreadNightWind))	// slow
		, PowerCard.For(typeof(TerrorOfTheHunted))				// slow
	) {

		// Innates
		InnatePowers = [
			InnatePower.For(typeof(LeaveATrailOfDeathlySilence)),
			InnatePower.For(typeof(LostInTheEndlessDark))
		];

		SpecialRules = [
			TerrorStalksTheLand.Rule,
			ShadowTouchedRealm_RangeCalculator.Rule
		];

		PowerRangeCalc = new ShadowTouchedRealm_RangeCalculator();

		Mods.Add( new EnableEmpoweredAbductMod(this) );
	}

	protected override void InitializeInternal( Board board, GameState gameState ) {
		var jungles = board.Spaces.Where(x=>x.IsJungle).ScopeTokens().ToArray();
		// 1+Incarna in lowest jungle
		jungles[0].Init(Presence.Token,1);
		jungles[0].Init( Incarna, 1 );
		// 1 in highest jungle
		jungles[1].Init( Presence.Token, 1 );

		gameState.OtherSpaces.Add(EndlessDark.Space);
	}

	public override void InitSpiritAction(ActionScope scope) {
		if( scope.Category == ActionCategory.Spirit_Power )
			scope.Upgrader = x => new TerrorStalksTheLand(x);
	}

}
