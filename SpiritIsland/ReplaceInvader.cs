namespace SpiritIsland;

static public class ReplaceInvader {

	/// <summary>User selects 1 Invader and downgrades it.</summary>
	/// <returns>If invader was downgraded</returns>
	public static Task Downgrade1( TargetSpaceCtx ctx, Present present, params HumanTokenClass[] groups )
		=> Downgrade1(ctx.Self,ctx.Tokens,present,groups);

	public static async Task Downgrade1( Spirit spirit, SpaceState tokens, Present present, params HumanTokenClass[] groups ) {
		HumanToken[] options = tokens.OfAnyHumanClass( groups );
		await Downgrade1Token( spirit, tokens, present, options );
	}

	public static async Task DowngradeAll( TargetSpaceCtx ctx, params HumanTokenClass[] groups ) {

		// downgrade any # of invaders
		var invadersThatCanBeDowngraded = ctx.Tokens.OfAnyHumanClass( groups )
			.ToDictionary( t => t, t => ctx.Tokens[t] )
			.ToCountDict();

		HumanToken[] options = invadersThatCanBeDowngraded.Keys.ToArray();
		while(0 < options.Length) {
			var oldInvader = await Downgrade1Token( ctx.Self, ctx.Tokens, Present.Done, options );
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
		var st = await spirit.Gateway.Select( An.Invader.ToReplace( "downgrade", tokens.Space, options, present ) );
		if(st == null) return null;
		HumanToken oldInvader = st.Token.AsHuman();

		await DowngradeSelectedInvader( tokens, oldInvader );
		return oldInvader;
	}

	public static async Task DowngradeSelectedInvader( SpaceState tokens, HumanToken oldInvader ) {
		// remove old invader
		tokens.Adjust( oldInvader, -1 );

		// Add new
		var newInvaderClass = oldInvader.Class == Human.City ? Human.Town
			: oldInvader.Class == Human.Town ? Human.Explorer
			: null;
		if(newInvaderClass != null)
			await AddReplacementOrDestroy( tokens, oldInvader, newInvaderClass );
	}

	static async Task AddReplacementOrDestroy( SpaceState tokens, HumanToken oldInvader, HumanTokenClass newInvaderClass ) {
		var newTokenWithoutDamage = tokens.GetDefault( newInvaderClass ).AsHuman().AddStrife( oldInvader.StrifeCount );
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
		var st = await ctx.Self.Gateway.Select( An.Invader.ToReplace("disolve", ctx.Space, tokens.OfHumanClass( oldInvader ) ) );
		if(st == null) return;
		var tokenToRemove = st.Token.AsHuman();

		// remove
		tokens.Adjust( tokenToRemove, -1 );

		// add
		int explorersToAdd = replaceCount - tokenToRemove.Damage; // ignore nitemare damage because it can't really destory stuff
		if( 0 < explorersToAdd )
			tokens.AdjustDefault( Human.Explorer, explorersToAdd );

		// distribute pre-existing strife.
		for(int i=0;i< tokenToRemove.StrifeCount; ++i)
			await ctx.AddStrife( Human.Explorer );
	}

}