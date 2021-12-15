﻿using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	/// <summary>
	/// Shifting Memories Track-Action
	/// </summary>
	public class DiscardElementsForCardPlay : GrowthActionFactory, ITrackActionFactory {

		readonly int totalNumToRemove;
		public DiscardElementsForCardPlay(int elementDiscardCount ) {
			this.totalNumToRemove = elementDiscardCount;
		}

		public bool RunAfterGrowthResult => true; // delay for gained prepared elements.

		public override async Task ActivateAsync( SelfCtx ctx ) {
			if( ctx.Self is ShiftingMemoryOfAges smoa
				&& totalNumToRemove <= smoa.PreparedElements.Total
				&& (await smoa.DiscardElements(totalNumToRemove,"additional card-play")).Count == totalNumToRemove
			) smoa.tempCardPlayBoost++;
		}

	}
}
