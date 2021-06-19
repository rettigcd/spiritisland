using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace SpiritIsland.Tests.Innates {

	public class MassiveFlooding_Tests {

		// Not enought elements -> nothing
		[Fact]
		public void InsufficientElements(){
			// Given: river -
			//   And: no cards / no elements
			
			//   And: in slow phase

			//  When: Select Massive Flooding

			// Then: Massive Flooding in unresolved

			//  And: Resolved

			//  and: No invaders changed - how do you test this???
			
		}

		// 1 sun, 2 water => Push 1 Explorer or Town
		[Fact]
		public void Level1_Pushes1TownOrExplorer(){
			// Given: River
			//   And: SS in A3
			//   And: presence in A8

			//   And: 2E, 2T, & 2T@1, 2C in every space on the board

			//   And: has purchased fast cards with exactly 1 sun & 2 water

			//   And: in slow phase

			//  When: Select Massive Flooding

			//  Then: Massive Flooding is unresolved

			//  Then: options include A2 & A4
			//  Then: Select A2 land that is within 1 of SS

			//  Then: options are 1E, 1T & 1T@1
			//  Then: Select 1T@1 to push

			//  Options are A1, A3, A4
			//  Then: select target

			// Resolved.

			// Verify Destination && Target
		}

		// 2 sun, 3 water => Instead, 2 damage, Push up to 3 explorers and/or towns

		// 3 sun, 4 water, 1 earth => Instead, 2 damage to each invader

		// !!!!!!!!!   Rivers special rules -> presence in water is SS

	}

}
