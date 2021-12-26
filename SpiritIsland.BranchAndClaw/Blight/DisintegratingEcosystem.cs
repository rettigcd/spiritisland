using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class DisintegratingEcosystem : BlightCardBase {

		public DisintegratingEcosystem():base("Disintegrating Ecosystem", 5 ) { }

		protected override Task BlightAction( GameState gs ) {
			// Immediately, on each board: 
			return GameCmd.OnEachBoard(
				Cmd.Multiple<BoardCtx>(
					// destroy 1 beast,
					BoardCmd.PickSpaceThenTakeAction("Destory 1 beast", Cmd.DestoryBeast(1), space=>gs.Tokens[space].Beasts.Any),
					// then add 1 blight to a land with town/city
					BoardCmd.PickSpaceThenTakeAction("Add 1 blight to a land with town/city", Cmd.AddBlight, space=>gs.Tokens[space].HasAny(Invader.Town,Invader.City) )
				)
			).Execute( gs );

		}
	}

}
