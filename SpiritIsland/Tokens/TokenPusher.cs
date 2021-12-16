using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class TokenPusher {

		public TokenPusher( TargetSpaceCtx ctx ) {
			this.ctx = ctx;
			this.source = ctx.Space;
		}

		public TokenPusher AddGroup(int count,params TokenGroup[] groups ) {

			count = System.Math.Min( count, ctx.GameState.Tokens[source].SumAny(groups) );

			int index = countArray.Count;
			countArray.Add( count );
			foreach(var group in groups) 
				indexLookupByGroup.Add( group, index );

			return this; // chain together
		}

		public Task<Space[]> MoveN() => Exec( Present.Always );
		public Task<Space[]> MoveUpToN() => Exec( Present.Done );

		async Task<Space[]> Exec( Present present ) {

			var counts = ctx.Target(source).Tokens;
			Token[] GetTokens() {
				var groupsWithRemainingCounts = indexLookupByGroup
					.Where( pair => countArray[pair.Value] > 0 )
					.Select( p => p.Key )
					.ToArray();
				return counts.OfAnyType( groupsWithRemainingCounts ); // !!! Make Dahan Freezable
			}

			var pushedToSpaces = new List<Space>();

			Token[] tokens;
			while(0 < (tokens = GetTokens()).Length) {
				// Select Token
				var decision = Select.TokenFrom1Space.TokenToPush( source, countArray.Sum(), tokens, present );
				var token = await ctx.Self.Action.Decision( decision );
				if(token == null) break;

				// Push to Destination
				Space destination = await PushToken( token );

				// Book keeping
				--countArray[indexLookupByGroup[token.Generic]]; // decrement count
				if(destination != null)
					pushedToSpaces.Add( destination ); // record push
			}
			return pushedToSpaces.ToArray();
		}

		public async Task<Space> PushToken( Token token ) {
			Space destination = await SelectDestination( token );
			if(destination == null) return null;

			await ctx.Move( token, source, destination );	// !!! if moving into frozen land, freeze Dahan

			customeAction?.Invoke( token, source, destination);

			return destination;
		}

		public TokenPusher AddCustomMoveAction( Func<Token,Space,Space,Task> customeAction ) {
			this.customeAction = customeAction;
			return this;
		}
		Func<Token,Space,Space,Task> customeAction;

		protected virtual async Task<Space> SelectDestination( Token token ) {
			IEnumerable<Space> destinationOptions = source.Adjacent.Where( s => ctx.Target(s).IsInPlay );
			foreach(var filter in destinationFilters)
				destinationOptions = destinationOptions.Where(filter);

			return await ctx.Decision( Select.Space.PushToken( token, source, destinationOptions, Present.Always ) );
		}

		public TokenPusher FilterDestinations(Func<Space,bool> destinationFilter ) {
			destinationFilters.Add(destinationFilter);
			return this;
		}

		#region private

		protected readonly SelfCtx ctx;
		protected readonly Space source;
		protected readonly List<Func<Space,bool>> destinationFilters = new List<Func<Space, bool>>();

		readonly List<int> countArray = new(); // the # we push from each group
		readonly Dictionary<TokenGroup, int> indexLookupByGroup = new(); // map from group back to its count

		#endregion

	}

	/// <summary>
	/// Overrides Selecting destination with a fixed destination
	/// </summary>
	public class TokenPusher_FixedDestination : TokenPusher {
		readonly Space destination;
		public TokenPusher_FixedDestination( TargetSpaceCtx ctx, Space destination ) : base( ctx ) { 
			this.destination = destination;
		}

		protected override Task<Space> SelectDestination( Token token ) {
			return Task.FromResult(destination);
		}

	}

}