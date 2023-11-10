namespace SpiritIsland.JaggedEarth;

public class SleepAndNeverWaken {

	const string Name = "Sleep and Never Waken";

	[MajorCard(Name,3,Element.Moon,Element.Air,Element.Earth,Element.Animal), Fast, FromPresenceIn(Target.Sand, 2)]
	[Instructions( "Invaders skip all Actions in target land. 1 Fear per 2 Explorer this Power removes. Remove up to 2 Explorer. -If you have- 3 Moon, 2 Air, 2 Animal: Remove up to 6 Explorer from among your lands." ), Artist( Artists.JoshuaWright )]
	public static async Task ActAsync(TargetSpaceCtx ctx ) {
		// invaders skip all actions in target land.
		ctx.Tokens.SkipAllInvaderActions( Name );

		// Track # of exlorers removed.
		int removed = 0;
		void CountDestroyedExplorers( ITokenRemovedArgs args ) {
			if(args.Removed.Class == Human.Explorer)
				removed += args.Count;
		}
		ctx.Tokens.Adjust( new TokenRemovedHandler(CountDestroyedExplorers), 1 );

		// remove up to 2 explorer.
		await Cmd.RemoveExplorers(2).ActAsync(ctx);

		// if you have 3 moon 2 air 2 animal:  Remove up to 6 explorer from among your lands.
		if( await ctx.YouHave( "3 moon,2 air,2 animal") )
			await RemoveExploreres( ctx, 6, ctx.Self.Presence.Spaces.ToArray() );

		// 1 fear per 2 explorer this Power Removes.
		ctx.AddFear( removed / 2 );
	}

	static async Task RemoveExploreres( TargetSpaceCtx ctx, int count, params Space[] fromSpaces ) {

		SpaceToken[] CalcOptions() => fromSpaces
			.SelectMany( space => space.Tokens.SpaceTokensOfClass(Human.Explorer) )
			.ToArray();

		SpaceToken[] options;
		while( count-- > 0 
			&& (options=CalcOptions()).Length > 0 
		) {
			var spaceToken = await ctx.SelectAsync( new A.SpaceToken($"Select Explorer to remove. ({count+1} remaining)", options, Present.Done));
			if(spaceToken == null ) break;
			await ctx.Target(spaceToken.Space).Remove(spaceToken.Token,1);
		}

	}

}