namespace SpiritIsland.JaggedEarth;

public class PowerCorrodesTheSpirit : BlightCard {

	public PowerCorrodesTheSpirit():base("Power Corrodes the Spirit", "Each Invader Phase: Each Spirit Destroys 1 of their presence if they have 3 or more Power Cards in play, or have a Power Card in play costing 4 or more (printed) Energy.", 4) {}

	public override DecisionOption<GameCtx> Immediately 
		=> Cmd.AtTheStartOfEachInvaderPhase(
			Cmd.ForEachSpirit(
				Cmd.DestroyPresence()
					.OnlyExecuteIf( ctx => 3 <= ctx.Self.InPlay.Count || ctx.Self.InPlay.Any( c => 4 <= c.Cost ) )
			) 
		);

}