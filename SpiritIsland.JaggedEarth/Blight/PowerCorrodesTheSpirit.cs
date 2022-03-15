namespace SpiritIsland.JaggedEarth;

public class PowerCorrodesTheSpirit : BlightCardBase {

	public PowerCorrodesTheSpirit():base("Power Corrodes the Spirit",4) {}

	public override ActionOption<GameState> Immediately 
		=> Cmd.AtTheStartOfEachInvaderPhase(Cmd.EachSpirit( Cause.Blight, 
			new SelfAction("Destroy 1 presence if spirit has 3 or more Power Cards in play or has a power card in play costing 4 or more Energy.",
					async ctx => {
						// if they have 3 or more Power Cards in play, or have a Power Card in play costing 4 or more (printed) Energy.
						if( 3 <= ctx.Self.InPlay.Count || ctx.Self.InPlay.Any(c=>4<=c.Cost) )
							// Destroys 1 of their presence
							await ctx.Presence.DestroyOne(DestoryPresenceCause.BlightedIsland);
					}
				)
			)
		);

}