using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class DahanRaid : IFearCard {

		public const string Name = "Dahan Raid";

		[FearLevel(1, "Each player chooses a different land with Dahan. 1 Damage there.")]
		public Task Level1( GameState gs ) {
			return ForEachPlayerChosenLandWithDahan( gs, ( s ) => gs.SpiritFree_DamageInvaders( s, 1 ) );
		}


		[FearLevel( 2, "Each player chooses a different land with Dahan. 1 Damage per Dahan there." )]
		public Task Level2( GameState gs ) {
			return ForEachPlayerChosenLandWithDahan( gs, ( s ) => gs.SpiritFree_DamageInvaders( s, gs.DahanCount(s) ) );
		}

		[FearLevel( 3, "Each player chooses a different land with Dahan. 2 Damage per Dahan there." )]
		public Task Level3( GameState gs ) {
			return ForEachPlayerChosenLandWithDahan( gs, ( s ) => gs.SpiritFree_DamageInvaders( s, 2*gs.DahanCount( s ) ) );
		}

		static async Task ForEachPlayerChosenLandWithDahan( GameState gs, Func<Space,Task> action ) {
			HashSet<Space> used = new HashSet<Space>();
			foreach(var spirit in gs.Spirits) {
				var options = gs.Island.AllSpaces.Where( gs.HasDahan ).Except( used ).ToArray();
				var target = await spirit.Action.Choose( new TargetSpaceDecision( "Fear:select land with dahan", options ));
				used.Add( target );
				await action( target );
			}
		}
	}
}
