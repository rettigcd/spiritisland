namespace SpiritIsland.BranchAndClaw;

public class PreyOnTheBuilders {

	const string Name = "Prey on the Builders";

	[SpiritCard( Name, 1, Element.Moon, Element.Fire, Element.Animal )]
	[Fast]
	[FromPresence(0)]
	public static async Task ActAsync(TargetSpaceCtx ctx ) {
		// you may gather 1 beast
		await ctx.GatherUpTo(1, TokenType.Beast);

		if( ctx.Beasts.Any )
			ctx.Skip1Build( Name );

	}

}