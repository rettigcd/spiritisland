﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Basegame {

	class Isolation : IFearCard {

		//"Each player removes 1 Explorer / Town from a land where it is the only Invader.", 
		public async Task Level1( GameState gs ) {
			foreach(var spirit in gs.Spirits) {
				var options = gs.Island.AllSpaces.Where( s => {
					var grp = gs.InvadersOn(s);
					return grp.HasExplorer && grp[Invader.Explorer] == grp.TotalCount;
				} ).ToArray();
				if(options.Length==0) return;
				var engine = new ActionEngine( spirit, gs );
				var target = await engine.SelectSpace("fear:Select land to remove 1 explorer",options);
				gs.Adjust(target,Invader.Explorer,-1);
			}
		}

		//"Each player removes 1 Explorer / Town from a land with 2 or fewer Invaders.", 
		public Task Level2( GameState gs ) {
			throw new System.NotImplementedException();
		}

		//"Each player removes an Invader from a land with 2 or fewer Invaders."),
		public Task Level3( GameState gs ) {
			throw new System.NotImplementedException();
		}
	}
}
