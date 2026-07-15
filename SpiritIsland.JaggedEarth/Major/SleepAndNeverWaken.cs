namespace SpiritIsland.JaggedEarth;

public class SleepAndNeverWaken {

	const string Name = "Sleep and Never Waken";

	[MajorCard(Name,3,Element.Moon,Element.Air,Element.Earth,Element.Animal), Fast, FromPresence(Filter.Sands, 2)]
	[Instructions( "Invaders skip all Actions in target land. 1 Fear per 2 Explorer this Power removes. Remove up to 2 Explorer. -If you have- 3 Moon, 2 Air, 2 Animal: Remove up to 6 Explorer from among your lands." ), Artist( Artists.JoshuaWright )]
	public static async Task ActAsync(TargetSpaceCtx ctx ) {
		// invaders skip all actions in target land.
		ctx.Space.SkipAllInvaderActions( Name );

		// Track # of exlorers removed.
		var removedCounter = new CountDestroyedExplorers();
		ctx.Space.Adjust( removedCounter, 1 );

		// remove up to 2 explorer.
		await Cmd.RemoveExplorers(2).ActAsync(ctx);

		// if you have 3 moon 2 air 2 animal:  Remove up to 6 explorer from among your lands.
		if( await ctx.YouHave( "3 moon,2 air,2 animal") )
			await RemoveExploreres( ctx, 6, ctx.Self.Presence.Lands.ToArray() );

		// 1 fear per 2 explorer this Power Removes.
		await ctx.AddFear( removedCounter.Count / 2 );
	}

	public class CountDestroyedExplorers : BaseModEntity, IHandleTokenRemoved {
		public int Count { get; private set; }
		public Task HandleTokenRemovedAsync( ITokenRemovedArgs args ) {
			if(args.Removed.Class == Human.Explorer)
				Count += args.Count;
			return Task.CompletedTask;
		}
	}

	static async Task RemoveExploreres( TargetSpaceCtx ctx, int count, params Space[] fromSpaces ) {

		SpaceToken[] CalcOptions() => fromSpaces
			.SelectMany( space => space.SpaceTokensOfTag(Human.Explorer) )
			.ToArray();

		SpaceToken[] options;
		while( count-- > 0 
			&& (options=CalcOptions()).Length > 0 
		) {
			var spaceToken = await ctx.Self.Select( new A.SpaceTokenDecision($"Select Explorer to remove. ({count+1} remaining)", options, Present.Done));
			if(spaceToken is null ) break;
			await ctx.Target(spaceToken.Space).Remove(spaceToken.Token,1);
		}

	}

}