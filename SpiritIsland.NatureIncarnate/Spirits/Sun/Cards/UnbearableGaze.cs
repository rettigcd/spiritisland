namespace SpiritIsland.NatureIncarnate;

public class UnbearableGaze {

	const string Name = "Unbearable Gaze";

	[SpiritCard(Name,1,Element.Sun,Element.Fire),Slow,FromSacredSite(1)]
	[Instructions( "1 Fear. Push 2 Explorer/Town from origin or target land  (or 1 Explorer/Town from each)." ), Artist( Artists.AgnieszkaDabrowiecka )]
	static public async Task ActAsync(TargetSpaceCtx ctx ) {
		// 1 Fear.
		ctx.AddFear( 1 );

		// Push 2 Explorer/Town from origin or target land  (or 1 Explorer/Town from each).
		await PushFromTargetOrOrigin( ctx, 2, Human.Explorer_Town );
	}

	static async Task PushFromTargetOrOrigin( TargetSpaceCtx ctx, int max, params HumanTokenClass[] tokenClasses ) {
		// Load all possible Sources into a list
		HashSet<SpaceState> sourceOptions = FindSacredSitesRange1( ctx );

		for(int i = 0; i < max; ++i) {
			sourceOptions.Add( ctx.Tokens );

			// find all explorers/Towns in (sources + target)
			var tokenOptions = sourceOptions
				.SelectMany( tokens =>
					tokens.OfAnyHumanClass( tokenClasses )
					.On( tokens.Space )
				);

			// Select 1 to push
			var toPush = await ctx.Self.Select( new A.SpaceToken( "Select Explorer/Town to push", tokenOptions, Present.Always ) );
			if(toPush == null) break; // nothing to push
			await PushSpecificToken( ctx.Self, toPush );

			// if user selected from Origin space, make that only origin space available.
			if(toPush.Space != ctx.Space) {
				sourceOptions.Clear();
				sourceOptions.Add( toPush.Space.Tokens );
			}
		}
	}

	static HashSet<SpaceState> FindSacredSitesRange1( TargetSpaceCtx ctx ) {
		return ctx.Self.FindSpacesWithinRange( new TargetCriteria( 1 ) )
					.Where( ctx.Self.Presence.IsOn )
					.ToHashSet();
	}

	static async Task PushSpecificToken( Spirit spirit, SpaceToken toPush ) {
		// Push it
		Space destination = await spirit.Select( A.Space.ToPushToken(
			toPush.Token, toPush.Space,
			toPush.Space.Tokens.Adjacent_Existing.Downgrade(),
			Present.Always
		) );
		if(destination != null)
			await toPush.MoveTo( destination );
	}
}