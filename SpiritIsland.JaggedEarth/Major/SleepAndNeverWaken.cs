namespace SpiritIsland.JaggedEarth;

public class SleepAndNeverWaken {

	const string Name = "Sleep and Never Waken";

	[MajorCard(Name,3,Element.Moon,Element.Air,Element.Earth,Element.Animal), Fast, FromPresenceIn(2,Terrain.Sand)]
	public static async Task ActAsync(TargetSpaceCtx ctx ) {
		// invaders skip all actions in target land.
		ctx.SkipAllInvaderActions( Name );

		// Track # of exlorers removed.
		int removed = 0;
		ctx.GameState.Tokens.TokenRemoved.ForRound.Add( x => {
			if(x.Token.Class == Invader.Explorer)
				removed += x.Count;
		} );

		// remove up to 2 explorer.
		await Cmd.RemoveExplorers(2).Execute(ctx);

		// if you have 3 moon 2 air 2 animal:  Remove up to 6 explorer from among your lands.
		if( await ctx.YouHave( "3 moon,2 air,2 animal") )
			await RemoveExploreres( ctx, 6, ctx.Self.Presence.Spaces.ToArray() );

		// 1 fear per 2 explorer this Power Removes.
		ctx.AddFear( removed / 2 );
	}

	static async Task RemoveExploreres( TargetSpaceCtx ctx, int count, params Space[] fromSpaces ) {

		SpaceToken[] CalcOptions() => fromSpaces
			.SelectMany(
				space => ctx.GameState.Tokens[space].OfType(Invader.Explorer)
					.Select( t => new SpaceToken(space,t) )
			)
			.ToArray();

		SpaceToken[] options;
		while( count-- > 0 
			&& (options=CalcOptions()).Length > 0 
		) {
			var token = await ctx.Decision( new Select.TokenFromManySpaces($"Select Explorer to remove. ({count+1} remaining)", options, Present.Done));
			if(token == null ) break;
			await ctx.Target(token.Space).Remove(token.Token,1);
		}

	}

}