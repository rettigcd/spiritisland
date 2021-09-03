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

			if( Has3Fire3Air( ctx.Self ) )
				// destory 1 town in each adjacent land
				foreach(var neighbor in ctx.Target.Adjacent)
					await ctx.PowerInvadersOn(neighbor).Destroy( 1,Invader.Town);

		}

		static bool Has3Fire3Air(Spirit spirit) => spirit.Elements.Contains( "3 fire,3 air" );


		// range 1
		[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method )]
		public class CustomAttribute : SpiritIsland.TargetSpaceAttribute {
			public CustomAttribute( From from, int range,Target targetType) : base( from, null, range, targetType ) { }
			protected override int CalcRange( IMakeGamestateDecisions ctx ) 
				=> Has3Fire3Air(ctx.Self) ? 3 : range; // aka 1
		}

	}

}

