namespace SpiritIsland.Basegame;

public class ConcealingShadows {

	const string Name = "Concealing Shadows";

	[SpiritCard(Name,0,Element.Moon,Element.Air),Fast,FromPresence(0)]
	[Instructions( "1 Fear. Dahan take no damage from Ravaging Invaders this turn." ), Artist( Artists.NolanNasser )]
	static public Task Act(TargetSpaceCtx ctx){
		// 1 fear
		ctx.AddFear(1);

		// dahan take no damage from ravaging invaders this turn
		ctx.Tokens.Init( new StopDahanDamageAndDestruction( Name ), 1 ); // stop damage other times

		return Task.CompletedTask;
	}

}