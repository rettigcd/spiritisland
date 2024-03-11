namespace SpiritIsland.BranchAndClaw;

public class TerrifyingChase {

	[SpiritCard( "Terrifying Chase", 1, Element.Sun, Element.Animal ),Slow,FromPresence( 0 )]
	[Instructions( "Push 2 Explorer / Town / Dahan. Push another 2 Explorer / Town / Dahan per Beasts in target land. If you Pushed any Invaders, 2 Fear." ), Artist( Artists.MoroRogers )]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {

		// push 2 exploeres / towns / dahan
		// push another 2 explorers / towns / dahan pers beast in target land
		int pushCount = 2 + 2 * ctx.Beasts.Count;

		int startingInvaderCount = ctx.Space.InvaderTotal();

		// first push invaders
		await ctx.Push(pushCount, Human.Explorer_Town.Plus(Human.Dahan));

		// if you pushed any invaders, 2 fear
		if( ctx.Space.InvaderTotal() < startingInvaderCount )
			ctx.AddFear(2);

	}

}