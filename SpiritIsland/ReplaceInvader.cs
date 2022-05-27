namespace SpiritIsland;

static public class ReplaceInvader {

	public static async Task Downgrade( TargetSpaceCtx ctx, Present present, params HealthTokenClass[] groups ) {

		var options = ctx.Tokens.OfAnyType( groups );
		HealthToken oldInvader = (HealthToken) await ctx.Self.Action.Decision( Select.Invader.ToReplace( "downgrade", ctx.Space, options, present ) );
		if(oldInvader == null) return;

		// remove old invader
		ctx.Tokens.Adjust( oldInvader, -1 );

		// Add new
		var newInvaderClass = oldInvader.Class == Invader.City ? Invader.Town : Invader.Explorer;

		var newTokenWithoutDamage = ctx.Tokens.GetDefault( newInvaderClass ).AddStrife(oldInvader.StrifeCount);
		var newTokenWithDamage = newTokenWithoutDamage.AddDamage( oldInvader.Damage );

		if(!newTokenWithDamage.IsDestroyed )
			ctx.Tokens.Adjust( newTokenWithDamage, 1 );
		else if( newInvaderClass != Invader.Explorer ) {
			// add the non-damaged token, and destory it.
			ctx.Tokens.Adjust( newTokenWithoutDamage, 1 );
			await ctx.Invaders.Destroy( 1, newTokenWithoutDamage );
		}

	}

	public static async Task SingleInvaderWithExplorers( Spirit spirit, InvaderBinding grp, TokenClass oldInvader, int replaceCount ) {

		var tokens = grp.Tokens;
		var specific = (HealthToken) await spirit.Action.Decision( Select.Invader.ToReplace("disolve", grp.Space, tokens.OfType( oldInvader ) ) );
		if(specific == null) return;

		tokens.Adjust( specific, -1 );
		tokens.AdjustDefault( Invader.Explorer, replaceCount );

		// apply pre-existing damage
		int damage = System.Math.Min(replaceCount,specific.FullHealth-specific.RemainingHealth); // !!! cleanup damage calculations
		await grp.Destroy(damage,Invader.Explorer);

		// !!! ??? pre-existing strife?
	}

}