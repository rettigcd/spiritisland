namespace SpiritIsland.NatureIncarnate;

public class FragmentsOfYesterYear {

	public const string Name = "Fragments of YesterYear";

	[MajorCard(Name,7,"sun,moon"),SlowButFastIf("3 sun")]
	[FromPresence(0)]
	[Instructions( "Remove all pieces, then Add the pieces matching target land's Setup symbols. -If you have- 3 sun: This power may be Fast. -If you have- 3 moon: Don't remove Dahan, any Spirit's Presence, or Spirit Token. Don't add Invaders/Blight." ), Artist( Artists.DavidMarkiwsky )]
	static public async Task ActAsync(TargetSpaceCtx ctx){
		bool youHave3Moon = await ctx.YouHave("3 moon");

		// Remove all pieces,
		foreach(IToken token in ctx.Space.OfType<IToken>().ToArray() ) {
			// -If you have- 3 moon: // Don't remove Dahan, any Spirit's Presence, or Spirit Token.
			if(youHave3Moon && token.HasAny(TokenCategory.Presence,TokenCategory.Dahan,TokenCategory.Incarna) ) continue;
			await ctx.Space.RemoveAsync( token, ctx.Space[token], RemoveReason.Removed );
		}

		// then Add the pieces matching target land's Setup symbols.
		if( ctx.SpaceSpec is not SingleSpaceSpec s1 ) return;
		StartUpCounts initialCounts = s1.StartUpCounts;
		await ctx.Space.AddDefaultAsync( Human.Dahan, initialCounts.Dahan );
		if(!youHave3Moon) { // // -If you have- 3 moon: // Don't add Invaders/Blight.
			await ctx.Space.AddDefaultAsync( Human.City, initialCounts.Cities );
			await ctx.Space.AddDefaultAsync( Human.Town, initialCounts.Towns );
			await ctx.Space.AddDefaultAsync( Human.Explorer, initialCounts.Explorers );
			ctx.Space.Blight.Adjust( initialCounts.Blight ); // don't use AddBlight because that pulls it from the card and triggers blighted island
		}
	}

}

