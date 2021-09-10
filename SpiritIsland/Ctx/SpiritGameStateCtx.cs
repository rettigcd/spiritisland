﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class SpiritGameStateCtx : IMakeGamestateDecisions {

		public Spirit Self { get; }
		public GameState GameState { get; }

		public SpiritGameStateCtx(Spirit self,GameState gameState ) {
			this.Self = self;
			this.GameState = gameState;
		}

		public virtual IEnumerable<Space> AdjacentTo( Space source )
			=> source.Adjacent.Where( x => SpaceFilter.Normal.TerrainMapper( x ) != Terrain.Ocean );

		#region Push

		public Task<Space[]> PushNTokens( Space source, int countToPush, params TokenGroup[] groups )
			=> new TokenPusher( this, source ).AddGroup( countToPush, groups ).MoveN();

		public Task<Space[]> PushUpToNTokens( Space source, int countToPush, params TokenGroup[] groups )
			=> new TokenPusher( this, source ).AddGroup( countToPush, groups ).MoveUpToN();

		#endregion Push

		#region Gather

		public async Task GatherUpToNTokens( Space target, int countToGather, params TokenGroup[] groups ) {
			Token[] calcTokens( Space space ) => GameState.Tokens[space].OfAnyType( groups );
			Space[] CalcSource() => target.Adjacent
				.Where( s => calcTokens( s ).Any() )
				.ToArray();

			string label = groups.Select( it => it.Label ).Join( "/" );

			Space[] neighborsWithItems = CalcSource();
			int gathered = 0;
			while(gathered < countToGather && neighborsWithItems.Length > 0) {
				var source = await Self.Action.Decide( new GatherTokensFromDecision( countToGather - gathered, groups, target, neighborsWithItems, Present.Done ) );
				if(source == null) break;

				var invader = await Self.Action.Decide( new SelectTokenToGatherDecision( source, target, calcTokens( source ), Present.IfMoreThan1 ) );

				await GameState.Move( invader, source, target );

				++gathered;
				neighborsWithItems = CalcSource();
			}

		}

		#endregion Gather

		#region Place Presence

		public Task PlacePresence( int range, string filterEnum ) {
			Space[] destinationOptions = Presence_DestinationOptions( range, filterEnum );
			return Presence_SelectFromTo( destinationOptions );
		}

		public async Task Presence_SelectFromTo( params Space[] destinationOptions ) {
			var from = await Self.SelectTrack();
			var to = await Self.Action.Decide( new TargetSpaceDecision( "Where would you like to place your presence?", destinationOptions, Present.Always ) );
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

		public virtual void AddFear( int count ) { // need space so we can track fear-space association for bringer
			GameState.Fear.AddDirect( new FearArgs { count = count, cause = Cause.None, space = null } );
		}

	}

}