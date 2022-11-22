namespace SpiritIsland.JaggedEarth;

class ManyMindsPresence : SpiritPresence {

	ManyMindsMoveAsOne spirit;

	public ManyMindsPresence(PresenceTrack energy, PresenceTrack cards ) : base( energy, cards ) {}

	public void Watch(GameState gs,ManyMindsMoveAsOne spirit) {
		this.spirit = spirit; 
		gs.Tokens.TokenMoved.ForGame.Add( TokenMoved );
		gs.Tokens.TokenRemoved.ForGame.Add( TokenRemoved );
	}

	public override async Task PlaceOn( SpaceState space ) {
		await base.PlaceOn( space );
		// if created sacred site, create virtual beast
		if(CountOn( space ) == 2)
			space.Adjust(TokenType.Beast,1); // virtual so don't trigger an event.
	}

	protected override async Task RemoveFrom_NoCheck( Space space, GameState gs ) {
		var tokens = gs.Tokens[space];
		await base.RemoveFrom_NoCheck( space, gs );
		// if destroyed sacred site, remove virtual beast
		if( CountOn( tokens ) == 1 )
			gs.Tokens[space].Adjust(TokenType.Beast, 1); // !!! virtual tokens don't trigger events
	}

	async Task TokenMoved( ITokenMovedArgs args ) {
		if(args.Class != TokenType.Beast) return; // not a beast
		if(this.CountOn( args.RemovedFrom ) < 2) return; // not our Sacred Site


		var srcBeasts = args.RemovedFrom.Beasts;
		if(srcBeasts.Count > 0 // force moved our virtual beast
			&& await spirit.Action.Decision( Select.DeployedPresence.Gather( "Move 2 presence with Beast?", args.AddedTo.Space, new []{ args.RemovedFrom.Space } ) ) == null
		) return; // not moving presence

		Move2Presence( args.GameState, args );

		await SacredSiteAtSouce_RestoreVirtualBeast( args, srcBeasts.Bind(args.ActionId) );

		await AddedVirtualBeastAtDestination_LimitTo1( args.GameState, args );

	}

	void Move2Presence( GameState gs, ITokenMovedArgs args ) {
		// Move 2 of our presence
		for(int i = 0; i < 2; ++i) {
			base.RemoveFrom_NoCheck( args.RemovedFrom.Space, gs ); // using base because we don't want to trigger anything
			base.PlaceOn( args.AddedTo );
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
		if(args.Token.Class != TokenType.Beast) return; // not a beast
		if(this.CountOn(args.Space)<2) return; // not our Sacred Site
		if(args.Space.Beasts.Count == 0){ // no more beasts
			// Destroying the sacred-site beast, destroyes the sacred site

			// the only time we care about what destroyed the presence is for Vengencence as a burning plague.
			// since this is Many Minds, we don't care about that value.
			var dontCareActionType = DestoryPresenceCause.None;

			// destroy sacred site
			await base.Destroy(args.Space.Space, args.GameState, dontCareActionType);
			await base.Destroy(args.Space.Space, args.GameState, dontCareActionType);
		}
	}

}