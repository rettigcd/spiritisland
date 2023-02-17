namespace SpiritIsland.BranchAndClaw;

public class ErosionOfWill : BlightCard {

	public ErosionOfWill():base("Erosion of Will", "Immediately, 2 fear per player. Each Spirit destroys 1 of their presence and loses 1 Energy.", 3 ) { }

	public override DecisionOption<GameCtx> Immediately => Cmd.Multiple(
		// 2 fear per player.
		AddFearPerPlayer(2),
		// each spirit 
		Cmd.ForEachSpirit( Cmd.Multiple(
			// destroys 1 of their presence and
			Cmd.DestroyPresence(),
			// loses 1 energy
			LoseEnergy(1)
		))
	);

	static public DecisionOption<GameCtx> AddFearPerPlayer(int count) 
		=> new DecisionOption<GameCtx>(
			$"Add {count} fear per player", 
			ctx => ctx.GameState.Fear.AddDirect(new FearArgs( count ) )
		);


	static public SelfAction LoseEnergy(int delta) 
		=> new SelfAction(
			$"Loose {delta} energy", 
			ctx => ctx.Self.Energy -= System.Math.Max(delta, ctx.Self.Energy)
		);

}