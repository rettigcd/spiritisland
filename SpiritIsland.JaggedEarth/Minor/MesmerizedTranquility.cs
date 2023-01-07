namespace SpiritIsland.JaggedEarth;

public class MesmerizedTranquility{ 

	[MinorCard("Mesmerized Tranquility",0,Element.Water,Element.Earth,Element.Animal),Fast,FromPresence(0)]
	static public Task ActAsync( TargetSpaceCtx ctx ){
		// Isolate target land.
		ctx.Isolate();

		// Each Invader does -1 Damage.
		ctx.GameState.ModifyRavage( ctx.Space, behavior => {
			var old = behavior.AttackDamageFrom1;
			behavior.AttackDamageFrom1 = (ss,t) => Math.Max(0, old(ss,t)-1);
		});

		return Task.CompletedTask;
	}

}