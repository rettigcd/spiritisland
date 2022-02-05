﻿namespace SpiritIsland.BranchAndClaw;

public class BoonOfGrowingPower {

	// target spirit
	[SpiritCard("Boon of Growing Power",1,Element.Sun,Element.Moon,Element.Plant)]
	[Slow]
	[AnySpirit]
	static public async Task ActAsync( TargetSpiritCtx ctx ) {

		// target spirit gains a power card
		await ctx.Other.Draw(ctx.GameState);

		// if you target another spirit, they also gain 1 energy
		if(ctx.Other != ctx.Self)
			++ctx.Other.Energy;

	}

}