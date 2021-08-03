using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Basegame {

	class EmigrationAccelerates : IFearCard {

		//"Each player removes 1 Explorer from a Coastal land.", 
		public async Task Level1( GameState gs ) {
			foreach(var spirit in gs.Spirits) {
				var engine = new ActionEngine(spirit,gs);
				var options = gs.Island.AllSpaces.Where(x=>x.IsCostal && gs.InvadersOn(x).HasExplorer).ToArray();
				if(options.Length == 0) break;
				var target = await engine.SelectSpace("Fear:Pick costal land remove explorer",options);
				gs.Adjust(target,Invader.Explorer,-1);
			}
		}

		//"Each player removes 1 Explorer / Town from a Coastal land.", 
		public Task Level2( GameState gs ) {
			throw new System.NotImplementedException();
		}

		//"Each player removes 1 Explorer / Town from any land."),
		public Task Level3( GameState gs ) {
			throw new System.NotImplementedException();
		}

	}
}

