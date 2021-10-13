
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland.Basegame {

	public class OceanPresence : SpiritPresence {

		public OceanPresence( PresenceTrack energy, PresenceTrack cardPlays ) : base( energy, cardPlays ) { }

		public override void PlaceOn( Space space ) {
			base.PlaceOn( space );
			currentBoards.Add( space.Board );
		}

		public override void RemoveFrom( Space space ) {
			base.RemoveFrom( space );
			var board = space.Board;
			if(!Spaces.Any(s=>s.Board == board ))
				currentBoards.Remove( board );
		}

		public bool IsOnBoard(Board board) => currentBoards.Contains(board);

		readonly HashSet<Board> currentBoards = new HashSet<Board>();

	}

}
