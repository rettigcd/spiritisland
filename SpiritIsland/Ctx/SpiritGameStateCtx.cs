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
			await TargetSpace( space ).DamageInvaders( damage );
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
			var space = await Self.Action.Decision( new Decision.Presence.Deployed( prompt, Self ) );
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

		#endregion Gather

		#region Place Presence

		/// <summary> Selects: (Source then Destination) for placing presence </summary>
		/// <remarks> Called from normal PlacePresence Growth + Gift of Proliferation. </remarks>
		public async Task PlacePresence( int range, string filterEnum ) {
			var from = await SelectPresenceSource();
			Space to = await SelectPresenceDestination( range, filterEnum );
			await Self.Presence.PlaceFromBoard( from, to, GameState );
		}

		/// <summary> Selects: Source then Destination(predetermined) for placing presence.</summary>
		/// <returns>Place in Ocean, Growth through sacrifice</returns>
		public async Task PlacePresence( params Space[] destinationOptions ) {
			var from = await SelectPresenceSource();
			var to = await Self.Action.Decision( new Decision.Presence.PlaceOn( Self, destinationOptions ) );
			await Self.Presence.PlaceFromBoard( from, to, GameState );
		}

		public Task<Track> SelectPresenceSource() {
			return Self.Action.Decision( new Decision.Presence.ToRemoveFromTrack( Self ) );
		}

		public async Task<Space> SelectPresenceDestination( int range, string filterEnum ) {
			return await Self.Action.Decision( new Decision.Presence.PlaceOn( this, range, filterEnum ) );
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