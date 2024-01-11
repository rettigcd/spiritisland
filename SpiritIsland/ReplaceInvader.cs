namespace SpiritIsland;

static public class ReplaceInvader {

	#region Downgrade

	public static async Task Downgrade1( Spirit spirit, SpaceState tokens, Present present, params HumanTokenClass[] groups ) {
		HumanToken[] options = tokens.HumanOfAnyTag( groups );
		await Downgrade1Token( spirit, tokens, present, options );
	}

	public static async Task DowngradeAll( Spirit self, SpaceState tokens, params HumanTokenClass[] groups ) {

		// downgrade any # of invaders
		var invadersThatCanBeDowngraded = tokens.HumanOfAnyTag( groups )
			.ToDictionary( t => t, t => tokens[t] )
			.ToCountDict();

		HumanToken[] options = invadersThatCanBeDowngraded.Keys.ToArray();
		while(0 < options.Length) {
			var oldInvader = await Downgrade1Token( self, tokens, Present.Done, options );
			if(oldInvader == null) break;
			// this one downgraded, can't use again
			invadersThatCanBeDowngraded[oldInvader]--;
			// next
			options = invadersThatCanBeDowngraded.Keys.ToArray();
		}
	}

	/// <summary> Offers Specific tokens, instead of token classes. </summary>
	/// <returns> Original token (before downgrade).</returns>
	static async Task<HumanToken> Downgrade1Token( Spirit spirit, SpaceState tokens, Present present, HumanToken[] options ) {
		var st = await spirit.SelectAsync( An.Invader.ToReplace( "downgrade", options.On(tokens.Space), present ) );
		if(st == null) return null;
		HumanToken oldInvader = st.Token.AsHuman();

		await DowngradeSelectedInvader( tokens, oldInvader );
		return oldInvader;
	}

	public static async Task DowngradeSelectedInvader( SpaceState tokens, HumanToken oldInvader ) {

		// Explorers just get removed
		if(oldInvader.HumanClass == Human.Explorer) {
			tokens.Adjust( oldInvader, -1 );
			return;
		}

		var newToken = tokens.GetDefault( DowngradeType( oldInvader.HumanClass ) ).AsHuman()
			.AddStrife( oldInvader.StrifeCount )
			.AddDamage( oldInvader.Damage, oldInvader.DreamDamage );

		// if downgrading it, destroys it, then do nothing
		if(newToken.IsDestroyed && tokens.PreventsInvaderDamage() ) return;

		await tokens.RemoveAsync(oldInvader,1,RemoveReason.Replaced);
		await tokens.AddAsync(newToken,1,AddReason.AsReplacement);
	}

	static HumanTokenClass DowngradeType( HumanTokenClass orig ) => orig == Human.City ? Human.Town
		: orig == Human.Town ? Human.Explorer
		: throw new ArgumentOutOfRangeException( nameof( orig ), "Must be Town or City." );

	#endregion Downgrade

	#region Upgrade

	/// <param name="oldInvader">Explorer or Town</param>
	/// <exception cref="ArgumentOutOfRangeException"></exception>
	public static async Task UpgradeSelectedInvader( SpaceState tokens, HumanToken oldInvader ) {

		// Upgrade it
		var newToken = tokens.GetDefault( UpgradeType( oldInvader.HumanClass ) ).AsHuman()
			.AddStrife( oldInvader.StrifeCount )
			.AddDamage( oldInvader.Damage, oldInvader.DreamDamage );

		await tokens.RemoveAsync( oldInvader, 1, RemoveReason.Replaced );
		await tokens.AddAsync( newToken, 1, AddReason.AsReplacement );
	}

	/// <summary> Offers Specific tokens, instead of token classes. </summary>
	/// <returns> Original token (before upgrade).</returns>
	static public async Task<HumanToken> Upgrade1Token( Spirit spirit, SpaceState tokens, Present present, HumanToken[] options, string actionSuffix = "" ) {
		var st = await spirit.SelectAsync( An.Invader.ToReplace( "upgrade" + actionSuffix, options.On( tokens.Space ), present ) );
		if(st == null) return null;
		HumanToken oldInvader = st.Token.AsHuman();

		await UpgradeSelectedInvader( tokens, oldInvader );
		return oldInvader;
	}

	static HumanTokenClass UpgradeType( HumanTokenClass orig ) => orig == Human.Explorer ? Human.Town
			: orig == Human.Town ? Human.City
			: throw new ArgumentOutOfRangeException( nameof( orig ), "Must be Explorer or Town." );


	#endregion Upgrade

	#region Disolve

	/// <summary>
	/// Disolves Tows/Cities into explorers.
	/// </summary>
	public static async Task DisolveInvaderIntoExplorers( TargetSpaceCtx ctx, HumanTokenClass oldInvader, int replaceCount ) {

		var tokens = ctx.Tokens;
		var st = await ctx.Self.SelectAsync( An.Invader.ToReplace("disolve", tokens.HumanOfTag( oldInvader ).On(ctx.Space) ) );
		if(st == null) return;
		var tokenToRemove = st.Token.AsHuman();

		int explorersToAdd = replaceCount - tokenToRemove.Damage; // ignore nightmare damage because it can't really destory stuff

		await tokens.RemoveAsync( tokenToRemove, 1, RemoveReason.Replaced );
		await tokens.AddDefaultAsync( Human.Explorer, explorersToAdd, AddReason.AsReplacement );

		// ! This is an approximation, to perfectly distribute strife, need to be able to replace with multiple types.
		await ctx.AddStrife( tokenToRemove.StrifeCount, Human.Explorer );
	}

	#endregion Disolve

	#region > Dahan

	public static async Task WithDahanAsync( SpaceState tokens, HumanTokenClass[] invaderTypes ) {
		var tokenToRemove = tokens.BestInvaderToBeRidOf( invaderTypes );
		if(tokenToRemove is not null) {

			var result = await tokens.RemoveAsync(tokenToRemove,1, RemoveReason.Replaced);
			if(0<result.Count)
				await tokens.AddDefaultAsync( Human.Dahan, 1, AddReason.AsReplacement );

		}
	}

	#endregion > Dahan

}