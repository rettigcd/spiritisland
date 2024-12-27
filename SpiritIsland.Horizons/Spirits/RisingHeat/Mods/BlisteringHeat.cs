using SpiritIsland;

class BlisteringHeat(Spirit spirit) : SpiritPresenceToken(spirit)
	, IModifyAddingToken, IModifyRemovingToken // Invaders
	, IHandleTokenAdded, IHandleTokenRemoved   // Presence
{
	public const string Name = "Blistering Hear";
	const string Description = "At your Sacred Site, Invades have -1 Health (min 1).";
	static public SpecialRule Rule => new SpecialRule(Name, Description);

	#region Adding/Removing Invaders
	async Task IModifyAddingToken.ModifyAddingAsync(AddingTokenArgs args) {
		if( Self.Presence.IsSacredSite(args.To) 
			&& args.Token is HumanToken ht 
			&& ht.HasTag(TokenCategory.Invader)
			&& 1 < ht.FullHealth 
		) {
			var downgraded = ht.AddHealth(-1);
			if( downgraded.IsDestroyed ) {
				await args.To.Destroy(ht,args.Count);
				args.Count = 0;
			} else {
				_downgradedTokens.Add(downgraded);
				args.Token = downgraded;
			}
		}
	}

	Task IModifyRemovingToken.ModifyRemovingAsync(RemovingTokenArgs args) {
		if( Self.Presence.IsSacredSite(args.From)
			&& args.Token is HumanToken ht
			&& _downgradedTokens.Contains(ht)
		) {
			var upgraded = ht.AddHealth(+1);
			// swap out
			args.From.Adjust(ht,-args.Count); 
			args.From.Adjust(upgraded,args.Count);

			args.Token = upgraded;
		}
		return Task.CompletedTask;
	}

	#endregion Adding/Removing Invaders

	#region Creating / Removing Sacred Site

	Task IHandleTokenAdded.HandleTokenAddedAsync(Space to, ITokenAddedArgs args) {
		if( args.To is Space space && args.To == this ) {
			int pc = space[this];
			bool becameSacredSite = pc - args.Count < 2 && 2 <= pc;
			if( becameSacredSite )
				return DowngradeAll(space);
		}
		return Task.CompletedTask;
	}

	public override async Task HandleTokenRemovedAsync(ITokenRemovedArgs args) {
		await base.HandleTokenRemovedAsync(args);

		// if we lose sacred site, upgrade invaders
		if( args.From is not Space space || args.From != this ) return;
		int pc = space[this];
		bool lostSacredSite = pc < 2 && 2 <= pc + args.Count;
		if( lostSacredSite )
			foreach( var orig in space.HumanOfAnyTag(TokenCategory.Invader).ToArray() )
				UpgradeAllOf1Type(space, orig);
	}


	async Task DowngradeAll(Space space) {
		foreach( var orig in space.HumanOfAnyTag(TokenCategory.Invader).ToArray() )
			await DowngradeAllOf1Type(space, orig);
	}

	void UpgradeAllOf1Type(Space space, HumanToken orig) {
		var upgraded = orig.AddHealth(1);
		space.Adjust(upgraded, space[orig]);
		space.Init(orig,0);
	}

	async Task DowngradeAllOf1Type(Space space, HumanToken orig) {
		var downgraded = orig.AddHealth(-1);
		if( downgraded.IsDestroyed ) {
			await space.Destroy(orig, space[orig]);
		} else {
			_downgradedTokens.Add(downgraded);
			space.Adjust(downgraded, space[orig]);
			space.Init(orig, 0);
		}
	}

	#endregion

	HashSet<HumanToken> _downgradedTokens = [];
}