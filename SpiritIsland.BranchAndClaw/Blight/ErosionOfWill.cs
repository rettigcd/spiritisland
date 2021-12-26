using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class ErosionOfWill : BlightCardBase {

		public ErosionOfWill():base("Erosion of Will", 3 ) { }

		protected override Task BlightAction( GameState gs ) {
			// Immediately,
			return Cmd.Multiple(
				// 2 fear per player.
				FearPerPlayer(2),
				// each spirit 
				GameCmd.EachSpirit( Cause.Blight,
					Cmd.Multiple(
						// destroys 1 of their presence and
						SelfCmd.DestoryPresence( ActionType.BlightedIsland ),
						// loses 1 energy
						LoseEnergy(1)
					)
				)
			).Execute( gs );

		}

		static public ActionOption<GameState> FearPerPlayer(int count) => new ActionOption<GameState>($"{count} fear per player", gs => gs.Fear.AddDirect(new FearArgs { count = count } ) );


		static public SelfAction LoseEnergy(int delta) => new SelfAction($"Loose {delta} energy", ctx => ctx.Self.Energy -= System.Math.Max(delta, ctx.Self.Energy) );


	}



}
