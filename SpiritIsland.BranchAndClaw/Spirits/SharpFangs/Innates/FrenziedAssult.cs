using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	[InnatePower( "Frenzied Assult" ),Slow]
	[FromPresence( 1, Target.Beast )]
	class FrenziedAssult {

		[InnateOption( "1 moon,1 fire,4 animal","1 fear and 2 Damage. Remove 1 beast." )]
		static public async Task Option1( TargetSpaceCtx ctx ) {
			// 1 fear, 2 damage
			await Execute( ctx, 1, 2 );
		}

		[InnateOption( "1 moon,2 fire,5 animal", "+1 fear and +1 Damage." )]
		static public async Task Option2( TargetSpaceCtx ctx ) {
			// +1 fear, +1 damage
			await Execute( ctx, 1+1, 2+1 );
		}

		static async Task Execute( TargetSpaceCtx ctx, int fear, int damage ) {
			ctx.AddFear( fear );
			await ctx.Invaders.SmartDamageToTypes( damage );
			// remove 1 beast
			ctx.Beasts.Count--;
		}

	}

}
