namespace SpiritIsland;

static public class ReplaceInvader {

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

	/// <summary> Offers Specific tokens, instead of token classes. </summary>
	/// <returns> Original token (before upgrade).</returns>
	static public async Task<HumanToken> Upgrade1Token( Spirit spirit, SpaceState tokens, Present present, HumanToken[] options ) {
		var st = await spirit.SelectAsync( An.Invader.ToReplace( "upgrade", options.On( tokens.Space ), present ) );
		if(st == null) return null;
		HumanToken oldInvader = st.Token.AsHuman();

		UpgradeSelectedInvader( tokens, oldInvader );
		return oldInvader;
	}


	public static async Task DowngradeSelectedInvader( SpaceState tokens, HumanToken oldInvader ) {
		// remove old invader
		tokens.Adjust( oldInvader, -1 );

		// Add new
		var newInvaderClass = oldInvader.HumanClass == Human.City ? Human.Town
			: oldInvader.HumanClass == Human.Town ? Human.Explorer
			: null;
		if(newInvaderClass != null)
			await AddReplacementOrDestroy( tokens, oldInvader, newInvaderClass );
	}

	/// <param name="oldInvader">Explorer or Town</param>
	/// <exception cref="ArgumentOutOfRangeException"></exception>
	public static void UpgradeSelectedInvader( SpaceState tokens, HumanToken oldInvader ) {
		// remove old invader
		tokens.Adjust( oldInvader, -1 );

		// Add new
		var newInvaderClass = oldInvader.HumanClass == Human.Explorer ? Human.Town
			: oldInvader.HumanClass == Human.Town ? Human.City
			: throw new ArgumentOutOfRangeException($"{nameof(oldInvader)} must be Explorer or Town");

		// Upgrade it
		var newTokenWithoutDamage = tokens.GetDefault( newInvaderClass ).AsHuman()
			.AddStrife( oldInvader.StrifeCount );
		var newTokenWithDamage = newTokenWithoutDamage.AddDamage( oldInvader.Damage, oldInvader.DreamDamage );
		tokens.Adjust( newTokenWithDamage, 1 );
	}


	static async Task AddReplacementOrDestroy( SpaceState tokens, HumanToken oldInvader, HumanTokenClass newInvaderClass ) {
		var newTokenWithoutDamage = tokens.GetDefault( newInvaderClass ).AsHuman()
			.AddStrife( oldInvader.StrifeCount );
		var newTokenWithDamage = newTokenWithoutDamage.AddDamage( oldInvader.Damage, oldInvader.DreamDamage );

		if(!newTokenWithDamage.IsDestroyed)
			tokens.Adjust( newTokenWithDamage, 1 );
		else if(newInvaderClass != Human.Explorer) {
			// add the non-damaged token, and destroys it.
			tokens.Adjust( newTokenWithoutDamage, 1 );
			await tokens.Invaders.DestroyNTokens( newTokenWithoutDamage, 1 );
		}
	}

	/// <summary>
	/// Disolves Tows/Cities into explorers.
	/// </summary>
	public static async Task DisolveInvaderIntoExplorers( TargetSpaceCtx ctx, HumanTokenClass oldInvader, int replaceCount ) {

		var tokens = ctx.Tokens;
		var st = await ctx.Self.SelectAsync( An.Invader.ToReplace("disolve", tokens.HumanOfTag( oldInvader ).On(ctx.Space) ) );
		if(st == null) return;
		var tokenToRemove = st.Token.AsHuman();

		// remove
		tokens.Adjust( tokenToRemove, -1 );

		// add
		int explorersToAdd = replaceCount - tokenToRemove.Damage; // ignore nitemare damage because it can't really destory stuff
		if( 0 < explorersToAdd )
			tokens.AdjustDefault( Human.Explorer, explorersToAdd );

		// distribute pre-existing strife.
		await ctx.AddStrife( tokenToRemove.StrifeCount, Human.Explorer );
	}

}