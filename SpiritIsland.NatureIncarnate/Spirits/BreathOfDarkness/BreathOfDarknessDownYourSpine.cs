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
			new GrowthOption( new ReclaimAll(), new GainPowerCard(), new MoveIncarnaAnywhere(), new PiecesEscape(int.MaxValue) ),
			new GrowthOption( new GainPowerCard(), new PlacePresence(3), new PiecesEscape(2) ),
			new GrowthOption( new PlacePresence(1), new AddOrMoveIncarnaToPresence(), new PiecesEscape(1), new GainEnergyEqualToCardPlays() )
		);

		// Innates
		InnatePowers = new InnatePower[] {
			InnatePower.For(typeof(LeaveATrailOfDeathlySilence)),
			InnatePower.For(typeof(LostInTheEndlessDark))
		};

		PowerRangeCalc = new ShadowTouchedRealm_RangeCalculator();
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

		gameState.TimePasses_WholeGame += _ => { _usedEmpoweredAbduct = false; return Task.CompletedTask; };

		gameState.OtherSpaces.Add(EndlessDark.Space);
	}

	public BreathIncarna Incarna => ((BreathPresence)Presence).Incarna;

	public override SelfCtx BindMyPowers( Spirit spirit ) {
		ActionScope.Current.Upgrader = x => new TerrorStalksTheLand( x );
		return new SelfCtx( spirit );
	}

	public override IEnumerable<IActionFactory> GetAvailableActions( Phase speed ) {
		foreach(var action in base.GetAvailableActions( speed )) yield return action;
		if( speed == Phase.Fast && ((BreathPresence)Presence).Incarna.Empowered && !_usedEmpoweredAbduct )
			yield return _ea;
	}

	static readonly EmpoweredAbduct _ea = new EmpoweredAbduct();
	bool _usedEmpoweredAbduct;

	public override void RemoveFromUnresolvedActions( IActionFactory selectedActionFactory ) { 
		if(selectedActionFactory == _ea)
			_usedEmpoweredAbduct = true;
		else 
			base.RemoveFromUnresolvedActions( selectedActionFactory );
	}

	// !! Managing the 2nd _empowerAbduct separately from _availableActions is much harder than just stuffing it in _availabeActions would be.

}
