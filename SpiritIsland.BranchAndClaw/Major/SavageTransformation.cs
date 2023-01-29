namespace SpiritIsland.BranchAndClaw;

public class SavageTransformation {

	[MajorCard( "Savage Transformation", 2, Element.Moon, Element.Animal )]
	[Slow]
	[FromPresence( 1 )]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {
		// 2 fear
		ctx.AddFear(2);

		// replace 1 explorer with 1 beast
		await ReplaceExplorerWithBeast( ctx, 0 );

		// if you have 2 moon, 3 animal: 
		if(await ctx.YouHave("2 moon,3 animal"))
			// replace 1 additional explorer with 1 beat in either target or adjacent land
			await ReplaceExplorerWithBeast( ctx, 1 );
		
	}

	static async Task ReplaceExplorerWithBeast( TargetSpaceCtx origCtx, int range ) {
		var options = origCtx.Tokens.Range( range )
			.SelectMany(
				ss => ss.OfClass( Invader.Explorer ).Select( t => new SpaceToken( ss.Space, (IVisibleToken)t ) ) // !!! does this happen offent enough to make it a method on SpaceState???
			);

		SpaceToken spaceToken2 = await origCtx.Self.Gateway.Decision( new Select.TokenFromManySpaces( "Replace additional with Beast", options, Present.Always ) );
		if(spaceToken2 == null) return;

		var actionCtx = origCtx.Target( spaceToken2.Space ); // allow
		await actionCtx.Invaders.Remove( spaceToken2.Token, 1, RemoveReason.Replaced );
		await actionCtx.Beasts.Add( 1, AddReason.AsReplacement );

	}

}