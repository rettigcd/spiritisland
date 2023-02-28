namespace SpiritIsland.Basegame;

class Drowning : BaseModEntity, IHandleTokenAddedAsync {

	public static SpecialRule Rule => new SpecialRule(
		"Drowning",
		"Destroy Drowned pieces.  At any time you may exchange [# of players] Health of these Invaders for 1 Energy."
	);


	readonly Spirit _spirit;
	int drownedInvaderHealthAccumulator = 0;

	public Drowning( Ocean ocean ) { _spirit = ocean; }

	public async Task HandleTokenAddedAsync( ITokenAddedArgs args ) {

		if(args.Added is not HumanToken ht) return;
		var gs = GameState.Current;

		// If we are saving a dahan
		if(ht.Class.Category == TokenCategory.Dahan && Ocean.ShouldSaveDahan() && args.To.Has( _spirit.Presence.Token )) {
			var moveOptions = gs.Island.Boards
				.Select( x => x.Ocean )
				.Tokens()
				.SelectMany( x => x.Adjacent )
				.Distinct()
				.ToArray();
			// And Ocean chooses to save it
			var destination = await _spirit.Gateway.Decision( Select.ASpace.PushToken( args.Added, args.To.Space, moveOptions, Present.Done ) );
			if(destination != null) {
				// Move them at the end of the Action. (Let everyone handle the move-event before we move them again)
				ActionScope.Current.AtEndOfThisAction( async _ => {
					//don't use original because that may or may not have been for a power.
					await using ActionScope childAction = await ActionScope.Start(ActionCategory.Default);
					await _spirit.BindSelf()
						.Move( args.Added, args.To.Space, destination );
				} );
				return; // the move it, don't drown it
			}
		}

		// Drown them immediately
		gs.Log( new Log.Debug( $"Drowning {args.Count}{ht.SpaceAbreviation} on {args.To.Space}" ) );
		await new InvaderBinding( args.To.Space.Tokens ).DestroyNTokens( ht, args.Count );

		// Track drowned invaders' health
		if(args.Added.Class.Category == TokenCategory.Invader)
			drownedInvaderHealthAccumulator += (ht.FullHealth * args.Count);
		CashInDrownedHealthForEnergy( gs );

	}

	void CashInDrownedHealthForEnergy( GameState gs ) {
		int spiritCount = gs.Spirits.Length;
		int earnedEnergy = drownedInvaderHealthAccumulator / spiritCount;
		if(earnedEnergy == 0) return;

		int cashedInHealth = spiritCount * earnedEnergy;
		gs.Log( new Log.Debug( $"Ocean gained {earnedEnergy} energy from cashing in {cashedInHealth} health of drowned invaders." ) );

		// Update Ocean
		drownedInvaderHealthAccumulator -= cashedInHealth;
		_spirit.Energy += earnedEnergy;
	}

}