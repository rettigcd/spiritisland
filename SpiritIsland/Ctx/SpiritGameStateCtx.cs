﻿using System;
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

		protected SpiritGameStateCtx(SpiritGameStateCtx src) {
			Self = src.Self;
			GameState = src.GameState;
			Cause = src.Cause;
			_terrainMapper = src._terrainMapper;
		}

		#endregion constructor

		#region Presence

		public virtual BoundPresence Presence => _presence ??= new BoundPresence(this);
		BoundPresence _presence;

		#endregion

		#region convenience Read-Only methods

		public IEnumerable<Space> AllSpaces => GameState.Island.AllSpaces;

		public Task Move(Token token, Space from, Space to )
			=> GameState.Tokens.Move( token, from, to );

		public Task<bool> YouHave( string elementString ) => Self.HasElements( ElementList.Parse(elementString) );

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
		public async Task<TargetSpaceCtx> SelectTargetSpace( string prompt, From sourceEnum, Terrain? sourceTerrain, int range, string filterEnum ) {
			var space = await Self.TargetLandApi.TargetsSpace( Self, GameState, prompt, sourceEnum, sourceTerrain, range, filterEnum );
			return new TargetSpaceCtx( this, space );
		}

		public TargetSpaceCtx Target( Space space ) => new TargetSpaceCtx( this, space );
		public TargetSpaceCtx TargetSpace( string spaceLabel ) => new TargetSpaceCtx( this, GameState.Island.AllSpaces.First(s=>s.Label==spaceLabel) );

		// Visually, selects the [presence] icon
		public async Task<TargetSpaceCtx> TargetDeployedPresence( string prompt ) {
			var space = await Self.Action.Decision( new Decision.Presence.Deployed( prompt, Self ) );
			return new TargetSpaceCtx( this, space );
		}

		// Visually, selects the [space] which has presence.
		public async Task<TargetSpaceCtx> TargetLandWithPresence( string prompt ) {
			var space = await Self.Action.Decision( new Decision.TargetSpace(prompt,Self.Presence.Spaces, Present.Always ) );
			return new TargetSpaceCtx( this, space );
		}

		#region Draw Cards

		public Task<PowerCard> Draw( Func<List<PowerCard>, Task> handleNotUsed = null ) => Self.Draw( GameState, handleNotUsed );
		public Task<PowerCard> DrawMinor() => Self.DrawMinor( GameState );
		public Task<PowerCard> DrawMajor( int numberToDraw = 4, bool forgetCard=true ) => Self.DrawMajor( GameState, numberToDraw, forgetCard );


		#endregion

		#region Place Presence

		/// <summary>
		/// Selects a space within [range] of current presence
		/// </summary>
		public async Task<Space> SelectSpaceWithinRangeOfCurrentPresence( int range, string filterEnum ) {
			return await Self.Action.Decision( new Decision.Presence.PlaceOn( this, range, filterEnum ) );
		}

		public IEnumerable<Space> FindSpacesWithinRangeOf( IEnumerable<Space> source, int range, string filterEnum ) {
			return Self.TargetLandApi.GetTargetOptions( Self, GameState, source, range, filterEnum );
		}

		#endregion Place Presence

		#region Generic Select space / option

		public async Task<TargetSpaceCtx> SelectSpace( string prompt, IEnumerable<Space> options ) {
			var space = await Self.Action.Decision( new Decision.TargetSpace( prompt, options, Present.Always ) );
			return space != null
				? new TargetSpaceCtx( this, space )
				: null;
		}

		public Task SelectActionOption( params ActionOption[] options )
			=> SelectActionOption( "Select Power Option", options );

		// overriden by Grinning Trickster's Lets See What Happens
		public virtual async Task SelectActionOption( string prompt, params ActionOption[] options ) {
			ActionOption[] applicable = options.Where( opt => opt.IsApplicable ).ToArray();
			string text = await Self.SelectText( prompt, applicable.Select( a => a.Description ).ToArray(), Present.AutoSelectSingle );
			if(text != null) {
				var selectedOption = applicable.Single( a => a.Description == text );
				await selectedOption.Action();
			}
		}

		public async Task SelectOptionalAction( string prompt, params ActionOption[] options ) {
			var applicable = options.Where( opt => opt.IsApplicable ).ToArray();
			string text = await Self.SelectText( prompt, applicable.Select( a => a.Description ).ToArray(), Present.Done );
			if( text != null && text != TextOption.Done.Text ) {
				var selectedOption = applicable.Single( a => a.Description == text );
				await selectedOption.Action();
			}
		}

		#endregion

		// Defer initializing this because some tests don't initialize nor depend on the GameState
		protected TerrainMapper TerrainMapper => _terrainMapper ??= GameState.Island.TerrainMapFor(Cause);
		protected TerrainMapper _terrainMapper;


		#region High level fear-specific decisions

		public async Task RemoveHealthFromOne( int healthToRemove, IEnumerable<Space> options ) {
			var spaceCtx = await SelectSpace( $"remove {healthToRemove} invader health from", options );
			await spaceCtx.RemoveHealthWorthOfInvaders( healthToRemove );
		}

		public async Task<Space> RemoveTokenFromOneSpace( IEnumerable<Space> spaceOptions, int count, params TokenGroup[] removables ) {
			var spaceCtx = await SelectSpace( "Remove invader from", spaceOptions );
			if(spaceCtx != null)
				while(count-->0)
					spaceCtx.Invaders.Remove( removables );
			return spaceCtx?.Space;
		}

		public async Task GatherExplorerToOne( IEnumerable<Space> spaceOptions, int count, params TokenGroup[] typeToGather ) {
			var spaceCtx = await SelectSpace( "Gather Invader to", spaceOptions );
			if(spaceCtx != null)
				await spaceCtx.GatherUpTo(count,typeToGather);
		}


		#endregion

	}


}