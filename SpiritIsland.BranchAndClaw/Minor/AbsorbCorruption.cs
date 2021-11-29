using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class AbsorbCorruption {

		[MinorCard( "Absorb Corruption", 1, Element.Sun, Element.Earth, Element.Plant )]
		[Slow]
		[FromPresence( 0 )]
		static public async Task ActAsync(TargetSpaceCtx ctx ) {

			var gatherBlight = new ActionOption( "Gather 1 blight", () => ctx.Gather( 1, TokenType.Blight.Generic ) );
			var removeBlight = new ActionOption( "Pay 1 Energy to remove 1 blight",  ()=>Pay1EnergyToRemoveBlight(ctx),  ctx.Blight>0 && 1 <= ctx.Self.Energy );
			var doBoth = new ActionOption( "Do Both", async () => { await gatherBlight.Action(); await removeBlight.Action(); }, await ctx.YouHave("2 plant") );

			await ctx.SelectActionOption( gatherBlight, removeBlight, doBoth );

		}

		static void Pay1EnergyToRemoveBlight( TargetSpaceCtx ctx ) {
			ctx.RemoveBlight(); // !!! put in safe guard that we don't just do this: ctx.Blight.Count--   because that doesn't put it back on the card.
			ctx.Self.Energy--;
		}
	}
}
