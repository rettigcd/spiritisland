namespace SpiritIsland.Basegame;

public class MemoryFadesToDust : BlightCardBase {

	public MemoryFadesToDust() : base( "Memory Fades to Dust", 4 ) {}

	public override ActionOption<GameState> Immediately => Cmd.AtTheStartOfEachInvaderPhase(
		Cmd.EachSpirit(Cause.Blight,
			Cmd.Pick1<SelfCtx>(
				Cmd.DestroyPresence(DestoryPresenceCause.BlightedIsland),
				Cmd.ForgetPowerCard
			)
		)
	);

}