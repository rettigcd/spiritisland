namespace SpiritIsland.JaggedEarth;

public class MesmerizedTranquility{ 

	[MinorCard("Mesmerized Tranquility",0,Element.Water,Element.Earth,Element.Animal),Fast,FromPresence(0)]
	static public Task ActAsync( TargetSpaceCtx ctx ){
		// Isolate target land.
		ctx.Isolate();

		// Each Invader does -1 Damage.
		++ctx.Tokens.DamagePenaltyPerInvader;
		ctx.GameState.TimePasses_ThisRound.Push(_=>{ ctx.Tokens.DamagePenaltyPerInvader--; return Task.CompletedTask; } );

		return Task.CompletedTask;
	}

}