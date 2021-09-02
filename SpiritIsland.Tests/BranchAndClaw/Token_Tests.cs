﻿using Shouldly;
using SpiritIsland.Basegame;
using SpiritIsland.BranchAndClaw;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SpiritIsland.Tests.BranchAndClaw {

	public class Token_Tests {

		[Fact]
		public void Wilds_Stops_Explore() {
			var gs = new GameState_BranchAndClaw( new Thunderspeaker(), Board.BuildBoardC());

			// Given: a space with no invaders
			Space noInvaderSpace = gs.Island.AllSpaces.First(s=>s.Terrain!=Terrain.Ocean && !gs.HasInvaders(s));
			var counts = gs.Tokens[noInvaderSpace];
			//   And: 1 neighboring town
			gs.Tokens[ noInvaderSpace.Adjacent.First() ].Adjust(Invader.Town[2],1);
			//   And: 1 wilds there
			counts.Adjust(BacTokens.Wilds,1);

			//  When: we explore there
			gs.Explore(new InvaderCard(noInvaderSpace.Terrain));

			//  Then: still no invaders
			gs.HasInvaders(noInvaderSpace).ShouldBeFalse("there should be no explorers in "+noInvaderSpace.Label);
			//   And: no wilds here
			counts.Has(BacTokens.Wilds).ShouldBeFalse("wilds should be used up");

		}

		[Fact]
		public async Task Disease_Stops_Build() {
			var gs = new GameState_BranchAndClaw( new Thunderspeaker(), Board.BuildBoardC() );

			// Given: a space with ONLY 1 explorer
			Space space = gs.Island.AllSpaces.First( s => s.Terrain != Terrain.Ocean && !gs.HasInvaders( s ) ); // 0 invaders
			gs.Tokens[space].Adjust( Invader.Explorer[1], 1 ); // add explorer
			//   And: 1 diseases there
			gs.Tokens[space].Adjust(BacTokens.Disease,1);

			//  When: we build there
			await gs.Build( new InvaderCard( space.Terrain ) );

			//  Then: still no towns (just original explorer)
			gs.Assert_Invaders(space, "1E@1" ); // "should be no town on "+space.Label
			//   And: no disease here
			gs.Tokens[space].Has(BacTokens.Disease).ShouldBeFalse( "disease should be used up" );


		}

	}

}
