
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class OceanPresence : SpiritPresence {

		public OceanPresence( PresenceTrack energy, PresenceTrack cardPlays ) : base( energy, cardPlays ) {
			IsValid = (s) => s.IsOcean || s.IsCoastal;
		}

		public override async Task PlaceOn( Space space, GameState gs ) {
			await base.PlaceOn( space, gs );
			currentBoards.Add( space.Board );
		}

		protected override async Task RemoveFrom_NoCheck( Space space, GameState gs ) {
			await base.RemoveFrom_NoCheck( space, gs );
			var board = space.Board;
			if(!Spaces.Any(s=>s.Board == board ))
				currentBoards.Remove( board );
		}

		public bool IsOnBoard(Board board) => currentBoards.Contains(board);

		readonly HashSet<Board> currentBoards = new HashSet<Board>();

	}

}
