using System;
using System.Threading.Tasks;

namespace SpiritIsland {

	/// <summary> High level Presence Methods for API </summary>
	public class BoundPresence {

		#region constructor

		public BoundPresence(SpiritGameStateCtx ctx ) { this.ctx = ctx; }
		readonly SpiritGameStateCtx ctx;

		#endregion

		// Used for Move, Gather, and Push presence
		public void Move( Space from, Space to ) => ctx.Self.Presence.Move(from,to,ctx.GameState);

		public async Task<(Space,Space)> PushUpTo1() {
			// Select source
			var source = await ctx.Self.Action.Decision( Decision.Presence.Deployed.SourceForPushing( ctx.Self ) );
			if(source == null) return (null,null);
			var sourceCtx = ctx.Target( source );
			// Select destination
            var destination = await sourceCtx.Self.Action.Decision( new Decision.Presence.Push( "Push Presence to", sourceCtx.Space, sourceCtx.Adjacent ));
			Move( source, destination );
			return (source, destination);
		}

		#region Place

		// Used for Spirit-Setup 
		public void PlaceOn(Space space) => ctx.Self.Presence.PlaceOn( space, ctx.GameState );

		/// <summary> Selects: (Source then Destination) for placing presence </summary>
		/// <remarks> Called from normal PlacePresence Growth + Gift of Proliferation. </remarks>
		public async Task PlaceWithin( int range, string filterEnum ) {
			var from = await SelectSource();
			Space to = await ctx.SelectSpaceWithinRangeOfCurrentPresence( range, filterEnum );
			await ctx.Self.Presence.PlaceFromTracks( from, to, ctx.GameState );
		}

		/// <summary> Selects: Source then Destination(predetermined) for placing presence.</summary>
		/// <returns>Place in Ocean, Growth through sacrifice</returns>
		public async Task Place( params Space[] destinationOptions ) {
			var from = await SelectSource();
			var to = await ctx.Self.Action.Decision( new Decision.Presence.PlaceOn( ctx.Self, destinationOptions, Present.Always ) );
			await ctx.Self.Presence.PlaceFromTracks( from, to, ctx.GameState );
		}

		#endregion

		#region Destroy 

		public Task Destroy( Space space ) => ctx.Self.Presence.Destroy( space, ctx.GameState );

		public async Task DestoryOne() {
			var space = await ctx.Self.Action.Decision( new Decision.Presence.DeployedToDestory("Select presence to destroy",ctx.Self) );
			await Destroy( space );
		}

		#endregion

		#region Restore Destroyed

		public async Task RestoreUpToNDestroyed( int count ) {
			count = Math.Max(count,ctx.Self.Presence.Destroyed);
			while(count > 0) {
				var dst = await ctx.Self.Action.Decision( new Decision.Presence.ReturnToTrackDestination( ctx.Self ) );
				if(dst == null) break;
				await ctx.Self.Presence.ReturnToTrack(Track.Destroyed,dst,ctx.GameState);
				--count;
			}
		}

		#endregion

		/// <remarks>Used for Absorb Presence and Replacing Presence</remarks>
		public void RemoveFrom( Space space ) => ctx.Self.Presence.RemoveFrom( space, ctx.GameState ); // Generally used for Replacing

		#region select Source

		/// <summary> Tries Presence Tracks first, then fails over to placed-presence on Island </summary>
		public async Task<IOption> SelectSource() {
			return (IOption)await ctx.Self.Action.Decision( new Decision.Presence.SourceFromTrack( ctx.Self ) )
				?? (IOption)await ctx.Self.Action.Decision( Decision.Presence.Deployed.SourceForPlacing( ctx.Self ) );
		}

		/// <summary> Tries Presence Tracks first, then fails over to placed-presence on Island </summary>
		public async Task<Space> SelectDeployed(string prompt) {
			return await ctx.Self.Action.Decision( new Decision.Presence.Deployed(prompt, ctx.Self ) );
		}



		#endregion

	}

}