
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class WindsOfRustAndAtrophy {

		[MajorCard("Winds of Rust and Atrophy",3,Speed.Fast,Element.Air,Element.Moon,Element.Animal)]
		[FromSacredSite(3)]
		static public async Task ActAsync(TargetSpaceCtx ctx) {
			var (self, _) = ctx;
			await ApplyEffect( ctx, ctx.Target );

			// if you have 3 air 3 water 2 animal, repeat this power
			if(self.Elements.Contains("3 air,2 water,2 animal" )) {
				var secondTarget = await ctx.TargetSpace(From.SacredSite, null, 3,Target.Any);
				await ApplyEffect( ctx, secondTarget);
			}
		}

		static async Task ApplyEffect( TargetSpaceCtx ctx, Space target ) {
			var (_,gs) = ctx;
			// 1 fear and defend 6
			ctx.AddFear( 1 );
			gs.Defend( target, 6 );

			// replace 1 city with 1 town OR 1 town with 1 explorer
			var grp = gs.InvadersOn( target );
			var options = grp.FilterBy( Invader.City, Invader.Town );
			var invader = await ctx.Self.SelectInvader( "Select invader to downgrade (city>town,town>explorer)", options );
			if(invader.Generic == Invader.City) {
				gs.Adjust( target, invader, -1 );
				gs.Adjust( target, InvaderSpecific.Town, 1 );
			} else if(invader.Generic == Invader.Town) {
				gs.Adjust( target, invader, -1 );
				gs.Adjust( target, InvaderSpecific.Explorer, 1 );
			}
		}
	}
}
