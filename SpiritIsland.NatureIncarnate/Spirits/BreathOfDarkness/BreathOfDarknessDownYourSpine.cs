namespace SpiritIsland.NatureIncarnate;

public class BreathOfDarknessDownYourSpine : Spirit {

	public const string Name = "Breath of Darkness Down Your Spine";

	public BreathOfDarknessDownYourSpine() : base(
		new BreathPresence()
		, PowerCard.For<EmergeFromTheDreadNightWind>()
		, PowerCard.For<ReachFromTheInfiniteDarkness>()
		, PowerCard.For<SwallowedByTheEndlessDark>()
		, PowerCard.For<TerrorOfTheHunted>()
	) {
		// Growth
		GrowthTrack = new GrowthTrack(
			new GrowthOption( new ReclaimAll(), new DrawPowerCard(), new MoveIncarnaAnywhere(), new PiecesEscape(int.MaxValue) ),
			new GrowthOption( new DrawPowerCard(), new PlacePresence(3), new PiecesEscape(2) ),
			new GrowthOption( new PlacePresence( 1 ), new AddOrMoveIncarnaToPresence(), new PiecesEscape(1), new GainEnergyEqualToCardPlays() )
		);

		// Innates
		InnatePowers = new InnatePower[] {
			InnatePower.For<LeaveATrailOfDeathlySilence>(),
			InnatePower.For<LostInTheEndlessDark>()
		};

		PowerRangeCalc = new ShadowTouchedRealm_RangeCalculator( this );
	}

	public override string Text => Name;

	public override SpecialRule[] SpecialRules => new SpecialRule[]{
		TerrorStalksTheLand.Rule,
		ShadowTouchedRealm_RangeCalculator.Rule
	};

	protected override void InitializeInternal( Board board, GameState gameState ) {
		var jungles = board.Spaces.Where(x=>x.IsJungle).Tokens().ToArray();
		// 1+Incarna in lowest jungle
		jungles[0].Init(Presence.Token,1);
		jungles[0].Init( Incarna, 1 );
		// 1 in highest jungle
		jungles[1].Init( Presence.Token, 1 );

		gameState.OtherSpaces.Add(EndlessDark.Space);
	}

	public BreathIncarna Incarna => ((BreathPresence)Presence).Incarna;

	public override SelfCtx BindMyPowers( Spirit spirit ) {
		ActionScope.Current.Upgrader = x => new TerrorStalksTheLand( x );
		return new SelfCtx( spirit );
	}


}
