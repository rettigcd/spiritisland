using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class DahanRaid : IFearOptions {

		public const string Name = "Dahan Raid";

		[FearLevel(1, "Each player chooses a different land with Dahan. 1 Damage there.")]
		public Task Level1( FearCtx ctx ) {
			return ForEachPlayerChosenLandWithDahan( ctx, sCtx=>sCtx.DamageInvaders( 1 ) );
		}

		[FearLevel( 2, "Each player chooses a different land with Dahan. 1 Damage per Dahan there." )]
		public Task Level2( FearCtx ctx ) {
			return ForEachPlayerChosenLandWithDahan( ctx, sCtx => sCtx.DamageInvaders( sCtx.DahanCount ) );
		}

		[FearLevel( 3, "Each player chooses a different land with Dahan. 2 Damage per Dahan there." )]
		public Task Level3( FearCtx ctx ) {
			return ForEachPlayerChosenLandWithDahan( ctx, sCtx => sCtx.DamageInvaders( sCtx.DahanCount * 2 ) );
		}

		static async Task ForEachPlayerChosenLandWithDahan( FearCtx ctx, Func<TargetSpaceCtx,Task> action ) {

			const string prompt = "Fear:select land with dahan";

			HashSet<Space> used = new ();
			foreach(var spiritCtx in ctx.Spirits) {
				// Select un-used space
				var options = spiritCtx.AllSpaces.Where( s=>spiritCtx.Target(s).HasDahan ).Except( used ).ToArray();
				var target = await spiritCtx.Self.Action.Decision( new Decision.TargetSpace( prompt, options, Present.Always ));
				used.Add( target );
				TargetSpaceCtx spactCtx = spiritCtx.Target(target);

				await action(spactCtx);
			}
		}
	}
}
