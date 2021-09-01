using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	[InnatePower( "Frenzied Assult", Speed.Slow )]
	[FromPresence( 1, Target.BeastOrJungle )]  // !!! BEAST only
	class FrenziedAssult {

		[InnateOption( "1 moon,1 fire,4 animal" )]
		static public async Task Option1( TargetSpaceCtx ctx ) {
			// 1 fear, 2 damage
			await Execute( ctx, 1, 2 );
		}

		[InnateOption( "1 moon,2 fire,5 animal" )]
		static public async Task Option2( TargetSpaceCtx ctx ) {
			// +1 fear, +1 damage
			await Execute( ctx, 1+1, 2+1 );
		}

		static async Task Execute( TargetSpaceCtx ctx, int fear, int damage ) {
			ctx.AddFear( fear );
			await ctx.PowerInvaders.SmartDamageToTypes( damage );
			// remove 1 beast
			ctx.GameState.BAC().Beasts.RemoveOneFrom( ctx.Target );
		}

	}

}
