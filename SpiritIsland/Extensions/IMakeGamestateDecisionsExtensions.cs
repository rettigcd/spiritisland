using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	// High-Level game actions
	public static class IMakeGamestateDecisionsExtensions {

		static public IMakeGamestateDecisions MakeDecisionsFor(this Spirit spirit, GameState gameState ) 
			=> new PowerCtx( spirit, gameState );

		#region IMakeGamestateDecision class

		class GsDecisionMaker : IMakeGamestateDecisions {
			public GsDecisionMaker(Spirit spirit,GameState gs ) { this.Self = spirit; GameState = gs; }
			public Spirit Self { get; }

			public GameState GameState { get; }
		}

		#endregion

		#region Gather

		static public async Task GatherUpToNTokens( this IMakeGamestateDecisions ctx, Space target, int countToGather, params TokenGroup[] groups ) {
			Token[] calcTokens(Space space) => ctx.GameState.Tokens[space].OfAnyType(groups);
			Space[] CalcSource() => target.Adjacent
				.Where(s=>calcTokens(s).Any())
				.ToArray();

			string label = groups.Select(it=>it.Label).Join("/");

			Space[] neighborsWithItems = CalcSource();
  			int gathered = 0;
			while(gathered<countToGather && neighborsWithItems.Length>0){
				var source = await ctx.Self.Action.Decide( new GatherTokensFromDecision( countToGather-gathered, groups, target, neighborsWithItems, Present.Done ));
				if(source == null) break;

				var invader = await ctx.Self.Action.Decide( new SelectTokenToGatherDecision( source, target, calcTokens(source), Present.IfMoreThan1 ) );

				await ctx.GameState.Move(invader, source, target);

				++gathered;
				neighborsWithItems = CalcSource();
			}

		}

		#endregion Gather

		#region Push

		static public Task<Space[]> PushNTokens( this IMakeGamestateDecisions ctx, Space source, int countToPush , params TokenGroup[] groups )
			=> new TokenPusher( ctx, source ).AddGroup(countToPush,groups).MoveN();

		static public Task<Space[]> PushUpToNTokens( this IMakeGamestateDecisions ctx, Space source, int countToPush , params TokenGroup[] groups ) 
			=> new TokenPusher( ctx, source ).AddGroup(countToPush,groups).MoveUpToN();

		#endregion Push

		#region Place Presence

		static public Task PlacePresence( this IMakeGamestateDecisions engine, int range, string filterEnum ) {
			Space[] destinationOptions = Presence_DestinationOptions( engine, range, filterEnum );
			return engine.Presence_SelectFromTo( destinationOptions );
		}

		static public async Task Presence_SelectFromTo( this IMakeGamestateDecisions engine, params Space[] destinationOptions ) {
			var from = await engine.Self.SelectTrack();
			var to = await engine.Self.Action.Decide( new TargetSpaceDecision( "Where would you like to place your presence?", destinationOptions, Present.Always ));
			await engine.Self.Presence.PlaceFromBoard(from, to, engine.GameState );
		}

		static public Space[] Presence_DestinationOptions( this IMakeGamestateDecisions engine, int range, string filterEnum ) {
			// Calculate options
			var existing = engine.Self.Presence.Spaces.ToArray();

			var inRange = existing
				.SelectMany( s => s.Range( range ) )
				.Distinct()
				.ToArray();

			Space[] destinationOptions = inRange
				.Where( SpaceFilter.Normal.GetFilter( engine.Self, engine.GameState, filterEnum ) )
				.OrderBy( x => x.Label )
				.ToArray();
			return destinationOptions.Length == 0
				? throw new System.Exception( "dude you don't have anywhere to place your presence" )
				: destinationOptions;
		}

		#endregion Place Presence

		#region Damage Invaders

		static public InvaderGroup InvadersOn( this IMakeGamestateDecisions engine, Space space )
			=> engine.Self.BuildInvaderGroupForPowers( engine.GameState, space );

		static public async Task DamageInvaders( this IMakeGamestateDecisions engine, Space space, int damage ) {
			if(damage == 0) return;
			await engine.InvadersOn( space ).ApplySmartDamageToGroup( damage );
		}

		#endregion

		// convenience for ctx so we don't have to do ctx.Self.SelectPowerOption(...)
		static public Task SelectActionOption( this IMakeGamestateDecisions eng, params ActionOption[] options )
			=> eng.Self.SelectAction( "Select Power Option", options );

		static public async Task SelectAction( this Spirit spirit, string prompt, params ActionOption[] options ) {
			var applicable = options.Where( opt => opt.IsApplicable ).ToArray();
			string text = await spirit.SelectText( prompt, applicable.Select( a => a.Description ).ToArray() );
			if(text != null) {
				var selectedOption = applicable.Single( a => a.Description == text );
				await selectedOption.Action();
			}
		}

		static public async Task SelectOptionalAction( this Spirit spirit, string prompt, params ActionOption[] options ) {
			var applicable = options.Where( opt => opt.IsApplicable ).ToArray();
			string text = await spirit.SelectText( prompt, applicable.Select( a => a.Description ).ToArray(), Present.Done );
			if(text != null) {
				var selectedOption = applicable.Single( a => a.Description == text );
				await selectedOption.Action();
			}
		}


		/// <summary>
		/// Used for Power-targetting, where range sympols appear.
		/// </summary>
		static public Task<Space> PowerTargetsSpace( this IMakeGamestateDecisions engine, From sourceEnum, Terrain? sourceTerrain, int range, string filterEnum )
			=> engine.Self.PowerApi.TargetsSpace( engine.Self, engine.GameState, sourceEnum, sourceTerrain, range, filterEnum );

	}

}