using Shouldly;
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
			Space noInvaderSpace = gs.Island.AllSpaces.First(s=>s.Terrain!=Terrain.Ocean && !gs.Invaders.AreOn(s));
			//   And: 1 neighboring town
			gs.Invaders.Counts[ noInvaderSpace.Adjacent.First() ].Adjust(Invader.Town[2],1);
			//   And: 1 wilds there
			gs.Wilds.AddOneTo(noInvaderSpace);

			//  When: we explore there
			gs.Explore(new InvaderCard(noInvaderSpace.Terrain));

			//  Then: still no invaders
			gs.Invaders.AreOn(noInvaderSpace).ShouldBeFalse("there should be no explorers in "+noInvaderSpace.Label);
			//   And: no wilds here
			gs.Wilds.AreOn(noInvaderSpace).ShouldBeFalse("wilds should be used up");

		}

		[Fact]
		public async Task Disease_Stops_Build() {
			var gs = new GameState_BranchAndClaw( new Thunderspeaker(), Board.BuildBoardC() );

			// Given: a space with ONLY 1 explorer
			Space space = gs.Island.AllSpaces.First( s => s.Terrain != Terrain.Ocean && !gs.Invaders.AreOn( s ) ); // 0 invaders
			gs.Invaders.Counts[space].Adjust( Invader.Explorer[1], 1 ); // add explorer
			//   And: 1 diseases there
			gs.Disease.AddOneTo( space );

			//  When: we build there
			await gs.Build( new InvaderCard( space.Terrain ) );

			//  Then: still no towns (just original explorer)
			gs.Assert_Invaders(space, "1E@1" ); // "should be no town on "+space.Label
			//   And: no disease here
			gs.Disease.AreOn( space ).ShouldBeFalse( "disease should be used up" );

		}

	}

}
