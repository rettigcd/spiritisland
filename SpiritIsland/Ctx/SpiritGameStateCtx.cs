using System;
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
			TerrainMapper = TerrainMapper.For(cause); // !! could make TerrainMapper a property on Cause so we don't have to look it up.
		}

		protected SpiritGameStateCtx(SpiritGameStateCtx src) {
			Self = src.Self;
			GameState = src.GameState;
			Cause = src.Cause;
			TerrainMapper = src.TerrainMapper;
		}

		#endregion constructor

		#region convenience Read-Only methods

		public IEnumerable<Space> AllSpaces => GameState.Island.AllSpaces;

		public Task Move(Token token, Space from, Space to, int count = 1 )
			=> GameState.Tokens.Move( token, from, to, count );

		public bool YouHave( string elementString ) => Self.Elements.Contains( elementString );

		#endregion

		public virtual void AddFear( int count ) { // overriden by TargetSpaceCtx to add the location
			GameState.Fear.AddDirect( new FearArgs { 
				count = count, 
				cause = Cause, 
				space = null 
			} );
		}

		/// <summary>
		/// Used for Power-targetting, where range sympols appear.
		/// </summary>
		public async Task<TargetSpaceCtx> SelectTargetSpace( From sourceEnum, Terrain? sourceTerrain, int range, string filterEnum ) {
			var space = await Self.PowerApi.TargetsSpace( Self, GameState, sourceEnum, sourceTerrain, range, filterEnum );
			return new TargetSpaceCtx( this, space );
		}

		public TargetSpaceCtx Target( Space space ) => new TargetSpaceCtx( this, space );
		public TargetSpaceCtx TargetSpace( string spaceLabel ) => new TargetSpaceCtx( this, GameState.Island.AllSpaces.First(s=>s.Label==spaceLabel) );

		public async Task<TargetSpaceCtx> TargetLandWithPresence( string prompt ) {
			var space = await Self.Action.Decision( new Decision.Presence.Deployed( prompt, Self ) );
			return new TargetSpaceCtx( this, space );
		}

		#region Draw Cards

		public Task<PowerCard> Draw( Func<List<PowerCard>, Task> handleNotUsed ) => Self.Draw( GameState, handleNotUsed );
		public Task<PowerCard> DrawMinor() => Self.DrawMinor( GameState );
		public Task<PowerCard> DrawMajor( int numberToDraw = 4 ) => Self.CardDrawer.DrawMajor( Self, GameState, null, numberToDraw );


		#endregion

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
				await Move( source.Token, source.Space, target );
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
				await Move( source.Token, source.Space, target );
				--countToGather;
			}
		}

		#endregion Gather

		#region Place Presence

		/// <summary> Selects: (Source then Destination) for placing presence </summary>
		/// <remarks> Called from normal PlacePresence Growth + Gift of Proliferation. </remarks>
		public async Task PlacePresence( int range, string filterEnum ) {
			var from = await SelectPresenceSource();
			Space to = await SelectSpaceWithinRangeOfCurrentPresence( range, filterEnum );
			await Self.Presence.PlaceFromBoard( from, to, GameState );
		}

		/// <summary> Selects: Source then Destination(predetermined) for placing presence.</summary>
		/// <returns>Place in Ocean, Growth through sacrifice</returns>
		public async Task PlacePresence( params Space[] destinationOptions ) {
			var from = await SelectPresenceSource();
			var to = await Self.Action.Decision( new Decision.Presence.PlaceOn( Self, destinationOptions ) );
			await Self.Presence.PlaceFromBoard( from, to, GameState );
		}

		public async Task<IOption> SelectPresenceSource() {
			return (IOption)await Self.Action.Decision( new Decision.Presence.Source( Self ) )
				?? (IOption)await Self.Action.Decision( new Decision.Presence.TakeFromBoard( Self ) );
		}

		/// <summary>
		/// Selects a space within [range] of current presence
		/// </summary>
		public async Task<Space> SelectSpaceWithinRangeOfCurrentPresence( int range, string filterEnum ) {
			return await Self.Action.Decision( new Decision.Presence.PlaceOn( this, range, filterEnum ) );
		}

		public IEnumerable<Space> FindSpacesWithinRangeOf( IEnumerable<Space> source, int range, string filterEnum ) {
			return Self.PowerApi.GetTargetOptions( Self, GameState, source, range, filterEnum );
		}

		#endregion Place Presence

		#region Generic Select space / option

		public async Task<TargetSpaceCtx> SelectSpace( string prompt, IEnumerable<Space> options ) {
			var space = await Self.Action.Decision( new Decision.TargetSpace( prompt, options, Present.Always ) );
			return space != null
				? new TargetSpaceCtx( this, space )
				: null;
		}

		// convenience for ctx so we don't have to do ctx.Self.SelectPowerOption(...)
		public Task SelectActionOption( params ActionOption[] options )
			=> Self.SelectAction( "Select Power Option", options );

		#endregion

		protected readonly TerrainMapper TerrainMapper;

		#region High level fear-specific decisions

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