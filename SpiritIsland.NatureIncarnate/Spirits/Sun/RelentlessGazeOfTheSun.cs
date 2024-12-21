namespace SpiritIsland.NatureIncarnate;

public class RelentlessGazeOfTheSun : Spirit {

	public const string Name = "Relentless Gaze of the Sun";
	public override string SpiritName => Name;

	#region Tracks
	static Track E2Sun => Track.MkEnergy(2, Element.Sun);
	static Track E3Fire => Track.MkEnergy(3, Element.Fire);
	static Track E4Any => Track.MkEnergy(4, Element.Any);
	static PresenceTrack EnergyTrack => new PresenceTrack(Track.Energy1, E2Sun, E3Fire, Track.SunEnergy, E4Any, Track.Energy5);
	static PresenceTrack CardPlayTrack => new PresenceTrack(Track.Card1, Track.Card1, Track.Card2, Track.SunEnergy, Track.Card3, Track.CardReclaim1, Track.Card4);
	#endregion Tracks

	public RelentlessGazeOfTheSun():base( 
		// Presence
		spirit => new SpiritPresence( spirit, EnergyTrack, CardPlayTrack )
		// Growth Track
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
		// Power Cards
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
