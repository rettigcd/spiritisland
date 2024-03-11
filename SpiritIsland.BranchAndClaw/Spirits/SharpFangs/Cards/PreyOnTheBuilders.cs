namespace SpiritIsland.BranchAndClaw;

public class PreyOnTheBuilders {

	const string Name = "Prey on the Builders";

	[SpiritCard( Name, 1, Element.Moon, Element.Fire, Element.Animal ),Fast,FromPresence(0)]
	[Instructions( "You may Gather 1 Beasts. If target land has Beasts, Invaders do not Build there this turn." ), Artist( Artists.MoroRogers )]
	public static async Task ActAsync(TargetSpaceCtx ctx ) {
		// you may gather 1 beast
		await ctx.GatherUpTo(1, Token.Beast);

		if( ctx.Beasts.Any )
			ctx.Space.Skip1Build( Name );

	}

}