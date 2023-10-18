namespace SpiritIsland.JaggedEarth;

public class MesmerizedTranquility{ 

	[MinorCard("Mesmerized Tranquility",0,Element.Water,Element.Earth,Element.Animal),Fast,FromPresence(0)]
	[Instructions( "Isolate target land. Each Invader does -1 Damage." ), Artist( Artists.KatGuevara )]
	static public Task ActAsync( TargetSpaceCtx ctx ){
		// Isolate target land.
		ctx.Isolate();

		// Each Invader does -1 Damage.
		// RavageBehavior cfg = ctx.Tokens.RavageBehavior;
		// Func<SpaceState, HumanToken, int> old = cfg.AttackDamageFrom1;
		// cfg.AttackDamageFrom1 = (ss,t) => Math.Max(0, old(ss,t)-1);
		ctx.Tokens.Adjust( new ReduceInvaderAttackBy1(1,Human.Invader), 1 );

		return Task.CompletedTask;
	}

}


