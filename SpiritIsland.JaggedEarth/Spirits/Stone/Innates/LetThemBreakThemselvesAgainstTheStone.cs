using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	[InnatePower("Let Them Break Themselves Against the Stone"), Fast, FromPresence(0)]
	[RepeatIf( "7 earth, 2 sun")]
	class LetThemBreakThemselvesAgainstTheStone {

		[InnateOption("3 earth","After Invaders deal 1 or more Damage to target land, 2 Damage")]
		static public Task Option0(TargetSpaceCtx ctx ) {
			ctx.GameState.LandDamaged.ForRound.Add( async (gs, args) =>{
				if(args.Space == ctx.Space)
					await gs.Invaders.On(args.Space,Cause.Power).UserSelectedDamage(2,ctx.Self);
			});
			return Task.CompletedTask;
		}

		[InnateOption("5 earth","Also deal half of the Damage Invaders did to the land (rounding down)")]
		static public Task Option1(TargetSpaceCtx ctx ) {
			ctx.GameState.LandDamaged.ForRound.Add( async ( gs, args ) => {
				if(args.Space == ctx.Space)
					await gs.Invaders.On( args.Space, Cause.Power ).UserSelectedDamage( 2 + args.Damage/2, ctx.Self );
			} );
			return Task.CompletedTask;
		}

	}

}
