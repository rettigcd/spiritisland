using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class Tsunami {

		public const string Name = "Tsunami";

		[MajorCard(Tsunami.Name,6,Speed.Slow,Element.Water,Element.Earth)]
		[FromSacredSite(2,Target.Coastal)]
		static public async Task ActAsync(TargetSpaceCtx ctx){
			// 2 fear
			ctx.AddFear(2);

			// +8 damage
			await ctx.DamageInvaders(8);

			// destroy 2 dahan
			await ctx.DestroyDahan( 2 );

			if(ctx.YouHave("3 water,2 earth")){
				var otherCoastalsOnSameBoard = ctx.Space.Board
					.Spaces.Where( s=> s != ctx.Space && ctx.TargetSpace(s).IsCoastal )
					.ToArray();

				foreach(var otherCoast in otherCoastalsOnSameBoard)
					await DamageOtherCoast( ctx.TargetSpace( otherCoast ) );

			}
		}

		static async Task DamageOtherCoast( TargetSpaceCtx otherCtx ) {
			// 1 fear
			otherCtx.AddFear( 1 );
			// 4 damage
			await otherCtx.DamageInvaders( 4 );
			// destroy 1 dahan
			if(otherCtx.HasDahan)
				await otherCtx.DestroyDahan( 1 );
		}
	}

}
