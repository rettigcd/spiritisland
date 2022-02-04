namespace SpiritIsland.PromoPack1;

[InnatePower( "The Burned Land Regrows" ),Slow]
[FromPresence(0,Target.Blight)]
public class TheBurnedLandRegrows {

	[InnateOption( "4 fire,1 plant", "If target land has 2 blight or more, remove 1 blight." )]
	static public async Task Option1( TargetSpaceCtx ctx ) { 
		// if target
		if(2 <= ctx.Tokens[TokenType.Blight])
			await ctx.RemoveBlight();
	}

	[InnateOption( "4 fire,2 plant", "Instead, remove 1 blight." )]
	static public Task Option2( TargetSpaceCtx ctx ) { 
		return ctx.RemoveBlight();
	}

	[InnateOption( "5 fire,2 plant,2 earth", "Remove another blight." )]
	static public async Task Option3( TargetSpaceCtx ctx ) { 
		await ctx.RemoveBlight();
		await ctx .RemoveBlight();
	}

}