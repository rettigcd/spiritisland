using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class Invaders {

		// !! This wrapper class (around SpaceInvaderCount) acts more like an Extension Method Class

		#region constructor

		public Invaders( GameState gs ) {
			this.gs = gs;
			this.gs.TimePassed += Heal;
		}

		#endregion

		public bool AreOn( Space space ) => Counts[space].Keys.Any();

		public Task Move( InvaderSpecific invader, Space from, Space to ) {
			Counts[from][invader]--;
			Counts[to][invader]++;
			return Moved.InvokeAsync( gs, new InvaderMovedArgs { from = from, to = to, Invader = invader } );
		}

		public InvaderGroup On( Space targetSpace, Cause cause ) {
			var counts = Counts[targetSpace];
			var grp = new InvaderGroup( targetSpace, counts );
			if(cause != Cause.None)
				grp.DestroyInvaderStrategy = new DestroyInvaderStrategy( gs.Fear.AddDirect, cause );
			return grp;
		}

		public override string ToString() {
			return Counts.Keys
				.Where(AreOn)
				.Select(x=>Counts[x].ToSummary())
				.Join(" / ");
		}


		#region private

		void Heal( GameState obj ) {
			foreach(var space in Counts.Keys)
				new InvaderGroup( space, Counts[space] ).Heal();
		}

		readonly GameState gs;

		#endregion

		public AsyncEvent<InvaderMovedArgs> Moved = new AsyncEvent<InvaderMovedArgs>();  // Thunderspeaker

		public SpaceInvaderCounts Counts = new SpaceInvaderCounts();

	}

	public class SpaceInvaderCounts {
		public IInvaderCounts this[Space space] {
			get {
				var countArray = invaderCount.ContainsKey( space )
					? invaderCount[space]
					: invaderCount[space] = new CountDictionary<InvaderSpecific>();
				return new InvaderCounts( countArray );
			}
		}

		public IEnumerable<Space> Keys => invaderCount.Keys;

		readonly Dictionary<Space, CountDictionary<InvaderSpecific>> invaderCount = new Dictionary<Space, CountDictionary<InvaderSpecific>>();

	}

}
