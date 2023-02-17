namespace SpiritIsland.JaggedEarth;

public class EntrapTheForcesOfCorruption{ 
		
	[MinorCard("Entrap the Forces of Corruption",1,Element.Earth,Element.Plant,Element.Animal),Fast,FromPresence(1)]
	static public async Task ActAsync( TargetSpaceCtx ctx ){
		// Gather up to 1 Blight
		await ctx.GatherUpTo(1,Token.Blight);

		// Isolate target land.
		ctx.Isolate();

		// When blight is added to target land, it doesn't cascade.
		ctx.Tokens.Init(new StopCascade(),1);
	}

	class StopCascade : BaseModEntity, IEndWhenTimePasses, IModifyAddingToken {
		public void ModifyAdding( AddingTokenArgs args ) {
			if(args.Token == Token.Blight)
				BlightToken.ForThisAction.ShouldCascade = false;
		}
	}

}