using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class BeliefTakesRoot : IFearCard {

		public const string Name = "Belief takes Root";

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
			foreach(var spirit in gs.Spirits)
				await X(spirit,used,gs);

		}

		static async Task X(Spirit spirit, HashSet<Space> used, GameState gs) {
			// pick land
			var targetOptions = spirit.Presence.Spaces
				.Where( s => gs.Tokens[s].HasAny( Invader.Town, Invader.Explorer ) )
				.Except( used )
				.ToArray();
			if(targetOptions.Length==0) return;
			var target = await spirit.Action.Decision( new Decision.TargetSpace( "Select land to remove 2 health worth of invaders/presence.", targetOptions ) );
			// mark used
			used.Add(target);

			int damage = 2 * spirit.Presence.Placed.Count(x=>x==target);

			var counts = gs.Tokens[target];
			Token pick;
			while(damage > 0 
				&& (pick = SmartInvaderDamageExtensions.PickSmartInvaderToDamage( counts, damage ))!=null
			) {
				--counts[pick];
				damage -= pick.Health;
			}
		}
	}
}

