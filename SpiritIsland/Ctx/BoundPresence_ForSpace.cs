using System;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class BoundPresence_ForSpace : BoundPresence {

		#region constructor

		readonly TargetSpaceCtx ctx;

		public BoundPresence_ForSpace(TargetSpaceCtx ctx ) : base( ctx ) {
			this.ctx = ctx;
		}

		#endregion

		public bool IsSelfSacredSite => ctx.Self.Presence.SacredSites.Contains(ctx.Space);

		public bool IsHere       => ctx.Self.Presence.IsOn( ctx.Space );

		public int Count => ctx.Self.Presence.CountOn(ctx.Space);

		public async Task PlaceDestroyedHere( int count = 1 ) {
			count = Math.Min(count, ctx.Self.Presence.Destroyed);
			while(count-- > 0 )
				await ctx.Self.PlacePresence( Track.Destroyed, ctx.Space, ctx.GameState );
		} 

		public async Task PlaceHere() {
			var from = await SelectSource();
			await ctx.Self.PlacePresence( from, ctx.Space, ctx.GameState );
		}

		public async Task MoveHereFromAnywhere(int count) {

			while(count > 0) {
				var from = await ctx.Presence.SelectDeployed($"Select presence to move. ({count} remaining)");
				this.Move( from, ctx.Space );
				count--;
			}
		}

	}


}