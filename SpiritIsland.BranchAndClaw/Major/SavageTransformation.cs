namespace SpiritIsland.BranchAndClaw;

public class SavageTransformation {

	[MajorCard( "Savage Transformation", 2, Element.Moon, Element.Animal ),Slow,FromPresence( 1 )]
	[Instructions( "2 Fear. Replace 1 Explorer with 1 Beasts. -If you have- 2 Moon, 3 Animal: Replace 1 additional Explorer with 1 Beasts in either target or adjacent land." ), Artist( Artists.LoicBelliau )]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {
		// 2 fear
		await ctx.AddFear(2);

		// replace 1 explorer with 1 beast
		await ReplaceExplorerWithBeast( ctx, 0 );

		// if you have 2 moon, 3 animal: 
		if(await ctx.YouHave("2 moon,3 animal"))
			// replace 1 additional explorer with 1 beat in either target or adjacent land
			await ReplaceExplorerWithBeast( ctx, 1 );
		
	}

	static async Task ReplaceExplorerWithBeast( TargetSpaceCtx origCtx, int range ) {
		var options = origCtx.Space.Range( range )
			.SelectMany( ss => ss.SpaceTokensOfTag( Human.Explorer ) );

		SpaceToken spaceToken2 = await origCtx.Self.SelectAsync( new A.SpaceTokenDecision( "Replace additional with Beast", options, Present.Always ) );
		if(spaceToken2 == null) return;

		await spaceToken2.Space.ReplaceAsync( spaceToken2.Token, 1, Token.Beast );
	}

}