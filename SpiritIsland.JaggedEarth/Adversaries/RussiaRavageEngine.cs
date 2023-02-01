namespace SpiritIsland.JaggedEarth;

class RussiaRavageEngine : RavageEngine {

	#region private fields

	public bool ShouldCheckCompetitionAmongHunters { get; set; }
	public bool CheckForPressureForFastProfit { get; set; }
	readonly RussiaToken _token;

	#endregion;

	#region constructor

	public RussiaRavageEngine( RussiaToken token ) {
		_token = token;
	}

	#endregion

	// After Ravage, on each board where it added no Blight: In the land with the most Explorer( min. 1), add 1 Explorer and 1 Town.
	public override async Task ActivateCard( InvaderCard card, GameState gameState ) {
		_token.PreRavage();
		await base.ActivateCard( card,gameState );
		if(CheckForPressureForFastProfit)
			_token.PressureForFastProfit( gameState );
	}

	const string CompetitionName = "Competition Among Hunters";
	protected override bool MatchesCardForRavage( InvaderCard card, SpaceState spaceState ) {
		if( base.MatchesCardForRavage( card, spaceState ) ) return true;
		bool hasCompetition = ShouldCheckCompetitionAmongHunters && 3 <= spaceState.Sum( Human.Explorer );
		if(hasCompetition)
			spaceState.AccessGameState().LogDebug($"{CompetitionName} causes ravage on {spaceState.Space.Text}");
		return hasCompetition;
	}

}