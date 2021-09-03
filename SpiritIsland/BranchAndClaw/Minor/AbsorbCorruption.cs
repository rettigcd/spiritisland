using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class AbsorbCorruption {

		[MinorCard( "Absorb Corruption", 1, Speed.Slow, Element.Sun, Element.Earth, Element.Plant )]
		[FromPresence( 0 )]
		static public Task ActAsync(TargetSpaceCtx ctx ) {

			return ctx.SelectPowerOption(
				new PowerOption(
					"Gather 1 blight", 
					() => ctx.GatherUpToNTokens( 1, TokenType.Blight.Generic ) 
				),
				new PowerOption(
					"Pay 1 Energy to remove 1 blight", 
					()=>Pay1EnergyToRemoveBlight(ctx), 
					ctx.Tokens.Has(TokenType.Blight) && ctx.Self.Energy>=1
				)
			);

		}

		static void Pay1EnergyToRemoveBlight( TargetSpaceCtx ctx ) {
			ctx.Tokens[TokenType.Blight]--;
			ctx.Self.Energy--;
		}
	}
}
