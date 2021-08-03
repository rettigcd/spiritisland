using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Basegame {

	class TallTalesOfSavagery : IFearCard {

		//"Each player removes 1 Explorer from a land with Dahan.", 
		public async Task Level1( GameState gs ) {
			foreach(var spirit in gs.Spirits) {
				var engine = new ActionEngine( spirit, gs );
				var options = gs.Island.AllSpaces.Where(s => gs.HasDahan(s) && gs.InvadersOn(s).HasExplorer ).ToArray();
				if(options.Length==0) return;
				var target = await engine.SelectSpace( "Fear:select land with dahan to remove explorer", options );
				gs.Adjust( target, Invader.Explorer, -1 );
			}
		}

		//"Each player removes 2 Explorer or 1 Town from a land with Dahan.", 
		public Task Level2( GameState gs ) {
			throw new System.NotImplementedException();
		}

		//"Remove 2 Explorer or 1 Town from each land with Dahan. Then, remove 1 City from each land with at least 2 Dahan."),
		public Task Level3( GameState gs ) {
			throw new System.NotImplementedException();
		}

	}
}

