using SpiritIsland.Core;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class FearOfTheUnseen : IFearCard {

		[FearLevel( 1, "Each player removes 1 Explorer / Town from a land with SacredSite." )]
		public async Task Level1( GameState gs ) {
			foreach(var spirit in gs.Spirits) {
				var engine = new ActionEngine(spirit,gs);
				var options = spirit.SacredSites.Where(s=>gs.InvadersOn(s).Has(Invader.Explorer,Invader.Town)).ToArray();
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
		public async Task Level2( GameState gs ) {
			foreach(var spirit in gs.Spirits) {
				var engine = new ActionEngine( spirit, gs );
				var options = spirit.Presence.Spaces.Where( s => gs.InvadersOn( s ).Has( Invader.Explorer, Invader.Town ) )
					.Union(spirit.SacredSites.Where(s=>gs.InvadersOn(s).HasCity))
					.ToArray();
				if(options.Length == 0) return;
				var target = await engine.SelectSpace( "Select land to remove 1 explorer/town/city.", options );
				var grp = gs.InvadersOn( target );
				var invaderToRemove = spirit.SacredSites.Contains(target) && grp[Invader.City]>0 ? Invader.City2
					: grp[Invader.Town] > 0 ? Invader.Town
					: grp[Invader.Town1] > 0 ? Invader.Town1
					: Invader.Explorer;
				gs.Adjust( target, invaderToRemove, -1 );
			}
		}

		[FearLevel( 3, "Each player removes 1 Explorer / Town from a land with Presence, or 1 City from a land with SacredSite." )]
		public async Task Level3( GameState gs ) {
			Space[] sacredSites = gs.Spirits.SelectMany( spirit => spirit.Presence.Spaces ).Distinct().ToArray(); // !!! is this SS or Presence?
			foreach(var spirit in gs.Spirits) {
				var engine = new ActionEngine( spirit, gs );
				var options = sacredSites.Where( s => gs.InvadersOn( s ).Has( Invader.Explorer, Invader.Town ) ).ToArray();
				if(options.Length == 0) return;
				var target = await engine.SelectSpace( "Select SS land to remove 1 explorer/town.", options );
				var grp = gs.InvadersOn( target );
				var invaderToRemove = (grp[Invader.Town] > 0) ? Invader.Town
					: grp[Invader.Town1] > 0 ? Invader.Town1
					: Invader.Explorer;
				gs.Adjust( target, invaderToRemove, -1 );
			}
		}
	}
}

