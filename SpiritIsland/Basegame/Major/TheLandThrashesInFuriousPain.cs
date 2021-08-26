using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class TheLandThrashesInFuriousPain {

		[MajorCard("The Land Thrashes in Furious Pain",4, Speed.Slow, Element.Moon, Element.Fire,Element.Earth)]
		[FromPresence(2,Target.Blight)]
		static public async Task ActAsync(TargetSpaceCtx ctx) {

			await ApplyDamageFromBlight( ctx.Target, ctx );

			// if you have 3 moon 3 earth
			if(ctx.Self.Elements.Contains("3 moon,3 earth")) {
				// repeat on an adjacent land.
				var alsoTarget = await ctx.Self.Action.Choose( new TargetSpaceDecision( "Select adjacent land to receive damage from blight", ctx.Target.Adjacent));
				await ApplyDamageFromBlight( alsoTarget, ctx );
			}
		}

		static Task ApplyDamageFromBlight( Space space, TargetSpaceCtx ctx ) {
			GameState gs = ctx.GameState;
			int damage = gs.GetBlightOnSpace( space ) * 2  // 2 damage per blight in target land
				+ space.Adjacent.Sum( x => gs.GetBlightOnSpace( x ) ); // +1 damage per blight in adjacent lands
			return ctx.DamageInvaders( space, damage );
		}
	}
}
