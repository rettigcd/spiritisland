using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	public class OverenthusiasticArson {

		[SpiritCard("Overenthusiastic Arson",1,Element.Fire,Element.Air), Fast, FromPresence(1)]
		static public async Task ActAsymc(TargetSpaceCtx ctx ) { 

			// Destory 1 town
			await ctx.Invaders.Destroy(1,Invader.Town);

			// discard the top card of the minor power deck.
			var card = ctx.GameState.MinorCards.FlipNext();

			// IF it provides fire:
			if(card.Elements.Contains( Element.Fire )) {
				// 1 fear
				ctx.AddFear(1);

				// 2 damage,
				await ctx.DamageInvaders(2);

				// add 1 blight
				await ctx.AddBlight(); // !!! this should cascade
			}
		}
	}

}
