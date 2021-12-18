using System.Threading.Tasks;

namespace SpiritIsland {

	static public class ReplaceInvader {

		public static Task Downgrade( TargetSpaceCtx ctx, params TokenCategory[] groups )
			=> Downgrade(ctx.Self,ctx.Invaders,groups);

		public static async Task Downgrade( Spirit spirit, InvaderBinding group, params TokenCategory[] groups ) {

			var options = group.Tokens.OfAnyType( groups );
			Token oldInvader = await spirit.Action.Decision( Select.Invader.ToDowngrade( "down-grade (C=>T or T=>E)", group.Space, options ) );
			if(oldInvader == null) return;

			if(oldInvader.Category == Invader.City) {
				await DowngradeCity( group, oldInvader );
			} else if(oldInvader.Category == Invader.Town) {
				await DowngradeTown( group, oldInvader );
			}
		}

		public static async Task SingleInvaderWithExplorers( Spirit spirit, InvaderBinding grp, TokenCategory oldInvader, int replaceCount ) {

			var tokens = grp.Tokens;
			var specific = await spirit.Action.Decision( Select.Invader.ToDowngrade("disolve", grp.Space, tokens.OfType( oldInvader )) );
			if(specific == null) return;

			tokens.Adjust( specific, -1 );
			tokens.Adjust( Invader.Explorer[1], replaceCount );

			// apply pre-existing damage
			int damage = System.Math.Min(replaceCount,specific.FullHealth-specific.Health);
			await grp.Destroy(damage,Invader.Explorer[1]);
		}

		static Task DowngradeTown( InvaderBinding inv, Token town ) {

			// remove town / dahan
			inv.Tokens.Adjust( town, -1 );

			// add explorer
			inv.Tokens.Adjust(Invader.Explorer.Default,1);

			// but if town/dahan only had 1 health, then that destroys resulting explorer
			return town.Health == 1 
				? inv.Destroy( 1, Invader.Explorer.Default ) 
				: Task.CompletedTask;
		}

		static async Task DowngradeCity( InvaderBinding inv, Token city ) {
			// remove city
			inv.Tokens.Adjust( city, -1 );

			// add town
			Token town = Invader.Town[city.Health - 1]; // maintain pre-existing damage
			if( town.Health == 0) {
				inv.Tokens.Adjust( Invader.Town.Default, 1 ); // add a town so we can destroy it
				await inv.Destroy( 1, Invader.Town.Default ); // resulting Town is destroyed
			} else 
				inv.Tokens.Adjust( town, 1 );
		}

	}

}
