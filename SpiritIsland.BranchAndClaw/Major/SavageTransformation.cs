namespace SpiritIsland.BranchAndClaw;

public class SavageTransformation {

	[MajorCard( "Savage Transformation", 2, Element.Moon, Element.Animal )]
	[Slow]
	[FromPresence( 1 )]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {
		// 2 fear
		ctx.AddFear(2);

		// replace 1 explorer with 1 beast
		if(ctx.Tokens.Has( Invader.Explorer ))
			await ReplaceExplorerWithBeast( ctx );

		// if you have 2 moon, 3 animal: 
		if(await ctx.YouHave("2 moon,3 animal")) {
			// replace 1 additional explorer with 1 beat in either target or adjacent land
			var secondSpaceCtx = await ctx.SelectAdjacentLandOrSelf( "convert 2nd explorer to beast", x=>x.Tokens.Has(Invader.Explorer) );
			if(secondSpaceCtx != null )
				await ReplaceExplorerWithBeast( secondSpaceCtx );
		}
	}

	static Task ReplaceExplorerWithBeast( TargetSpaceCtx ctx ) {
		ctx.Invaders.Remove(Invader.Explorer.Default,1,RemoveReason.Replaced);
		return ctx.Beasts.Add(1,AddReason.AsReplacement);
	}

}