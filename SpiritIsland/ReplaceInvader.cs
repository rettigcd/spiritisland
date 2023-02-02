namespace SpiritIsland;

static public class ReplaceInvader {

	public static async Task Downgrade( TargetSpaceCtx ctx, Present present, params HumanTokenClass[] groups ) {

		var options = ctx.Tokens.OfAnyHumanClass( groups );
		var st = await ctx.Self.Gateway.Decision( Select.Invader.ToReplace( "downgrade", ctx.Space, options, present ) );
		if(st == null) return;
		HumanToken oldInvader = (HumanToken)st.Token;

		// remove old invader
		ctx.Tokens.Adjust( oldInvader, -1 );

		// Add new
		var newInvaderClass = oldInvader.Class == Human.City ? Human.Town : Human.Explorer;

		var newTokenWithoutDamage = ctx.Tokens.GetDefault( newInvaderClass ).AddStrife(oldInvader.StrifeCount);
		var newTokenWithDamage = newTokenWithoutDamage.AddDamage( oldInvader.Damage, oldInvader.DreamDamage );

		if(!newTokenWithDamage.IsDestroyed )
			ctx.Tokens.Adjust( newTokenWithDamage, 1 );
		else if( newInvaderClass != Human.Explorer ) {
			// add the non-damaged token, and destory it.
			ctx.Tokens.Adjust( newTokenWithoutDamage, 1 );
			await ctx.Invaders.DestroyNTokens( newTokenWithoutDamage, 1 );
		}

	}

	public static async Task SingleInvaderWithExplorers( TargetSpaceCtx ctx, HumanTokenClass oldInvader, int replaceCount ) {

		var tokens = ctx.Tokens;
		var st = await ctx.Self.Gateway.Decision( Select.Invader.ToReplace("disolve", ctx.Space, tokens.OfHumanClass( oldInvader ) ) );
		if(st == null) return;
		var tokenToRemove = (HumanToken)st.Token;

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