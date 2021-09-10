﻿using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class AbsorbCorruption {

		[MinorCard( "Absorb Corruption", 1, Speed.Slow, Element.Sun, Element.Earth, Element.Plant )]
		[FromPresence( 0 )]
		static public Task ActAsync(TargetSpaceCtx ctx ) {

			return ctx.SelectActionOption(
				new ActionOption(
					"Gather 1 blight", 
					() => ctx.Gather( 1, TokenType.Blight.Generic ) 
				),
				new ActionOption(
					"Pay 1 Energy to remove 1 blight", 
					()=>Pay1EnergyToRemoveBlight(ctx), 
					ctx.Tokens.Blight>0 && 1 <= ctx.Self.Energy
				)
			);

		}

		static void Pay1EnergyToRemoveBlight( TargetSpaceCtx ctx ) {
			ctx.Tokens.Blight.Count--;
			ctx.Self.Energy--;
		}
	}
}
