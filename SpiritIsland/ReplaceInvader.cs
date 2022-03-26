namespace SpiritIsland;

static public class ReplaceInvader {

	public static Task Downgrade( TargetSpaceCtx ctx, params TokenClass[] groups )
		=> Downgrade(ctx.Self,ctx.Invaders,groups);

	public static async Task Downgrade( Spirit spirit, InvaderBinding group, params TokenClass[] groups ) {

		var options = group.Tokens.OfAnyType( groups );
		Token oldInvader = await spirit.Action.Decision( Select.Invader.ToDowngrade( "down-grade (C=>T or T=>E)", group.Space, options ) );
		if(oldInvader == null) return;

		if(oldInvader.Class == Invader.City)
			await DowngradeCity( group, oldInvader );
		else if(oldInvader.Class == Invader.Town)
			await DowngradeTown( group, oldInvader );
	}

	public static async Task SingleInvaderWithExplorers( Spirit spirit, InvaderBinding grp, TokenClass oldInvader, int replaceCount ) {

		var tokens = grp.Tokens;
		var specific = await spirit.Action.Decision( Select.Invader.ToDowngrade("disolve", grp.Space, tokens.OfType( oldInvader )) );
		if(specific == null) return;

		tokens.Adjust( specific, -1 );
		tokens.Adjust( Invader.Explorer[1], replaceCount );

		// apply pre-existing damage
		int damage = System.Math.Min(replaceCount,specific.FullHealth-specific.Health);
		await grp.Destroy(damage,Invader.Explorer[1]);

		// ??? pre-existing strife?
	}

	static async Task DowngradeTown( InvaderBinding inv, Token town ) {

		// remove town
		inv.Tokens.Adjust( town, -1 );

		// add explorer
		var newExplorer = Invader.Explorer.Default.WithStrife( town.Strife() );
		inv.Tokens.Adjust( newExplorer, 1 );

		// but if town/dahan only had 1 health, then that destroys resulting explorer
		if( town.Health == 1 )
			await inv.Destroy( 1, Invader.Explorer.Default );
	}

	static async Task DowngradeCity( InvaderBinding inv, Token city ) {
		// remove city
		inv.Tokens.Adjust( city, -1 );

		// add town
		Token town = Invader.Town[city.Health - 1].WithStrife( city.Strife() ); // maintain pre-existing damage

		if( town.Health == 0) {
			inv.Tokens.Adjust( Invader.Town.Default, 1 ); // add a town so we can destroy it
			await inv.Destroy( 1, Invader.Town.Default ); // resulting Town is destroyed
			return;
		}

		inv.Tokens.Adjust( town, 1 );
	}

}