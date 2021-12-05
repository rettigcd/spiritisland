using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {
	public class ShapeTheSelfAnew {

		public const string Name = "Shape the Self Anew";

		[SpiritCard( ShapeTheSelfAnew.Name, 0, Element.Moon), Slow, Yourself]
		public static async Task ActAsync(TargetSpiritCtx ctx ) {
			// Gain a Minor Power.
			// If you have 4 moon, You may gain a Major Power instead of a Minor Power.
			if(await ctx.YouHave("4 moon"))
				await ctx.DrawMajor(true);
			else
				await ctx.DrawMinor();

			// You may Forget this Power Card to gain 3 Energy.
			if(await ctx.Self.UserSelectsFirstText( "Forget this card for +3 energy.", "Yes, forget it.", "no thanks." )) {
				// Forget this Power Card
				var thisCard = ctx.Self.InPlay.Single( x => x.Name == ShapeTheSelfAnew.Name );
				ctx.Self.Forget( thisCard );

				// gain 3 energy
				ctx.Self.Energy += 3;
			}

		}

	}

}
