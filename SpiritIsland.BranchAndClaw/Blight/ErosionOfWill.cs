namespace SpiritIsland.BranchAndClaw;

public class ErosionOfWill : BlightCard {

	public ErosionOfWill():base("Erosion of Will", "Immediately, 2 fear per player. Each Spirit destroys 1 of their presence and loses 1 Energy.", 3 ) { }

	public override BaseCmd<GameState> Immediately => Cmd.Multiple(
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

	static public BaseCmd<GameState> AddFearPerPlayer(int count) 
		=> new BaseCmd<GameState>(
			$"Add {count} fear per player", 
			ctx => ctx.Fear.AddDirect(new FearArgs( count ) )
		);


	static public SpiritAction LoseEnergy(int delta) 
		=> new SpiritAction(
			$"Loose {delta} energy", 
			self => self.Energy -= Math.Max(delta, self.Energy)
		);

}