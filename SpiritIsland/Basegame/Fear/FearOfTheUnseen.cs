﻿using SpiritIsland.Core;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class FearOfTheUnseen : IFearCard {

		[FearLevel( 1, "Each player removes 1 Explorer / Town from a land with SacredSite." )]
		public async Task Level1( GameState gs ) {
			Space[] sacredSites = gs.Spirits.SelectMany(spirit=>spirit.SacredSites).Distinct().ToArray();
			foreach(var spirit in gs.Spirits) {
				var engine = new ActionEngine(spirit,gs);
				var options = sacredSites.Where(s=>gs.InvadersOn(s).Has(Invader.Explorer,Invader.Town)).ToArray();
				if(options.Length==0) return;
				var target = await engine.SelectSpace("Select SS land to remove 1 explorer/town.",options);
				var grp = gs.InvadersOn(target);
				var invaderToRemove = (grp[Invader.Town]>0 ) ? Invader.Town
					: grp[Invader.Town1]>0 ? Invader.Town1
					: Invader.Explorer;
				gs.Adjust(target,invaderToRemove,-1);
			}
		}

		[FearLevel( 2, "Each player removes 1 Explorer / Town from a land with Presence." )]
		public Task Level2( GameState gs ) {
			throw new System.NotImplementedException();
		}

		[FearLevel( 3, "Each player removes 1 Explorer / Town from a land with Presence, or 1 City from a land with SacredSite." )]
		public Task Level3( GameState gs ) {
			throw new System.NotImplementedException();
		}
	}
}
