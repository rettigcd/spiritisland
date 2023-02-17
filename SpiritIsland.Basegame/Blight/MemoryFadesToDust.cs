namespace SpiritIsland.Basegame;

public class MemoryFadesToDust : BlightCard {

	public MemoryFadesToDust() : base( "Memory Fades to Dust", "At the start of each Invader Phase each Spirit Forgets a Power or destroys 1 of their presence.", 4 ) {}

	public override DecisionOption<GameCtx> Immediately => 
		Cmd.Pick1<SelfCtx>(
			Cmd.DestroyPresence(),
			Cmd.ForgetPowerCard
		)
		.ForEachSpirit()
		.AtTheStartOfEachInvaderPhase();

}