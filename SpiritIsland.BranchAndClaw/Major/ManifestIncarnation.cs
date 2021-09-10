using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class ManifestIncarnation {

		static ManifestIncarnation() {
			SpaceFilter.lookup["Cities"] = (ctx, space) => ctx.GameState.Tokens[space].Has(Invader.City);
		}

		[MajorCard( "Manifest Incarnation", 6, Speed.Slow, Element.Sun, Element.Moon, Element.Earth, Element.Animal )]
		[FromPresence( 0, "Cities" )]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {
			// 6 fear
			ctx.AddFear(6);

			// +1 fear for each town/city and for each of your presence in target land.
			int fearCount = ctx.Tokens.SumAny(Invader.City,Invader.Town)
				+ ctx.Self.Presence.Placed.Count(x=>x==ctx.Space);
			ctx.AddFear(fearCount);

			// Remove 1 city, 1 town and 1 explorer.
			var tokenToRemove = ctx.Tokens.PickBestInvaderToRemove(Invader.City);    	ctx.Tokens.Adjust( tokenToRemove, -1 );
			tokenToRemove     = ctx.Tokens.PickBestInvaderToRemove( Invader.Town );     ctx.Tokens.Adjust( tokenToRemove, -1 );
			tokenToRemove     = ctx.Tokens.PickBestInvaderToRemove( Invader.Explorer ); ctx.Tokens.Adjust( tokenToRemove, -1 );

			// if you have 3 sun and 3 moon, invaders do -6 damage on their ravage.
			if(ctx.YouHave( "3 sun,3 moon" ))
				ctx.Defend( 6 ); // !! not exactly correct but close

			// Then, Invaders in target land ravage.
			await ctx.GameState.RavageSpace( ctx.Invaders );
		}

	}

}
