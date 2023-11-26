namespace SpiritIsland.NatureIncarnate;

#nullable enable

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
		// If we want to use SpaceState.Pusher, we need to do them 1 at a time, not together.
		SpaceState? selectedOrigin = null;
		await new TokenMover( ctx.Self, "Push"
			, new SourceSelector( FindOrigins( ctx ).Append( ctx.Tokens ).Distinct() )
			, DestinationSelector.Adjacent
		)
			.AddGroup( max, tokenClasses )
			.Track( move => { if(move.From != ctx.Tokens) selectedOrigin = move.From; } )
			.FilterDestination( s => s == ctx.Space || selectedOrigin == null || selectedOrigin == s )
			.DoN();


	}

	static IEnumerable<SpaceState> FindOrigins( TargetSpaceCtx ctx )
		=> ctx.Self
			.FindTargettingSourcesFor(
				ctx.Space,
				new TargetingSourceCriteria( From.SacredSite ),
				new TargetCriteria( 1 )
			);
}