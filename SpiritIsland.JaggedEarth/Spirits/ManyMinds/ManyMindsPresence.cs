namespace SpiritIsland.JaggedEarth;

class ManyMindsPresence : SpiritPresence {

	public ManyMindsPresence(PresenceTrack energy, PresenceTrack cards ) : base( energy, cards ) {}

	public void Watch( GameState gs ) {
		gs.Tokens.TokenMoved.ForGame.Add( TokenMoved );
		gs.AddToAllActiveSpaces( new TokenRemovedHandler( TokenRemoved, true ) );
	}

	public override async Task PlaceOn( SpaceState space, UnitOfWork actionScope ) {
		await base.PlaceOn( space, actionScope );
		// if created sacred site, create virtual beast
		if(CountOn( space ) == 2)
			space.Adjust( SpiritIsland.Token.Beast,1); // virtual so don't trigger an event.
	}

	protected override async Task RemoveFrom_NoCheck( SpaceState space, int count ) {
		await base.RemoveFrom_NoCheck( space, count );
		// if destroyed sacred site, remove virtual beast
		if( CountOn( space ) == 1 )
			space.Adjust( SpiritIsland.Token.Beast, 1); // !!! virtual tokens don't trigger events
	}

	async Task TokenMoved( ITokenMovedArgs args ) {
		if(args.TokenRemoved.Class != SpiritIsland.Token.Beast) return; // not a beast
		if(this.CountOn( args.RemovedFrom ) < 2) return; // not our Sacred Site


		var srcBeasts = args.RemovedFrom.Beasts;
		if(srcBeasts.Count > 0 // force moved our virtual beast
			&& await Self.Gateway.Decision( Select.DeployedPresence.Gather( 
				"Move 2 presence with Beast?", 
				args.AddedTo.Space, 
				new []{ args.RemovedFrom },
				Token
			) ) == null
		) return; // not moving presence

		await Move2Presence( args.AddedTo.AccessGameState(), args );

		await SacredSiteAtSouce_RestoreVirtualBeast( args, srcBeasts.Bind(args.ActionScope) );

		await AddedVirtualBeastAtDestination_LimitTo1( args.AddedTo.AccessGameState(), args );

	}

	async Task Move2Presence( GameState gs, ITokenMovedArgs args ) {
		// Move 2 of our presence
		for(int i = 0; i < 2; ++i) {
			await base.RemoveFrom_NoCheck( gs.Tokens[args.RemovedFrom.Space] ); // using base because we don't want to trigger anything
			await base.PlaceOn( args.AddedTo, args.ActionScope );
		}
	}

	async Task SacredSiteAtSouce_RestoreVirtualBeast( ITokenMovedArgs args, TokenBinding srcBeasts ) {
		if(2 <= CountOn( args.RemovedFrom ))
			await srcBeasts.Add(1); // Beast token is virtual so maybe we don't want to trigger TokenAdded
	}

	Task AddedVirtualBeastAtDestination_LimitTo1( GameState gs, ITokenMovedArgs args ) {
		// if destination/to now has 4 or more presence,
		// then there was already a virtual beast there and we need to remove 1 of the virtual beasts
		if(4 <= CountOn( args.AddedTo ))
			gs.Tokens[args.AddedTo.Space].Beasts.Adjust(-1); // don't trigger event
		return Task.CompletedTask;
	}

	async Task TokenRemoved( ITokenRemovedArgs args ) {
		if(args.Reason.IsDestroy())
			await TokenDestroyed(args);
	}

	async Task TokenDestroyed(ITokenRemovedArgs args) {
		if(args.Token.Class != SpiritIsland.Token.Beast) return; // not a beast
		if(this.CountOn(args.RemovedFrom)<2) return; // not our Sacred Site
		if(args.RemovedFrom.Beasts.Count == 0){ // no more beasts
			// Destroying the sacred-site beast, destroyes the sacred site

			// the only time we care about what destroyed the presence is for Vengencence as a burning plague.
			// since this is Many Minds, we don't care about that value.
			var dontCareActionType = DestoryPresenceCause.None;

			// destroy sacred site
			await base.Destroy(args.RemovedFrom.Space, args.RemovedFrom.AccessGameState(), 2, dontCareActionType, args.ActionScope);
		}
	}

}