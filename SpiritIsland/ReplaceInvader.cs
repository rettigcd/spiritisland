namespace SpiritIsland;

static public class ReplaceInvader {

	public static async Task Downgrade( TargetSpaceCtx ctx, Present present, params HealthTokenClass[] groups ) {

		var options = ctx.Tokens.OfAnyType( groups );
		HealthToken oldInvader = (HealthToken) await ctx.Self.Gateway.Decision( Select.Invader.ToReplace( "downgrade", ctx.Space, options, present ) );
		if(oldInvader == null) return;

		// remove old invader
		ctx.Tokens.Adjust( oldInvader, -1 );

		// Add new
		var newInvaderClass = oldInvader.Class == Invader.City ? Invader.Town : Invader.Explorer;

		var newTokenWithoutDamage = ctx.Tokens.GetDefault( newInvaderClass ).AddStrife(oldInvader.StrifeCount);
		var newTokenWithDamage = newTokenWithoutDamage.AddDamage( oldInvader.Damage, oldInvader.DreamDamage );

		if(!newTokenWithDamage.IsDestroyed )
			ctx.Tokens.Adjust( newTokenWithDamage, 1 );
		else if( newInvaderClass != Invader.Explorer ) {
			// add the non-damaged token, and destory it.
			ctx.Tokens.Adjust( newTokenWithoutDamage, 1 );
			await ctx.Invaders.Destroy( 1, newTokenWithoutDamage );
		}

	}

	public static async Task SingleInvaderWithExplorers( TargetSpaceCtx ctx, TokenClass oldInvader, int replaceCount ) {

		var tokens = ctx.Tokens;
		var tokenToRemove = (HealthToken) await ctx.Self.Gateway.Decision( Select.Invader.ToReplace("disolve", ctx.Space, tokens.OfType( oldInvader ) ) );
		if(tokenToRemove == null) return;

		// remove
		tokens.Adjust( tokenToRemove, -1 );

		// add
		int explorersToAdd = replaceCount - tokenToRemove.Damage; // ignore nitemare damage because it can't really destory stuff
		if( 0 < explorersToAdd )
			tokens.AdjustDefault( Invader.Explorer, explorersToAdd );

		// distribute pre-existing strife.
		for(int i=0;i< tokenToRemove.StrifeCount; ++i)
			await ctx.AddStrife( Invader.Explorer );
	}

}