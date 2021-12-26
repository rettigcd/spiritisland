using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class PromisingFarmlands : BlightCardBase {

		public PromisingFarmlands():base("Promising Farmlands", 4 ) { }

		protected override Task BlightAction( GameState gs ) {
			// Immediatly, on each board:
			return GameCmd.OnEachBoard(
				BoardCmd.PickSpaceThenTakeAction("Add 1 town and 1 city to an inland land with no town/city", 
					// Add 1 town and 1 city
					Cmd.Multiple(Cmd.AddTown(1),Cmd.AddCity(1)),
					// to an inland land with no town/city
					t => t.Space.IsInland && !t.HasAny(Invader.Town,Invader.City)
				)
			).Execute( gs );
		}

	}

}
