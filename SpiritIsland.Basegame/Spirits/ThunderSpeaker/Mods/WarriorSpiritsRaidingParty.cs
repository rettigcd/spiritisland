namespace SpiritIsland.Basegame;

class WarriorSpiritsRaidingParty : Incarna, IHandleTokenRemoved {

	#region Rule

	public const string Name = "Warrior-Spirits Raiding Party";
	const string Description = "When your Actions move Dahan and they bring your Incarna, you may deal 1 Damage at your Incarna (max. 1 Damage per Action).";
	static public SpecialRule Rule => new SpecialRule(Name, Description);

	#endregion Rule

	#region Setup Action

	static public async Task PlaceIncarna(Spirit spirit) {
		SpaceToken token = await spirit.SelectAlways("Select Presence to replace with Incarna + Dahan", spirit.Presence.Deployed);
		await token.Space.Dahan.AddDefault(1,AddReason.AsReplacement);
		await token.Space.ReplaceAsync(token.Token,1,spirit.Incarna);
	}

	#endregion Setup Action

	#region constructors

	public WarriorSpiritsRaidingParty(Spirit spirit)
		:base(spirit,"Ts",Img.Ts_Incarna, Img.Ts_Incarna) { }

	#endregion constructors

	public async Task HandleTokenRemovedAsync(ITokenRemovedArgs args) {

		if( args is ITokenMovedArgs moved ) { 
			if( moved.Removed.Class == _leaderClass )
				await TryToFollowLeader(moved);
		}
	}

	async Task TryToFollowLeader(ITokenMovedArgs args) {
		// can only follow between spaces, not presence track/destroyed
		if( args.From is not Space from || args.To is not Space to ) return;

		// If Regular presence is there now, or was there previously, let it do the move.
		if( NormalPresenceHandlesTheFollow(args, from) ) return; // don't follow again.

		// Check # of leaders & # of followers
		int followerCount = from[this]; // only include Incarna, if normal presence, it does the following.
		int maxThatCanMove = Math.Min(args.Count, followerCount);
		if( maxThatCanMove == 0 ) return;

		// Using 'Gather' here so user can click on existing Presence in Source
		// If we used 'Push', user would click on Destination instead of Source
		string prompt = "Move Incarna with " + args.Removed.Class.Label + "?";
		var st = this.On(from);
		while( 0 < maxThatCanMove-- ) {
			var source = await Self.Select(A.SpaceTokenDecision.ToCollect(prompt, [st], Present.Done, to.SpaceSpec));
			if( source is not null ) {
				await source.MoveTo(to);
				await Do1DamagePerAction(to);
			}
		}
	}

	bool NormalPresenceHandlesTheFollow(ITokenMovedArgs args, Space from)
		=> Self.Presence.Token is FollowingPresenceToken follower
			&& (from[follower] > 0 // still there, waiting to handle it
				|| follower.FollowedMoves.Contains(args) // already handled it
			);

	async Task Do1DamagePerAction(Space to) {
		var scope = ActionScope.Current;
		if( !scope.ContainsKey(Name) ) {
			await Self.Target(to).DamageInvaders(1);
			scope[Name] = true; // used
		}
	}

	readonly ITokenClass _leaderClass = Human.Dahan;

}
