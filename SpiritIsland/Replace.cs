using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	static public class Replace {

		public static Task Downgrade( TargetSpaceCtx ctx, params TokenGroup[] groups )
			=> Downgrade(ctx.Self,ctx.Invaders,groups);

		public static async Task Downgrade( Spirit spirit, InvaderGroup group, params TokenGroup[] groups ) {

			var options = group.Tokens.OfAnyType( groups );
			Token oldInvader = await spirit.Action.Decision( new Decision.InvaderToDowngrade( group.Space, options, Present.Always ) );
			if(oldInvader == null) return;

			if(oldInvader.Generic == Invader.City) {
				await DowngradeCity( group, oldInvader );
			} else if(oldInvader.Generic == Invader.Town) {
				await DowngradeTown( group, oldInvader );
			}
		}

		public static async Task InvaderWithExplorer( Spirit spirit, InvaderGroup grp, TokenGroup oldInvader, int replaceCount ) {

			var counts = grp.Tokens;
			var specific = await spirit.Action.Decision( new Decision.TokenOnSpace("Select "+oldInvader.Label+" to disolve", grp.Space, counts.OfType( oldInvader ), Present.Always));
			if(specific == null) return;

			counts.Adjust( specific, -1 );
			counts.Adjust( Invader.Explorer[1], replaceCount );

			// apply pre-existing damage
			int damage = System.Math.Min(replaceCount,specific.FullHealth-specific.Health);
			await grp.Destroy(damage,Invader.Explorer[1]);
		}

		static Task DowngradeTown( InvaderGroup inv, Token town ) {

			// remove town / dahan
			inv.Tokens.Adjust( town, -1 );

			// add explorer
			inv.Tokens.Adjust(Invader.Explorer.Default,1);

			// but if town/dahan only had 1 health, then that destroys resulting explorer
			return town.Health == 1 
				? inv.Destroy( 1, Invader.Explorer.Default ) 
				: Task.CompletedTask;
		}

		static async Task DowngradeCity( InvaderGroup inv, Token city ) {
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

		public static Task DahanWithExplorer( TargetSpaceCtx ctx ) {
			if(ctx.Tokens.Has( TokenType.Dahan[1] )) {
				ctx.Tokens.Adjust( TokenType.Dahan[1], -1 );
				ctx.Tokens.Adjust( Invader.Explorer.Default, 1 );
				return ctx.Invaders.Destroy( 1, Invader.Explorer.Default );
			}

			if(ctx.Tokens.Has( TokenType.Dahan[2] )) {
				ctx.Tokens.Adjust( TokenType.Dahan[1], -1 );
				ctx.Tokens.Adjust( Invader.Explorer.Default, 1 );
			}
			return Task.CompletedTask;
		}

	}

}
