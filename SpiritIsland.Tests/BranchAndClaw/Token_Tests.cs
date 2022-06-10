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
			var gs = new GameState( new Thunderspeaker(), Board.BuildBoardC());

			// Given: a space with no invaders
			Space noInvaderSpace = gs.Island.AllSpaces.First(s=>!s.IsOcean && !gs.Tokens[s].HasInvaders());
			var tokens = gs.Tokens[noInvaderSpace];
			//   And: 1 neighboring town
			gs.Tokens[ noInvaderSpace.Adjacent.First() ].AdjustDefault(Invader.Town,1);
			//   And: 1 wilds there
			tokens.Wilds.Init(1);

			//  When: we explore there
			_ = InvaderCardEx.For( noInvaderSpace ).Explore( gs );

			//  Then: still no invaders
			gs.Tokens[noInvaderSpace].HasInvaders().ShouldBeFalse("there should be no explorers in "+noInvaderSpace.Label);
			//   And: no wilds here
			(tokens.Wilds.Count>0).ShouldBeFalse("wilds should be used up");

		}

		[Fact]
		public async Task Disease_Stops_Build() {
			var gs = new GameState( new Thunderspeaker(), Board.BuildBoardC() );

			// Given: a space with ONLY 1 explorer
			Space space = gs.Island.AllSpaces.First( s => !s.IsOcean && !gs.Tokens[s].HasInvaders() ); // 0 invaders
			gs.Tokens[space].AdjustDefault( Invader.Explorer, 1 ); // add explorer
			//   And: 1 diseases there
			await gs.Tokens[space].Disease.Bind(Guid.NewGuid()).Add(1);

			//  When: we build there
			await InvaderCardEx.For( space ).Build( gs );

			//  Then: still no towns (just original explorer)
			gs.Assert_Invaders(space, "1E@1" ); // "should be no town on "+space.Label
			//   And: no disease here
			gs.Tokens[space].Disease.Any.ShouldBeFalse( "disease should be used up" );


		}

	}

}
