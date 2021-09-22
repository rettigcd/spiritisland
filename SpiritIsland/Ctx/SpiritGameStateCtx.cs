using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class SpiritGameStateCtx {

		public Spirit Self { get; }
		public GameState GameState { get; }
		public Cause Cause { get; }

		#region constructor

		public SpiritGameStateCtx(Spirit self,GameState gameState, Cause cause) {
			Self = self;
			GameState = gameState;
			Cause = cause;
		}

		#endregion constructor

		#region GameState only / Non-spirit parts

		public IEnumerable<Space> AdjacentTo( Space source )
			=> source.Adjacent.Where( x => this.SpaceFilter.TerrainMapper( x ) != Terrain.Ocean );

		public bool IsCostal( Space space ) => this.SpaceFilter.IsCoastal( space );

		public async Task DamageInvaders( Space space, int damage ) {
			if(damage == 0) return;
			await InvadersOn( space ).SmartDamageToGroup( damage );
		}

		protected virtual SpaceFilter SpaceFilter => Cause switch {
			Cause.Power => SpaceFilter.ForPowers,
			_ => SpaceFilter.Normal
		};

		#endregion

		public virtual InvaderGroup InvadersOn( Space target )
			=> Cause switch {
				Cause.Power => Self.BuildInvaderGroupForPowers(GameState, target),
				_ => GameState.Invaders.On( target, Cause )
			};

		public virtual void AddFear( int count ) { // need space so we can track fear-space association for bringer
			GameState.Fear.AddDirect( new FearArgs { 
				count = count, 
				cause = Cause, 
				space = null 
			} );
		}

		/// <summary>
		/// Used for Power-targetting, where range sympols appear.
		/// </summary>
		public async Task<TargetSpaceCtx> TargetsSpace( From sourceEnum, Terrain? sourceTerrain, int range, string filterEnum ) {
			var space = await Self.PowerApi.TargetsSpace( Self, GameState, sourceEnum, sourceTerrain, range, filterEnum );
			return new TargetSpaceCtx( Self, GameState, space, Cause );
		}

		public TargetSpaceCtx TargetSpace( Space space ) => new TargetSpaceCtx( Self, GameState, space, Cause );

		public async Task<TargetSpaceCtx> TargetLandWithPresence( string prompt ) {
			var space = await Self.Action.Decision( new Decision.PresenceDeployed( prompt, Self ) );
			return new TargetSpaceCtx( Self, GameState, space, Cause );
		}

		#region Push

		public Task<Space[]> Push( Space source, int countToPush, params TokenGroup[] groups )
			=> new TokenPusher( this, source ).AddGroup( countToPush, groups ).MoveN();

		public Task<Space[]> PushUpTo( Space source, int countToPush, params TokenGroup[] groups )
			=> new TokenPusher( this, source ).AddGroup( countToPush, groups ).MoveUpToN();

		#endregion Push

		#region Gather

		public async Task GatherUpTo( Space target, int countToGather, params TokenGroup[] groups ) {
			SpaceToken[] GetOptions() => target.Adjacent
				.SelectMany(a=>GameState.Tokens[a].OfAnyType(groups).Select(t=>new SpaceToken(a,t)))
				.ToArray();

			SpaceToken[] options;
			while( 0 < countToGather
				&& (options=GetOptions()).Length>0
			) {
				var source = await Self.Action.Decision( new Decision.AdjacentSpaceTokensToGathers(countToGather, target, options, Present.Done ));
				if(source == null) break;
				await GameState.Move( source.Token, source.Space, target );
				--countToGather;
			}
		}

		public async Task Gather( Space target, int countToGather, params TokenGroup[] groups ) {
			SpaceToken[] GetOptions() => target.Adjacent
				.SelectMany(a=>GameState.Tokens[a].OfAnyType(groups).Select(t=>new SpaceToken(a,t)))
				.ToArray();

			SpaceToken[] options;
			while( 0 < countToGather
				&& (options=GetOptions()).Length>0
			) {
				var source = await Self.Action.Decision( new Decision.AdjacentSpaceTokensToGathers(countToGather, target, options, Present.Always ));
				if(source == null) break;
				await GameState.Move( source.Token, source.Space, target );
				--countToGather;
			}
		}


		//public async Task OldGather( Space target, int countToGather, params TokenGroup[] groups ) {
		//	Token[] calcTokens( Space space ) => GameState.Tokens[space].OfAnyType( groups );
		//	Space[] CalcSource() => AdjacentTo( target )
		//		.Where( s => calcTokens( s ).Any() )
		//		.ToArray();

		//	string label = groups.Select( it => it.Label ).Join( "/" );

		//	Space[] neighborsWithItems = CalcSource();
		//	int gathered = 0;
		//	while(gathered < countToGather && neighborsWithItems.Length > 0) {
		//		var source = await Self.Action.Decision( new Decision.AdjacentSpaceWithTokensToGathers( countToGather - gathered, groups, target, neighborsWithItems, Present.Always ) );
		//		if(source == null) break;

		//		var invader = await Self.Action.Decision( new Decision.TokenToGather( source, target, calcTokens( source ), Present.IfMoreThan1 ) );

		//		await GameState.Move( invader, source, target );

		//		++gathered;
		//		neighborsWithItems = CalcSource();
		//	}

		//}


		#endregion Gather

		#region Place Presence

		public Task PlacePresence( int range, string filterEnum ) {
			Space[] destinationOptions = Presence_DestinationOptions( range, filterEnum );
			return Presence_SelectFromTo( destinationOptions );
		}

		public async Task Presence_SelectFromTo( params Space[] destinationOptions ) {
			var from = await Self.SelectTrack();
			var to = await Self.Action.Decision( new Decision.TargetSpace( "Where would you like to place your presence?", destinationOptions, Present.Always ) );
			await Self.Presence.PlaceFromBoard( from, to, GameState );
		}

		public Space[] Presence_DestinationOptions( int range, string filterEnum ) {
			// Calculate options
			var existing = Self.Presence.Spaces.ToArray();

			var inRange = existing
				.SelectMany( s => s.Range( range ) )
				.Distinct()
				.ToArray();

			Space[] destinationOptions = inRange
				.Where( SpaceFilter.Normal.GetFilter( Self, GameState, filterEnum ) )
				.OrderBy( x => x.Label )
				.ToArray();
			return destinationOptions.Length == 0
				? throw new System.Exception( "dude you don't have anywhere to place your presence" )
				: destinationOptions;
		}

		#endregion Place Presence

		#region Select Action

		// convenience for ctx so we don't have to do ctx.Self.SelectPowerOption(...)
		public Task SelectActionOption( params ActionOption[] options )
			=> Self.SelectAction( "Select Power Option", options );

		#endregion

		public bool YouHave( string elementString ) => Self.Elements.Contains( elementString );

		public async Task<TargetSpaceCtx> SelectSpace( string prompt, IEnumerable<Space> options ) {
			var space = await Self.Action.Decision( new Decision.TargetSpace( prompt, options, Present.Always ) );
			return space != null
				? new TargetSpaceCtx( Self, GameState, space, Cause )
				: null;
		}


		#region used in Fear

		public async Task RemoveHealthFromOne( int healthToRemove, IEnumerable<Space> options ) {
			var space = await SelectSpace( $"remove {healthToRemove} invader health from", options );
			space.Invaders.SmartRemovalOfHealth( healthToRemove );
		}

		public async Task<Space> RemoveTokenFromOne( IEnumerable<Space> spaceOptions, int count, params TokenGroup[] removable ) {
			var space = await SelectSpace( "Remove invader from", spaceOptions );
			if(space != null)
				while(count-->0)
					space.Tokens.RemoveInvader( removable );
			return space?.Space;
		}

		public async Task GatherExplorerToOne( IEnumerable<Space> spaceOptions, int count, params TokenGroup[] typeToGather ) {
			var spaceCtx = await SelectSpace( "Gather Invader to", spaceOptions );
			if(spaceCtx != null)
				await spaceCtx.GatherUpTo(count,typeToGather);
		}


		#endregion

	}

}