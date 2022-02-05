﻿namespace SpiritIsland.BranchAndClaw;

public class RazorSharpUndergrowth {

	[MinorCard( "Razor-Sharp Undergrowth", 1, Element.Moon, Element.Plant )]
	[Fast]
	[FromPresence( 0, Target.NoBlight )]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {

		// destroy 1 explorer
		await ctx.Invaders.Destroy(1,Invader.Explorer);
		// and 1 dahan
		await ctx.DestroyDahan( 1 );
		// add 1 wilds
		await ctx.Wilds.Add(1);
		// defend 2
		ctx.Defend(2);

	}

}