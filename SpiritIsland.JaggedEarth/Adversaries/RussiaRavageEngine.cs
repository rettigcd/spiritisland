namespace SpiritIsland.JaggedEarth;

class RussiaRavageEngine : RavageEngine {

	#region private fields

	readonly bool _hasCompetitionAmongHunters;
	readonly bool _hasPressureForFastProfit;
	readonly RussiaToken _token;

	#endregion;

	#region constructor

	public RussiaRavageEngine( int level, RussiaToken token ) {
		_hasCompetitionAmongHunters = 3 <= level; // Matching Card
		_hasPressureForFastProfit = 6 <= level; // alters Ravage
		_token = token;
	}

	#endregion

	// After Ravage, on each board where it added no Blight: In the land with the most Explorer( min. 1), add 1 Explorer and 1 Town.
	public override async Task ActivateCard( InvaderCard card, GameState gameState ) {
		_token.PreRavage();
		await base.ActivateCard( card,gameState );
		if(_hasPressureForFastProfit)
			_token.PressureForFastProfit( gameState );
	}

	protected override bool MatchesCardForRavage( InvaderCard card, SpaceState spaceState ) => base.MatchesCardForRavage(card,spaceState )
		|| _hasCompetitionAmongHunters && 3 <= spaceState.Sum( Invader.Explorer );

}