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
		ctx.Tokens.Init(token,1);
		Space other = await SelectSecondSite( ctx );
		other?.ScopeTokens.Init(token,1);

	}

	static Task<Space> SelectSecondSite( TargetSpaceCtx ctx ) {
		SpaceState[] options = ActionScope.Current.Tokens.Where( s => s != ctx.Tokens && 2 <= s.Dahan.CountAll ).ToArray();
		Task<Space> other = ctx.Self.SelectAsync( new A.Space( "Mark Second Space for Coursing", options, Present.Always ) );
		return other;
	}

	/// <summary>
	/// For each Action, Moves tokens added to its space to a single other space.
	/// </summary>
	class CoursingPaths( Spirit spirit ) 
		: TokenClassToken(Name,'>', Img.Land_Push_Dahan)
		, IHandleTokenAddedAsync
	{

		const string Name = "Coursing Paths";

		readonly Spirit _spirit = spirit;

		async Task IHandleTokenAddedAsync.HandleTokenAddedAsync( SpaceState to, ITokenAddedArgs args ) {
			// Ongoing: After pieces are added or moved into the marked lands:

			// move those pieces directly to any 1 land.
			SpaceState destination = await GetDestination( args );
			if(destination == to) return;

			ActionScope.Current.Log( new Log.Debug( $"{Name} moving {args.Count} {args.Added} from {((Space)args.To).Text} to {destination.Space.Text}" ) );
			await args.Added.MoveAsync(to,destination,args.Count);
		}

		async Task<SpaceState> GetDestination( ITokenAddedArgs args ) {
			ActionScope scope = ActionScope.Current;

			// Check for previously selected
			string key = $"CoursingPath:Move {((Space)args.To).Text} to";
			if(scope.ContainsKey(key)) return (SpaceState)scope[key];

			// Pick brand new
			SpaceState destination = (await _spirit.SelectAsync( new A.Space( 
				$"{Name}: Move {args.Count}{args.Added} from {((Space)args.To).Text} to:",
				ActionScope.Current.Tokens,
				Present.Always 
			) )).ScopeTokens;
			scope[key] = destination;
			return destination;
		}
	}

}
