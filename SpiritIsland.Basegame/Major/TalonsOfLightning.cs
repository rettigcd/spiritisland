using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class TalonsOfLightning {

		[MajorCard( "Talons of Lightning", 6, Speed.Fast, Element.Fire, Element.Air )]
		[ExtendableRange( From.Presence, 1, Target.MountainOrWetland, "3 fire,3 air", 2 )]
		static public async Task ActionAsync( TargetSpaceCtx ctx ) {

			// 3 fear
			ctx.AddFear(3);

			// 5 damage
			await ctx.DamageInvaders(5);

			if( ctx.YouHave( "3 fire,3 air" )){
				// destory 1 town in each adjacent land
				foreach(var neighbor in ctx.Space.Adjacent)
					await ctx.InvadersOn(neighbor).Destroy( 1,Invader.Town);

			}

		}

	}

}

