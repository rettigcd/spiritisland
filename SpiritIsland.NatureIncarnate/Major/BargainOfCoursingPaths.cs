namespace SpiritIsland.NatureIncarnate;

public class BargainOfCoursingPaths {

	public const string Name = "Bargain of Coursing Paths";

	[MajorCard(Name,2,"moon,air,water,earth"),Fast]
	[FromPresence(0,Filter.TwoDahan)]
	[Instructions( "Bargain: 1 presence now and -1 Energy/turn. Now: Mark both target land and another with 2 or more Dahan. Ongoing: After pieces are added or moved into the marked lands: move those pieces directly to any 1 land. -If you have- 3 air,2 water,2 earth: The presence cost comes from your presence tracks." ), Artist( Artists.AgnieszkaDabrowiecka )]
	static public async Task ActAsync(TargetSpaceCtx ctx) {
		// Bargain: 1 presence now and -1 Energy/turn.
		// -If you have- 3 air,2 water,2 earth: The presence cost comes from your presence tracks.
		await ctx.Self.PayPresenceForBargain( "3 air,2 water,2 earth" );

		// and you gain 1 less Energy each turn.
		ctx.Self.Presence.AdjustEnergyTrackDueToBargain(-1);

		// Now: Mark both target land and another with 2 or more Dahan.
		var token = new CoursingPaths(ctx.Self);
		ctx.Space.Init(token,1);
		Space other = await SelectSecondSite( ctx );
		other?.Init(token,1);

	}

	static Task<Space> SelectSecondSite( TargetSpaceCtx ctx ) {
		Space[] options = ActionScope.Current.Spaces.Where( s => s != ctx.Space && 2 <= s.Dahan.CountAll ).ToArray();
		return ctx.Self.SelectAsync( new A.SpaceDecision( "Mark Second Space for Coursing", options, Present.Always ) );
	}

	/// <summary>
	/// For each Action, Moves tokens added to its space to a single other space.
	/// </summary>
	class CoursingPaths( Spirit spirit ) 
		: TokenClassToken(Name,'>', Img.Land_Push_Dahan)
		, IHandleTokenAdded
	{

		const string Name = "Coursing Paths";

		readonly Spirit _spirit = spirit;

		async Task IHandleTokenAdded.HandleTokenAddedAsync( Space to, ITokenAddedArgs args ) {
			// Ongoing: After pieces are added or moved into the marked lands:

			// move those pieces directly to any 1 land.
			Space destination = await GetDestination( args );
			if(destination == to) return;

			ActionScope.Current.Log( new Log.Debug( $"{Name} moving {args.Count} {args.Added} from {args.To.Text} to {destination.Label}" ) );
			await args.Added.MoveAsync(to,destination,args.Count);
		}

		async Task<Space> GetDestination( ITokenAddedArgs args ) {
			ActionScope scope = ActionScope.Current;

			// Check for previously selected
			string key = $"CoursingPath:Move {args.To.Text} to";
			if(scope.ContainsKey(key)) return (Space)scope[key];

			// Pick brand new
			Space destination = await _spirit.SelectAsync( new A.SpaceDecision( 
				$"{Name}: Move {args.Count}{args.Added} from {args.To.Text} to:",
				ActionScope.Current.Spaces,
				Present.Always 
			) );
			scope[key] = destination;
			return destination;
		}
	}

}
