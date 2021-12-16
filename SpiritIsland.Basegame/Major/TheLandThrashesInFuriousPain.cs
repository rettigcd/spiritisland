using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class TheLandThrashesInFuriousPain {

		[MajorCard("The Land Thrashes in Furious Pain",4, Element.Moon, Element.Fire,Element.Earth)]
		[Slow]
		[FromPresence(2,Target.Blight)]
		static public async Task ActAsync(TargetSpaceCtx ctx) {

			static Task DamageLandFromBlight( TargetSpaceCtx ctx ) {
				// 2 damage per blight in target land
				int damage = ctx.BlightOnSpace * 2
					// +1 damage per blight in adjacent lands
					+ ctx.Adjacent.Sum( x => ctx.Target(x).BlightOnSpace );
				return ctx.DamageInvaders( damage );
			}

			await DamageLandFromBlight( ctx );

			// if you have 3 moon 3 earth
			if(await ctx.YouHave("3 moon,3 earth")) {
				// repeat on an adjacent land.
				var alsoTarget = await ctx.Decision( new Select.Space( "Select additional land to receive blight damage", ctx.Space.Adjacent, Present.Always));
				await DamageLandFromBlight( ctx.Target( alsoTarget ) );
			}
		}

	}
}
