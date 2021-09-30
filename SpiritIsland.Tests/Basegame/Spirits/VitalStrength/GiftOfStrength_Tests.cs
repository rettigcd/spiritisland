using SpiritIsland.Basegame;
using SpiritIsland.SinglePlayer;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SpiritIsland.Tests.Basegame.Spirits.VitalStrengthNS {

	public class GiftOfStrength_Tests {

		protected SinglePlayerGame game;
		protected readonly Spirit spirit;
		protected readonly VirtualEarthUser User;

		public GiftOfStrength_Tests() {
			spirit = new VitalStrength();
			User = new VirtualEarthUser( spirit );
			var gs = new GameState( spirit, Board.BuildBoardA() );
			game = new SinglePlayerGame( gs );
		}

		[Fact]
		public void Replaying_FastCards() {

			// Given: Earth has enough elements to trigger GOS
			User.SelectsGrowthA_Reclaim_PP2();
			spirit.Elements[Element.Sun] = 1;
			spirit.Elements[Element.Earth] = 2;
			spirit.Elements[Element.Plant] = 2;

			//  And: Earth has 4 cards
			spirit.AddActionFactory( MakePowerCard( Slow0 ) ); // not played
			spirit.AddActionFactory( MakePowerCard( Fast0 ) ); // not played
			spirit.AddActionFactory( MakePowerCard( Fast1 ) ); // played - should appear
			spirit.AddActionFactory( MakePowerCard( Fast2 ) ); // played - no - too expensive
			User.IsDoneBuyingCards();

			//  And: already played 2 fast cards (cost 1 & 2)
			User.SelectsFastAction( "Fast-0,(Fast-1),Fast-2,Gift of Strength" );
			User.SelectsFastAction( "Fast-0,(Fast-2),Gift of Strength" );

			User_PlaysGiftOfStrengthOnSelf();

			// Then: user can replay ONLY the played: Fast-1 card.
			User.SelectsFastAction( "Fast-0,(Replay Card [max cost:1])" );
			User.AssertDecision( "Select card to replay", "Fast-1", "Fast-1" ); // !!! should there be a ,Done here?

		}

		[Fact]
		public void Replaying_SlowCards() {

			// Given: Earth has enough elements to trigger GOS
			User.SelectsGrowthA_Reclaim_PP2();
			spirit.Elements[Element.Sun] = 1;
			spirit.Elements[Element.Earth] = 2;
			spirit.Elements[Element.Plant] = 2;

			//  And: Earth has 4 cards
			spirit.AddActionFactory( MakePowerCard( Fast0 ) ); // played - no - its fast!
			spirit.AddActionFactory( MakePowerCard( Slow0 ) ); // not played
			spirit.AddActionFactory( MakePowerCard( Slow1 ) ); // played - should appear
			spirit.AddActionFactory( MakePowerCard( Slow2 ) ); // played - no - too expensive
			User.IsDoneBuyingCards();

			//  And: played GOS on self
			User_PlaysGiftOfStrengthOnSelf();
			//  And: Played fast card
			User.SelectsFastAction( "(Fast-0),Replay Card [max cost:1]" );
			User.IsDoneWith( Speed.Fast );

			// (now in slow...)

			//  And: plays Slow-1
			User.SelectsSlowAction( "Slow-0,(Slow-1),Slow-2,Replay Card [max cost:1]" );

			// When: Replaying card
			User.SelectsSlowAction( "Slow-0,Slow-2,(Replay Card [max cost:1])" );
			User.AssertDecision( "Select card to replay", "Slow-1", "Slow-1" ); // !!! should there be a ,Done here?
		}

		void User_PlaysGiftOfStrengthOnSelf() {
			// When: user applies 'Gift of Strength' to self
			User.SelectsFastAction( "Fast-0,(Gift of Strength)" );
		}


		// Replay-Action works in slow
		// In Slow, Replacy finds only Played-Slow
		//  - ignores fast
		//  - ignores unplayed-slow

		static public PowerCard MakePowerCard( Func<TargetSpaceCtx,Task> d ) => PowerCard.For( d.Method );

		[SpiritCard("Slow-0",0,Speed.Slow)]
		[FromPresenceIn(0,Terrain.Ocean)] // will skip the Target-Space step
		static Task Slow0(TargetSpaceCtx _) => Task.CompletedTask;

		[SpiritCard("Slow-1",1,Speed.Slow)]
		[FromPresenceIn(0,Terrain.Ocean)] // will skip the Target-Space step
		static Task Slow1(TargetSpaceCtx _) => Task.CompletedTask;

		[SpiritCard("Slow-2",2,Speed.Slow)]
		[FromPresenceIn(0,Terrain.Ocean)] // will skip the Target-Space step
		static Task Slow2(TargetSpaceCtx _) => Task.CompletedTask;


		[SpiritCard("Fast-0",0,Speed.Fast)]
		[FromPresenceIn(0,Terrain.Ocean)] // will skip the Target-Space step
		static Task Fast0(TargetSpaceCtx _) => Task.CompletedTask;

		[SpiritCard("Fast-1",1,Speed.Fast)]
		[FromPresenceIn(0,Terrain.Ocean)] // will skip the Target-Space step
		static Task Fast1(TargetSpaceCtx _) => Task.CompletedTask;

		[SpiritCard("Fast-2",2,Speed.Fast)]
		[FromPresenceIn(0,Terrain.Ocean)] // will skip the Target-Space step
		static Task Fast2(TargetSpaceCtx _) => Task.CompletedTask;

	}

}
