namespace SpiritIsland.NatureIncarnate;

public class RelentlessGazeOfTheSun : Spirit {

	public const string Name = "Relentless Gaze of the Sun";

	public override string Text => Name;

	public override SpecialRule[] SpecialRules => new SpecialRule[] {
		RelentlessRepeater.Rule
	};

	public RelentlessGazeOfTheSun():base( 
		new SunPresence()
		,PowerCard.For<BlindingGlare>()
		,PowerCard.For<UnbearableGaze>()
		,PowerCard.For<WitherBodiesScarStones>()
		,PowerCard.For<FocusTheSunsRays>()
	) {
		GrowthTrack = new GrowthTrack(
			new GrowthOption( new PlacePresence( 2 ) )
		).Add( new GrowthPickGroups( 1,
			new GrowthOption(
				new ReclaimAll(),
				new AddDestroyedPresenceTogether()
			),
			new GrowthOption(
				new GainPowerCard()
			),
			new GrowthOption(
				new GainEnergyAnAdditionalTime(),
				new MovePresenceTogether()
			)
		) );

		InnatePowers = new InnatePower[] {
			InnatePower.For<ScorchingConvergence>(),
			InnatePower.For<ConsiderAHarmoniousNature>()
		};
	}

	protected override void InitializeInternal( Board board, GameState gs ) {
		// Put 2 presence and 1 Badlands on your starting board
		var start = board.Spaces.First(s=>s.IsSand);
		start.Tokens.Adjust(Presence.Token, 2);
		start.Tokens.Badlands.Init(1);
	}

	#region Relentless Punishment

	/// <remarks>overriden to capture current power card being played</remarks>
	public override async Task TakeActionAsync( IActionFactory factory, Phase phase ) {
		_currentPowerCard = factory as PowerCard;
		await base.TakeActionAsync( factory, phase );
		_currentPowerCard = null;
	}
	PowerCard? _currentPowerCard; // when playing power cards, grab it so we know what to repeat

	public override async Task<Space> TargetsSpace(
		SelfCtx ctx, // this has the new Action for this action.
		string prompt,
		IPreselect preselect,
		TargetingSourceCriteria sourceCriteria,
		params TargetCriteria[] targetCriteria
	) {
		var targetSpace = await base.TargetsSpace( ctx, prompt, preselect, sourceCriteria, targetCriteria );

		if(_currentPowerCard != null && SpacesFromTrifecta( targetCriteria ).Contains( targetSpace )) {
			AddActionFactory(new RelentlessRepeater(_currentPowerCard,targetSpace));
			_currentPowerCard = null; // clear so we don't accidentally add twice
		}

		return targetSpace;
	}

	IEnumerable<Space> SpacesFromTrifecta( TargetCriteria[] targetCriteria ) {
		IEnumerable<SpaceState> trifectas = Presence.SacredSites.Where( ss => 3 <= Presence.CountOn( ss ) );
		IEnumerable<Space> spacesFromTrifectas = PowerRangeCalc.GetTargetOptionsFromKnownSource( trifectas, targetCriteria ).Select( ss => ss.Space );
		return spacesFromTrifectas;
	}

	#endregion Relentless Punishment

	#region Collect Energy x2

	public bool CollectEnergySecondTime;

	public override Task DoGrowth( GameState gameState ) {
		CollectEnergySecondTime = false;
		return base.DoGrowth( gameState );
	}

	protected override async Task ApplyRevealedPresenceTracks_Inner( SelfCtx ctx ) { 
		if(CollectEnergySecondTime)
			Energy += EnergyPerTurn;

		await base.ApplyRevealedPresenceTracks_Inner( ctx );
	}

	#endregion

}
