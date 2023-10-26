namespace SpiritIsland;

static public class ReplaceInvader {

	/// <summary>User selects 1 Invader and downgrades it.</summary>
	/// <returns>If invader was downgraded</returns>
	public static async Task Downgrade1( TargetSpaceCtx ctx, Present present, params HumanTokenClass[] groups ) {
		HumanToken[] options = ctx.Tokens.OfAnyHumanClass( groups );
		await Downgrade1Token( ctx, present, options );
	}

	public static async Task DowngradeAll( TargetSpaceCtx ctx, params HumanTokenClass[] groups ) {

		// downgrade any # of invaders
		var invadersThatCanBeDowngraded = ctx.Tokens.OfAnyHumanClass(Human.Invader)
			.ToDictionary( t => t, t => ctx.Tokens[t] )
			.ToCountDict();

		HumanToken[] options = invadersThatCanBeDowngraded.Keys.ToArray();
		while(0 < options.Length) {
			var oldInvader = await Downgrade1Token( ctx, Present.Done, options );
			if(oldInvader == null) break;
			// this one downgraded, can't use again
			invadersThatCanBeDowngraded[oldInvader]--;
			// next
			options = invadersThatCanBeDowngraded.Keys.ToArray();
		}
	}

	/// <summary> Offers Specific tokens, instead of token classes. </summary>
	/// <returns> Original token (before downgrade).</returns>
	static async Task<HumanToken> Downgrade1Token( TargetSpaceCtx ctx, Present present, HumanToken[] options ) {
		var st = await ctx.Self.Gateway.Decision( Select.Invader.ToReplace( "downgrade", ctx.Space, options, present ) );
		if(st == null) return null;
		HumanToken oldInvader = st.Token.AsHuman();

		await DowngradeSelectedInvader( ctx, oldInvader );
		return oldInvader;
	}

	public static async Task DowngradeSelectedInvader( TargetSpaceCtx ctx, HumanToken oldInvader ) {
		// remove old invader
		ctx.Tokens.Adjust( oldInvader, -1 );

		// Add new
		var newInvaderClass = oldInvader.Class == Human.City ? Human.Town
			: oldInvader.Class == Human.Town ? Human.Explorer
			: null;
		if(newInvaderClass != null)
			await AddReplacementOrDestroy( ctx.Tokens, oldInvader, newInvaderClass );
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
		var st = await ctx.Self.Gateway.Decision( Select.Invader.ToReplace("disolve", ctx.Space, tokens.OfHumanClass( oldInvader ) ) );
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