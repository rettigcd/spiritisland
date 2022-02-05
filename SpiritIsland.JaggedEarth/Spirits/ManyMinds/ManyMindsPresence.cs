﻿namespace SpiritIsland.JaggedEarth;

class ManyMindsPresence : SpiritPresence {

	ManyMindsMoveAsOne spirit;

	public ManyMindsPresence(PresenceTrack energy, PresenceTrack cards ) : base( energy, cards ) {}

	public void Watch(GameState gs,ManyMindsMoveAsOne spirit) {
		this.spirit = spirit; 
		gs.Tokens.TokenMoved.ForGame.Add( TokenMoved );
		gs.Tokens.TokenRemoved.ForGame.Add( TokenRemoved );
	}

	public override async Task PlaceOn( Space space, GameState gs ) {
		await base.PlaceOn( space, gs );
		// if created sacred site, create virtual beast
		if(CountOn( space ) == 2)
			gs.Tokens[space].Adjust(TokenType.Beast,1); // virtual so don't trigger an event.
	}

	protected override async Task RemoveFrom_NoCheck( Space space, GameState gs ) {
		await base.RemoveFrom_NoCheck( space, gs );
		// if destroyed sacred site, remove virtual beast
		if( CountOn( space ) == 1 )
			gs.Tokens[space].Adjust(TokenType.Beast, 1); // !!! virtual tokens don't trigger events
	}

	async Task TokenMoved( TokenMovedArgs args ) {
		if(args.Token != TokenType.Beast) return; // not a beast
		if(this.CountOn( args.RemovedFrom ) < 2) return; // not our Sacred Site


		var srcBeasts = args.GameState.Tokens[args.RemovedFrom].Beasts;
		if(srcBeasts.Count > 0 // force moved our virtual beast
			&& await spirit.Action.Decision( Select.DeployedPresence.Gather( "Move 2 presence with Beast?", args.AddedTo, new []{ args.RemovedFrom } ) ) == null
		) return; // not moving presence

		Move2Presence( args.GameState, args );

		await SacredSiteAtSouce_RestoreVirtualBeast( args, srcBeasts );

		await AddedVirtualBeastAtDestination_LimitTo1( args.GameState, args );

	}

	void Move2Presence( GameState gs, TokenMovedArgs args ) {
		// Move 2 of our presence
		for(int i = 0; i < 2; ++i) {
			base.RemoveFrom_NoCheck( args.RemovedFrom, gs ); // using base because we don't want to trigger anything
			base.PlaceOn( args.AddedTo, gs );
		}
	}

	async Task SacredSiteAtSouce_RestoreVirtualBeast( TokenMovedArgs args, TokenBinding srcBeasts ) {
		if(2 <= CountOn( args.RemovedFrom ))
			await srcBeasts.Add(1); // Beast token is virtual so maybe we don't want to trigger TokenAdded
	}

	Task AddedVirtualBeastAtDestination_LimitTo1( GameState gs, TokenMovedArgs args ) {
		// if destination/to now has 4 or more presence,
		// then there was already a virtual beast there and we need to remove 1 of the virtual beasts
		if(4 <= CountOn( args.AddedTo ))
			gs.Tokens[args.AddedTo].Beasts.Adjust(-1); // don't trigger event
		return Task.CompletedTask;
	}

	async Task TokenRemoved( ITokenRemovedArgs args ) {
		if(args.Reason.IsDestroy())
			await TokenDestroyed(args);
	}

	async Task TokenDestroyed(ITokenRemovedArgs args) {
		if(args.Token.Class != TokenType.Beast) return; // not a beast
		if(this.CountOn(args.Space)<2) return; // not our Sacred Site
		if(args.GameState.Tokens[args.Space].Beasts.Count == 0){ // no more beasts
			// Destroying the sacred-site beast, destroyes the sacred site

			// the only time we care about what destroyed the presence is for Vengencence as a burning plague.
			// since this is Many Minds, we don't care about that value.
			var dontCareActionType = ActionType.None;

			// destroy saved site
			await base.Destroy(args.Space, args.GameState, dontCareActionType);
			await base.Destroy(args.Space, args.GameState, dontCareActionType);
		}
	}

}