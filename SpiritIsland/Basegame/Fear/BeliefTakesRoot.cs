using SpiritIsland.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class BeliefTakesRoot : IFearCard {

		[FearLevel( 1, "Defend 2 in all lands with Presence." )]
		public Task Level1( GameState gs ) {
			Defend2WherePresence( gs );
			return Task.CompletedTask;
		}

		static void Defend2WherePresence( GameState gs ) {
			foreach(var space in gs.Spirits.SelectMany( s => s.Presence.Spaces ).Distinct())
				gs.Defend( space, 2 );
		}

		[FearLevel( 2, "Defend 2 in all lands with Presence. Each Spirit gains 1 Energy per SacredSite they have in lands with Invaders." )]
		public Task Level2( GameState gs ) {
			Defend2WherePresence( gs );
			foreach(var spirit in gs.Spirits)
				spirit.Energy += spirit.SacredSites.Count( gs.HasInvaders );
			return Task.CompletedTask;
		}

		[FearLevel( 3, "Each player chooses a different land and removes up to 2 Health worth of Invaders per Presence there." )]
		public async Task Level3( GameState gs ) {
			var used = new HashSet<Space>();
			foreach(var spirit in gs.Spirits) {
				var engine = new ActionEngine(spirit,gs);
				var options = spirit.Presence.Spaces
					.Where(s=>gs.InvadersOn(s).Has(Invader.Town,Invader.Explorer))
					.Except(used)
					.ToArray();
				if(options.Length==0) continue;
				var target = await engine.SelectSpace("Select land to remove 2 health worth of invaders/presence.",options);
				used.Add(target);
				var grp = gs.InvadersOn(target);
				if(grp.HasTown)
					gs.Adjust(target,Invader.Town,-1);
				else
					gs.Adjust(target,Invader.Explorer,-2);
			}
		}
	}
}

