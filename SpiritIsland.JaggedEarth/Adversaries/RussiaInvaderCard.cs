namespace SpiritIsland.JaggedEarth;

// Adds Escalation
class RussiaInvaderCard : InvaderCard {

	#region private fields

	readonly bool _hasCompetitionAmongHunters;
	readonly bool _hasPressureForFastProfit;
	readonly RussiaToken _token;

	#endregion;

	public RussiaInvaderCard( InvaderCard card, int level, RussiaToken token ) : base( card ) {
		_hasCompetitionAmongHunters = 3 <= level; // Matching Card
		_hasPressureForFastProfit = 6 <= level; // alters Ravage
		_token = token;
	}

	// After Ravage, on each board where it added no Blight: In the land with the most Explorer( min. 1), add 1 Explorer and 1 Town.
	public override async Task Ravage( GameState gameState ) {
		_token.PreRavage();
		await base.Ravage( gameState );
		if(_hasPressureForFastProfit)
			_token.PressureForFastProfit( gameState );
	}

	protected override bool MatchesCardForRavage( SpaceState spaceState ) => MatchesCard( spaceState )
		|| _hasCompetitionAmongHunters && 3 <= spaceState.Sum( Invader.Explorer );

}

