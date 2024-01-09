namespace SpiritIsland.NatureIncarnate;

public class RelentlessGazeOfTheSun : Spirit {

	public const string Name = "Relentless Gaze of the Sun";

	public override string Text => Name;

	public override SpecialRule[] SpecialRules => new SpecialRule[] {
		RelentlessRepeater.Rule
	};

	public RelentlessGazeOfTheSun():base( 
		spirit => new SunPresence(spirit)
		, new GrowthTrack(
			new GrowthOption( new PlacePresence( 2 ) )
		).Add( new GrowthPickGroups( 1,
			new GrowthOption(
				new ReclaimAll(),
				new AddDestroyedPresence( 1 ).SetNumToPlace( 3, Present.Done )
			),
			new GrowthOption(
				new GainPowerCard()
			),
			new GrowthOption(
				new GainEnergyAnAdditionalTime(),
				new MovePresenceTogether()
			)
		) )
		, PowerCard.For(typeof(BlindingGlare))
		,PowerCard.For(typeof(UnbearableGaze))
		,PowerCard.For(typeof(WitherBodiesScarStones))
		,PowerCard.For(typeof(FocusTheSunsRays))
	) {
		InnatePowers = new InnatePower[] {
			InnatePower.For(typeof(ScorchingConvergence)), 
			InnatePower.For(typeof(ConsiderAHarmoniousNature))
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

	/// <remarks>overriden to capture space played on and queue up repeat</remarks>
	public override async Task<Space> TargetsSpace(
		string prompt,
		IPreselect preselect,
		TargetingSourceCriteria sourceCriteria,
		params TargetCriteria[] targetCriteria
	) {
		var targetSpace = await base.TargetsSpace( prompt, preselect, sourceCriteria, targetCriteria );

		if(_currentPowerCard != null    // is power card
			&& TargetsFromSuperSacredSite( targetSpace, sourceCriteria, targetCriteria )
		) {
			AddActionFactory( new RelentlessRepeater( _currentPowerCard, targetSpace ) );
			_currentPowerCard = null; // clear so we don't accidentally add twice
		}

		return targetSpace;
	}

	bool TargetsFromSuperSacredSite( Space targetSpace, TargetingSourceCriteria sourceCriteria, TargetCriteria[] targetCriteria ) {
		return FindTargettingSourcesFor(
			targetSpace,
			new TargetingSourceCriteria( TargetFrom.SuperSacredSite, sourceCriteria.Restrict ), // capture restrict but boost
			targetCriteria
		).Any();
	}

	PowerCard? _currentPowerCard; // when playing power cards, grab it so we know what to repeat

	#endregion Relentless Punishment

	#region Collect Energy x2

	public bool CollectEnergySecondTime;

	public override Task DoGrowth( GameState gameState ) {
		CollectEnergySecondTime = false;
		return base.DoGrowth( gameState );
	}

	protected override async Task ApplyRevealedPresenceTracks_Inner( Spirit self ) { 
		if(CollectEnergySecondTime)
			Energy += EnergyPerTurn;

		await base.ApplyRevealedPresenceTracks_Inner( self );
	}

	#endregion

	#region Memento

	protected override object CustomMementoValue {
		get => new GazeMemento(this);
		set => ((GazeMemento)value).Restore(this);
	}
	class GazeMemento {
		public GazeMemento(RelentlessGazeOfTheSun spirit) {
			_b = spirit.CollectEnergySecondTime;
			_card = spirit._currentPowerCard;
		}
		public void Restore(RelentlessGazeOfTheSun spirit) {
			spirit.CollectEnergySecondTime = _b;
			spirit._currentPowerCard = _card;
		}
		readonly bool _b;
		readonly PowerCard? _card;
	}
	#endregion Memento
}
