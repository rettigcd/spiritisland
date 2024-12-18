namespace SpiritIsland.NatureIncarnate;

#nullable enable

public class UnbearableGaze {

	const string Name = "Unbearable Gaze";

	[SpiritCard(Name,1,Element.Sun,Element.Fire),Slow,FromSacredSite(1)]
	[Instructions( "1 Fear. Push 2 Explorer/Town from origin or target land  (or 1 Explorer/Town from each)." ), Artist( Artists.AgnieszkaDabrowiecka )]
	static public async Task ActAsync(TargetSpaceCtx ctx ) {
		// 1 Fear.
		await ctx.AddFear( 1 );

		// Push 2 Explorer/Town from origin or target land  (or 1 Explorer/Town from each).
		await PushFromTargetOrOrigin( ctx, 2, Human.Explorer_Town );
	}

	static async Task PushFromTargetOrOrigin( TargetSpaceCtx ctx, int max, params HumanTokenClass[] tokenClasses ) {

		await new SourceSelector(TargetSpaceAttribute.TargettedSpace.Sources.Append( ctx.Space ).Distinct() )
			.AddGroup( max, tokenClasses )
			.Config( FromTargetOrOrigin(ctx.Space) )
			.PushN( ctx.Self );
	}

	static Action<SourceSelector> FromTargetOrOrigin( Space targetSpace ) {
		return (ss) => {
			Space? selectedOrigin = null;
			ss.Track( from => { if(from.Space != targetSpace) selectedOrigin = from.Space; } );
			ss.FilterSource( ss => ss == targetSpace || selectedOrigin is null || selectedOrigin == ss );
		};
	}

}