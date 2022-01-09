using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	public class OverenthusiasticArson {

		public const string Name = "Overenthusiastic Arson";

		[SpiritCard(OverenthusiasticArson.Name,1,Element.Fire,Element.Air), Fast, FromPresence(1)]
		static public async Task ActAsymc(TargetSpaceCtx ctx ) { 

			// Destroy 1 town
			await ctx.Invaders.Destroy(1,Invader.Town);

			// discard the top card of the minor power deck.
			var card = ctx.GameState.MinorCards.FlipNext();

			// Show the card to the user
			_ = await ctx.Self.SelectPowerCard(OverenthusiasticArson.Name + " turned up:",new PowerCard[] { card }, CardUse.Other, Present.Always );

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
