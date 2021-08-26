
using System;

namespace SpiritIsland {

	/// <summary>
	/// plus the callback when user makes a decision
	/// </summary>
	public interface IDecisionMaker : IDecisionPlus {
		public void Select(IOption option);
	}

}
