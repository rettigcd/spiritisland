using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	/// <summary> High level Presence Methods for API </summary>
	public class BoundPresence {

		#region constructor

		public BoundPresence(SelfCtx ctx ) { this.ctx = ctx; }
		readonly SelfCtx ctx;

		#endregion

		// Used for Move, Gather, and Push presence
		public void Move( Space from, Space to ) => ctx.Self.Presence.Move(from,to,ctx.GameState);

		public async Task<(Space,Space)> PushUpTo1() {
			// Select source
			var source = await ctx.Decision( Select.DeployedPresence.ToPush( ctx.Self ) );
			if(source == null) return (null,null);
			var sourceCtx = ctx.Target( source );
			// Select destination
			var destination = await sourceCtx.Decision( Select.Space.PushPresence( sourceCtx.Space, sourceCtx.Adjacent, Present.Always ));
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
			Space to = await SelectDestinationWithinRange( range, filterEnum );
			await ctx.Self.PlacePresence( from, to, ctx.GameState );
		}

		/// <summary> Selects: Source then Destination(predetermined) for placing presence.</summary>
		/// <returns>Place in Ocean, Growth through sacrifice</returns>
		public async Task Place( params Space[] destinationOptions ) {
			var from = await SelectSource();
			var to = await ctx.Decision( Select.Space.ToPlacePresence( destinationOptions, Present.Always ) );
			await ctx.Self.PlacePresence( from, to, ctx.GameState );
		}

		#endregion

		#region Destroy 

		public Task Destroy( Space space, ActionType actionType ) => ctx.Self.Presence.Destroy( space, ctx.GameState, actionType );

		public async Task DestoryOne(ActionType actionType) {
			var space = await ctx.Decision( Select.DeployedPresence.ToDestroy("Select presence to destroy",ctx.Self) );
			await Destroy( space, actionType );
		}

		#endregion

		#region Restore Destroyed

		public async Task RestoreUpToNDestroyed( int count ) {
			count = Math.Max(count,ctx.Self.Presence.Destroyed);
			while(count > 0) {
				var dst = await ctx.Decision( Select.TrackSlot.ToCover( ctx.Self ) );
				if(dst == null) break;
				await ctx.Self.Presence.ReturnDestroyedToTrack(dst,ctx.GameState);
				--count;
			}
		}

		#endregion

		/// <remarks>Used for Absorb Presence and Replacing Presence</remarks>
		public Task RemoveFrom( Space space ) => ctx.Self.Presence.RemoveFrom( space, ctx.GameState ); // Generally used for Replacing

		#region select Source

		/// <summary> Tries Presence Tracks first, then fails over to placed-presence on Island </summary>
		public async Task<IOption> SelectSource(string actionPhrase = "place") {
			string prompt = $"Select Presence to {actionPhrase}.";
			return (IOption)await ctx.Decision( Select.TrackSlot.ToReveal( prompt, ctx.Self ) )
				?? (IOption)await ctx.Decision( Select.DeployedPresence.All( prompt, ctx.Self,Present.Always) );
		}

		public Task<Space> SelectDeployed(string prompt)
			=> ctx.Decision( Select.DeployedPresence.All(prompt, ctx.Self,Present.Always ) );

		public Task<Space> SelectSacredSite(string prompt)
			=> ctx.Decision( Select.DeployedPresence.SacredSites(prompt, ctx.Self,Present.Always ) );
		

		#endregion

		#region select Destination

		/// <summary> Selects a space within [range] of current presence </summary>
		public async Task<Space> SelectDestinationWithinRange( int range, string filterEnum ) {
			var options = GetValidDestinationOptionsFromPresence(range,filterEnum, ctx.Self.Presence.Spaces );
			return await ctx.Decision( Select.Space.ToPlacePresence( options, Present.Always ) );
		}

		/// <summary> Select a space withing [range] of specified spaces </summary>
		public IEnumerable<Space> GetValidDestinationOptionsFromPresence( int range, string filterEnum, IEnumerable<Space> source ) {
			return ctx.Self.RangeCalc.GetTargetOptionsFromKnownSource( ctx.Self, ctx.GameState, TargettingFrom.None, source, new TargetCriteria( range, filterEnum) )
				.Where(ctx.Self.Presence.IsValid);
		}

		#endregion

	}

}