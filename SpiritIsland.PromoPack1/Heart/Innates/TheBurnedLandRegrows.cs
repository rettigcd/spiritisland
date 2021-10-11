using System.Threading.Tasks;

namespace SpiritIsland.PromoPack1 {
	[InnatePower( "The Burned Land Regrows" ),Slow]
	[FromPresence(0,Target.Blight)]
	public class TheBurnedLandRegrows {

		[InnateOption( "4 fire,1 plant", "If target land has 2 blight or more, remove 1 blight." )]
		static public Task Option1( TargetSpaceCtx ctx ) { 
			// if target
			if(2 <= ctx.Tokens[TokenType.Blight])
				ctx.RemoveBlight();
			return Task.CompletedTask;
		}

		[InnateOption( "4 fire,2 plant", "Instead, remove 1 blight." )]
		static public Task Option2( TargetSpaceCtx ctx ) { 
			ctx.RemoveBlight();
			return Task.CompletedTask;
		}

		[InnateOption( "5 fire,2 plant,2 earth", "Remove another blight." )]
		static public Task Option3( TargetSpaceCtx ctx ) { 
			ctx.RemoveBlight();
			ctx.RemoveBlight();
			return Task.CompletedTask;
		}


	}

}
