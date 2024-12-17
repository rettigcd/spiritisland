namespace SpiritIsland.NatureIncarnate;

public class RelentlessGazeOfTheSun : Spirit {

	public const string Name = "Relentless Gaze of the Sun";

	public override string SpiritName => Name;

	public RelentlessGazeOfTheSun():base( 
		spirit => new SunPresence(spirit)
		, new GrowthTrack(
			new GrowthGroup( new PlacePresence( 2 ) )
		).Add( new GrowthPickGroups( 1,
			new GrowthGroup(
				new ReclaimAll(),
				new AddDestroyedPresence( 1 ).SetNumToPlace( 3, Present.Done )
			),
			new GrowthGroup(
				new GainPowerCard()
			),
			new GrowthGroup(
				new GainEnergyAnAdditionalTime(),
				new MovePresenceTogether()
			)
		) )
		,PowerCard.ForDecorated(BlindingGlare.ActAsync)
		,PowerCard.ForDecorated(UnbearableGaze.ActAsync)
		,PowerCard.ForDecorated(WitherBodiesScarStones.ActAsync)
		,PowerCard.ForDecorated(FocusTheSunsRays.ActAsync)
	) {
		InnatePowers = [
			InnatePower.For(typeof(ScorchingConvergence)), 
			InnatePower.For(typeof(ConsiderAHarmoniousNature))
		];
		SpecialRules = [RelentlessPunishment.Rule];

		Mods.Add(new RelentlessPunishment(this));
	}

	protected override void InitializeInternal( Board board, GameState gs ) {
		// Put 2 presence and 1 Badlands on your starting board
		var start = board.Spaces.First(s=>s.IsSand);
		start.ScopeSpace.Setup(Presence.Token, 2);
		start.ScopeSpace.Badlands.Init(1);
	}

}
