namespace SpiritIsland.BranchAndClaw {

	public class ErosionOfWill : BlightCardBase {

		public ErosionOfWill():base("Erosion of Will", 3 ) { }

		public override ActionOption<GameState> Immediately => Cmd.Multiple(
			// 2 fear per player.
			AddFearPerPlayer(2),
			// each spirit 
			Cmd.EachSpirit( Cause.Blight, Cmd.Multiple(
				// destroys 1 of their presence and
				Cmd.DestroyPresence( ActionType.BlightedIsland ),
				// loses 1 energy
				LoseEnergy(1)
			))
		);

		static public ActionOption<GameState> AddFearPerPlayer(int count) 
			=> new ActionOption<GameState>(
				$"Add {count} fear per player", 
				gs => gs.Fear.AddDirect(new FearArgs { count = count } )
			);


		static public SelfAction LoseEnergy(int delta) 
			=> new SelfAction(
				$"Loose {delta} energy", 
				ctx => ctx.Self.Energy -= System.Math.Max(delta, ctx.Self.Energy)
			);


	}



}
