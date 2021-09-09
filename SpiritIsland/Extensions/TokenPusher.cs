using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class TokenPusher {

		public TokenPusher( IMakeGamestateDecisions ctx, Space source ) {
			this.ctx = ctx;
			this.source = source;
		}

		public TokenPusher ForPowerOrBlight() {
			spaceFilter = SpaceFilter.ForPowers;
			return this;
		}

		public TokenPusher AddGroup(int count,params TokenGroup[] groups ) {

			count = System.Math.Min( count, ctx.GameState.Tokens[source].SumAny(groups) );

			int index = countArray.Count;
			countArray.Add( count );
			foreach(var group in groups) indexLookup.Add( group, index );

			return this; // chain together
		}

		public Task<Space[]> MoveN() => Exec( Present.IfMoreThan1 ); // ? switch to .Always ?
		public Task<Space[]> MoveUpToN() => Exec( Present.Done );

		async Task<Space[]> Exec( Present present ) {

			var counts = ctx.GameState.Tokens[source];
			Token[] GetTokens() => counts.OfAnyType( indexLookup.Where( pair=>countArray[pair.Value]>0).Select(p=>p.Key).ToArray() );

			var pushedToSpaces = new List<Space>();

			Token[] tokens;
			while(0 < (tokens = GetTokens()).Length) {
				var decision = new SelectTokenToPushDecision( source, countArray.Sum(), tokens, present );
				var token = await ctx.Self.Action.Decide( decision );

				if(token == null)
					break;

				Space destination = await SelectDestination( token );
				await ctx.GameState.Move( token, source, destination );

				pushedToSpaces.Add( destination ); // record push
				--countArray[indexLookup[token.Generic]]; // decrement count
			}
			return pushedToSpaces.ToArray();
		}

		protected virtual async Task<Space> SelectDestination( Token token ) {
			IEnumerable<Space> destinationOptions = source.Adjacent.Where( s => spaceFilter.TerrainMapper( s ) != Terrain.Ocean );
			return await ctx.Self.Action.Decide( new PushTokenDecision( token, source, destinationOptions, Present.Always ) );
		}

		#region private

		readonly IMakeGamestateDecisions ctx;
		protected readonly Space source;

		readonly List<int> countArray = new();
		readonly Dictionary<TokenGroup, int> indexLookup = new();
		SpaceFilter spaceFilter = SpaceFilter.Normal;

		#endregion

	}

	/// <summary>
	/// Overrides Selecting destination with a fixed destination
	/// </summary>
	public class TokenMover : TokenPusher {
		Space destination;
		public TokenMover( IMakeGamestateDecisions ctx, Space source, Space destination ) : base( ctx, source ) { 
			this.destination = destination;
		}

		protected override Task<Space> SelectDestination( Token token ) {
			return Task.FromResult(destination);
		}

	}

}