using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class AbsorbCorruption {

		[MinorCard( "Absorb Corruption", 1, Element.Sun, Element.Earth, Element.Plant )]
		[Slow]
		[FromPresence( 0 )]
		static public async Task ActAsync(TargetSpaceCtx ctx ) {

			var gatherOpt = new ActionOption( "Gather 1 blight", () => ctx.Gather( 1, TokenType.Blight.Generic ) );
			var removeOpt = new ActionOption( "Pay 1 Energy to remove 1 blight",  ()=>Pay1EnergyToRemoveBlight(ctx),  ctx.Blight>0 && 1 <= ctx.Self.Energy );
			var doBoth = new ActionOption( "Do Both", async () => { await gatherOpt.Action(); await removeOpt.Action(); }, await ctx.YouHave("2 plant") );

			await ctx.SelectActionOption( gatherOpt, removeOpt, doBoth );

		}

		static void Pay1EnergyToRemoveBlight( TargetSpaceCtx ctx ) {
			ctx.Blight.Count--;
			ctx.Self.Energy--;
		}
	}
}
