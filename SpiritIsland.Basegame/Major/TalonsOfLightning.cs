using System;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class TalonsOfLightning {

		[MajorCard( "Talons of Lightning", 6, Speed.Fast, Element.Fire, Element.Air )]
		[TalonsOfLightning.Custom( From.Presence, 1, Target.MountainOrWetland)]
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

		// range 1
		[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method )]
		public class CustomAttribute : SpiritIsland.TargetSpaceAttribute {
			public CustomAttribute( From from, int range,string targetType) : base( from, null, range, targetType ) { }
			protected override int CalcRange( SpiritGameStateCtx ctx ) 
				=> ctx.YouHave("3 fire,3 air") ? 3 : range; // aka 1
		}

	}

}

