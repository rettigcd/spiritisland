using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Basegame {

	class DahanRaid : IFearCard {

		//"Each player chooses a different land with Dahan. 1 Damage there.", 
		public async Task Level1( GameState gs ) {
			HashSet<Space> used = new HashSet<Space>();
			foreach(var spirit in gs.Spirits) {
				var engine = new ActionEngine(spirit,gs);
				var options = gs.Island.AllSpaces.Where(gs.HasDahan).Except(used).ToArray();
				var target = await engine.SelectSpace("Fear:select land with dahan for 1 damage",options);
				gs.DamageInvaders(target,1);
				used.Add(target);
			}
		}

		//"Each player chooses a different land with Dahan. 1 Damage per Dahan there.", 
		public Task Level2( GameState gs ) {
			throw new System.NotImplementedException();
		}

		//"Each player chooses a different land with Dahan. 2 Damage per Dahan there."),
		public Task Level3( GameState gs ) {
			throw new System.NotImplementedException();
		}
	}
}
