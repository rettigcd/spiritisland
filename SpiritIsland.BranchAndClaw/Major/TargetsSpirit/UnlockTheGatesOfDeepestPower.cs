using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class UnlockTheGatesOfDeepestPower {

		[MajorCard( "Unlock the Gates of Deepest Power", 4, Element.Sun,Element.Moon,Element.Fire,Element.Air,Element.Water,Element.Earth,Element.Plant,Element.Animal )]
		[Fast]
		[AnySpirit]
		static public async Task ActAsync( TargetSpiritCtx ctx ) {

			// target Spirit gains a major power by drawing 2 and keeping 1, without having to forget another power card
			PowerCard card = await ctx.OtherCtx.DrawMajor( 2, false );
			ctx.Other.AddCardToHand( card );

			// if 2 of each element,
			if(await ctx.YouHave("2 sun,2 moon,2 fire,2 air,2 water,2 earth,2 plant,2 animal" ))
				await PlayCardByPayingHalfCostOrForgetting( card, ctx.OtherCtx );

		}

		private static async Task PlayCardByPayingHalfCostOrForgetting( PowerCard card, SpiritGameStateCtx ctx ) {

			// target spirit may now play the major power they keep by:
			//    * paying half its cost (round up) OR
			int cost = (card.Cost + card.Cost % 2) / 2;
			var payingHalfCostOption = new ActionOption(
				$"paying {cost}",
				() => {
					ctx.Self.PlayCard(card, cost);
				},
				cost <= ctx.Self.Energy
			);

			//    * forgetting it at the end of turn.
			var forgettingCardOption = new ActionOption(
				$"forgetting at end of turn",
				() => {
					ctx.Self.PlayCard(card, 0); 
					ctx.GameState.TimePasses_ThisRound.Push( new ForgetCard( ctx.Self, card ).Forget );
				}
			);

			// It gains all elmemental thresholds.

			await ctx.SelectOptionalAction( $"Play {card.Name} now by:",
				payingHalfCostOption,
				forgettingCardOption
			);
		}
	}

}
