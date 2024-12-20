namespace SpiritIsland.Basegame;

/// <summary> Added to each Ocean to handle drowning </summary>
class Drowning( Ocean ocean ) : BaseModEntity, IHandleTokenAdded {

	static public Drowner GetDrowner() => new Drowner();

	public static SpecialRule Rule => new SpecialRule(
		"Drowning",
		"Destroy Drowned pieces.  At any time you may exchange [# of players] Health of these Invaders for 1 Energy."
	);

	#region IHandledTokenAdded

	async Task IHandleTokenAdded.HandleTokenAddedAsync( Space to, ITokenAddedArgs args ) {

		if(args.Added is not HumanToken ht) return;

		// If we are saving a dahan
		if(ht.HumanClass.HasTag(TokenCategory.Dahan) && Ocean.ShouldSaveDahan() && CanSaveDahanOnSpace(to) )
			if( await SaveDahan(args) )
				return;

		// Drown them immediately
		ActionScope.Current.Log( new Log.Debug( $"Drowning {args.Count}{ht.SpaceAbreviation} on {args.To.Text}" ) );
		await to.Invaders.DestroyNTokens( ht, args.Count );

		// Track drowned invaders' health
		if(args.Added.HasTag(TokenCategory.Invader))
			_drownedInvaderHealthAccumulator += (ht.FullHealth * args.Count);

		CashInDrownedHealthForEnergy();
	}

	#endregion IHandledTokenAdded

	#region private methods

	// Is this correct?  Does Ocean have to be in the ocean or can it just be on the board?
	bool CanSaveDahanOnSpace( Space to ) => to.Has( _ocean.Presence );

	async Task<bool> SaveDahan(ITokenAddedArgs args) {

		var destinationOptions = GameState.Current.Island.Boards
			.Select(x => x.Ocean)
			.ScopeTokens()
			.SelectMany(x => x.Adjacent)
			.Distinct()
			.ToArray();

		// And Ocean chooses to save them (may be more than 1)
		// For now save them all to the same spot.
		var moves = new SpaceToken((Space)args.To,args.Added).BuildMoves(destinationOptions);
		var move = await _ocean.Select("Save Dahan from Drowning",moves, Present.Done);
		if( move is null) return false;


		// Move all of them at the end of the Action. (Let everyone handle the move-event before we move them again)
		ActionScope.Current.AtEndOfThisAction(async _ => {
			//don't use original because that may or may not have been for a power.
			await using ActionScope childAction = await ActionScope.Start(ActionCategory.Default);
			await move.Apply(args.Count);
		});
		return true;
	}

	void CashInDrownedHealthForEnergy() {
		int earnedEnergy = _drownedInvaderHealthAccumulator / SpiritCount;
		if(earnedEnergy == 0) return;

		int cashedInHealth = SpiritCount * earnedEnergy;
		ActionScope.Current.Log( new Log.Debug( $"Ocean gained {earnedEnergy} energy from cashing in {cashedInHealth} health of drowned invaders." ) );

		// Update Ocean
		_drownedInvaderHealthAccumulator -= cashedInHealth;
		_ocean.Energy += earnedEnergy;
	}

	#endregion

	#region private fields

	int SpiritCount => _spiritCount ??= GameState.Current.Spirits.Length;
	int? _spiritCount;

	int _drownedInvaderHealthAccumulator = 0;

	readonly Ocean _ocean = ocean;

	#endregion
}

class Drowner {
	public Drowner() {
		_drowningSpace = GameState.Current.Spirits.Single(x => x is Ocean)
			.Presence.Lands.First().SpaceSpec // find any space the ocean has presnece
			.Boards[0].Ocean.ScopeSpace; // find the Ocean space on that board
	}
	public Task Drown(SpaceToken spaceToken, int count=1) => spaceToken.MoveTo(_drowningSpace,count);
	readonly Space _drowningSpace;
}