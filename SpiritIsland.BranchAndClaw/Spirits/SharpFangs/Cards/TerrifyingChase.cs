namespace SpiritIsland.BranchAndClaw;

public class TerrifyingChase {

	[SpiritCard( "Terrifying Chase", 1, Element.Sun, Element.Animal )]
	[Slow]
	[FromPresence( 0 )]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {

		// push 2 exploeres / towns / dahan
		// push another 2 explorers / towns / dahan pers beast in target land
		int pushCount = 2 + 2 * ctx.Beasts.Count;

		int startingInvaderCount = ctx.Tokens.InvaderTotal();

		// first push invaders
		await ctx.Push(pushCount, Invader.Explorer_Town.Plus(TokenType.Dahan));

		// if you pushed any invaders, 2 fear
		if( ctx.Tokens.InvaderTotal() < startingInvaderCount )
			ctx.AddFear(2);

	}

}