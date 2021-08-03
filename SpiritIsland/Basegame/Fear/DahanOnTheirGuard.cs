using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Basegame {

	public class DahanOnTheirGuard : IFearCard {

		// "In each land, Defend 1 per Dahan.", 
		public Task Level1( GameState gs ) {
			foreach(var space in gs.Island.AllSpaces.Where( gs.HasDahan ))
				gs.Defend( space, gs.GetDahanOnSpace( space ) );
			return Task.CompletedTask;
		}

		// "In each land with Dahan, Defend 1, plus an additional Defend 1 per Dahan.", 
		public Task Level2( GameState gs ) {
			throw new System.NotImplementedException();
		}

		// "In each land, Defend 2 per Dahan."),
		Task IFearCard.Level3( GameState gs ){
			throw new System.NotImplementedException();
		}
	}
}
