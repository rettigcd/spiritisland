using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class DahanRaid : IFearOptions {

		public const string Name = "Dahan Raid";

		[FearLevel(1, "Each player chooses a different land with Dahan. 1 Damage there.")]
		public Task Level1( FearCtx ctx ) {
			return ForEachPlayerChosenLandWithDahan( ctx.GameState, ( s ) => ctx.GameState.SpiritFree_FearCard_DamageInvaders( s, 1 ) );
		}


		[FearLevel( 2, "Each player chooses a different land with Dahan. 1 Damage per Dahan there." )]
		public Task Level2( FearCtx ctx ) {
			var gs = ctx.GameState;
			return ForEachPlayerChosenLandWithDahan( gs, ( s ) => gs.SpiritFree_FearCard_DamageInvaders( s, gs.DahanGetCount(s) ) );
		}

		[FearLevel( 3, "Each player chooses a different land with Dahan. 2 Damage per Dahan there." )]
		public Task Level3( FearCtx ctx ) {
			var gs = ctx.GameState;
			return ForEachPlayerChosenLandWithDahan( gs, ( s ) => gs.SpiritFree_FearCard_DamageInvaders( s, 2*gs.DahanGetCount( s ) ) );
		}

		static async Task ForEachPlayerChosenLandWithDahan( GameState gs, Func<Space,Task> action ) {
			HashSet<Space> used = new ();
			foreach(var spirit in gs.Spirits) {
				var options = gs.Island.AllSpaces.Where( gs.DahanIsOn ).Except( used ).ToArray();
				var target = await spirit.Action.Decision( new Decision.TargetSpace( "Fear:select land with dahan", options, Present.Always ));
				used.Add( target );
				await action( target );
			}
		}
	}
}
