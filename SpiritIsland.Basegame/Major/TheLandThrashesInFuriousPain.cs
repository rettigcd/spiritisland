using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class TheLandThrashesInFuriousPain {

		[MajorCard("The Land Thrashes in Furious Pain",4, Speed.Slow, Element.Moon, Element.Fire,Element.Earth)]
		[FromPresence(2,Target.Blight)]
		static public async Task ActAsync(TargetSpaceCtx ctx) {

			static Task DamageLandFromBlight( TargetSpaceCtx ctx ) {
				// 2 damage per blight in target land
				int damage = ctx.BlightOnSpace * 2
					// +1 damage per blight in adjacent lands
					+ ctx.Adjacents.Sum( x => ctx.GameState.GetBlightOnSpace( x ) );
				return ctx.DamageInvaders( ctx.Space, damage );
			}

			await DamageLandFromBlight( ctx );

			// if you have 3 moon 3 earth
			if(ctx.YouHave("3 moon,3 earth")) {
				// repeat on an adjacent land.
				var alsoTarget = await ctx.Self.Action.Decision( new Decision.TargetSpace( "Select additional land to receive blight damage", ctx.Space.Adjacent));
				await DamageLandFromBlight( ctx.TargetSpace( alsoTarget ) );
			}
		}

	}
}
