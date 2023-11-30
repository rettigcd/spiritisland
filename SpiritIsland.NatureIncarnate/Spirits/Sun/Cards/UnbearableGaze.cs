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

		await new SourceSelector( FindOrigins( ctx ).Append( ctx.Tokens ).Distinct() )
			.AddGroup( max, tokenClasses )
			.Config( FromTargetOrOrigin(ctx.Space) )
			.PushN( ctx.Self );
	}

	static IEnumerable<SpaceState> FindOrigins( TargetSpaceCtx ctx )
		=> ctx.Self
			.FindTargettingSourcesFor(
				ctx.Space,
				new TargetingSourceCriteria( TargetFrom.SacredSite ),
				new TargetCriteria( 1 )
			);

	static Action<SourceSelector> FromTargetOrOrigin( Space target ) {
		return (ss) => {
			Space? selectedOrigin = null;
			ss.Track( from => { if(from.Space != target) selectedOrigin = from.Space; } );
			ss.FilterSource( ss => ss == target || selectedOrigin == null || selectedOrigin == ss.Space );
		};
	}

}