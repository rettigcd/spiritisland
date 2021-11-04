using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland.Decision.Presence {

	/// <summary>
	/// Presence follows another token
	/// </summary>
	public class Gather : Deployed, IAdjacentDecision {

		/// <summary>
		/// Predetermined From, Predetermined To
		/// </summary>
		public Gather(string prompt, Space to, params Space[] from ) 
			:base(prompt, from, Present.Done)
		{
			this.Original = to;
			this.Adjacent = from;
		}

		public Gather(string prompt, Space to, IEnumerable<Space> from ) 
			:base(prompt, from, Present.Done)
		{
			this.Original = to;
			this.Adjacent = from.ToArray();
		}

		public AdjacentDirection Direction => AdjacentDirection.Incoming;

		public Space Original { get; }

		public Space[] Adjacent { get; }
	}


}
