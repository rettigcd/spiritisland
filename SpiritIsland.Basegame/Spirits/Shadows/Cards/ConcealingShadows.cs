namespace SpiritIsland.Basegame;

public class ConcealingShadows {

	[SpiritCard("Concealing Shadows",0,Element.Moon,Element.Air)]
	[Fast]
	[FromPresence(0)]
	static public Task Act(TargetSpaceCtx ctx){
		// 1 fear
		ctx.AddFear(1);

		// dahan take no damage from ravaging invaders this turn
		ctx.Tokens.RavageBehavior.DamageDefenders = ( _, _1, _2 ) => Task.CompletedTask;

		return Task.CompletedTask;
	}

}