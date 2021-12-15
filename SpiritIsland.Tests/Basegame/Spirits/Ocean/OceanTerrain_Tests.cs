using Shouldly;
using SpiritIsland.Basegame;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SpiritIsland.Tests.Basegame.Spirits.OceanNS {

	public class OceanTerrain_Tests {

		readonly Board board = Board.BuildBoardA();
		readonly RiverSurges river = new RiverSurges();
		readonly Ocean ocean = new Ocean();

		public OceanTerrain_Tests() {

		}

		public GameState GetGame(bool oceanIsPlaying ) {
			var gs = oceanIsPlaying
				? new GameState( river, ocean ) { Island = new Island( board, Board.BuildBoardB() ) }
				: new GameState( river, board );

			gs.Initialize();

			if(oceanIsPlaying)
				ocean.Presence.PlaceOn( gs.Island.Boards[0][8], gs ); // put ocean presence on river's board, but not in the ocean

			return gs;
		}

		[Theory]
		[InlineData( false, "A1,A2" )]
		[InlineData( true, "A0,A1,A2" )]
		public void TargettingOceanAsWetLand(bool hasOcean,string expectedOptions ) {
			var gs = GetGame( hasOcean );
			gs.Phase = Phase.Fast;

			river.Presence.Move( board[5], board[2], gs ); // move it to A2
			Assert_Options( "A2", river.Presence.Spaces, "river starting presence" );

			// Talons ofLightning - Range 1
			_ = PowerCard.For<TalonsOfLightning>().ActivateAsync( new SelfCtx( river, gs, Cause.Power ) );

			Assert_Options( expectedOptions, river.Action.GetCurrent().Options, hasOcean ? "include ocean" : "exclude ocean" );

			static void Assert_Options( string expectedOptions, IEnumerable<IOption> actual, string msg ) {
				var options = actual.Select( x => x.Text ).OrderBy( x => x ).Join( "," );
				options.ShouldBe( expectedOptions, msg );
			}

		}

		// (1) cannot cascade blight into ocean

		// (2) drown any invaders / dahan pushed into these areas
		// (2) can't push invaders into Ocean
		// (2) can't push dahan into ocean

		// With Ocean

		// (0) no change for other spirits growth
		// (0) other spirits cannot grow (place a presence) into ocean

		// (1) Can Target out power that requires a wetland (from/to) ocean
		// (1) can cascade blight into ocean
		// (2) can push invaders into the ocean and drown them
		// (2) Tidal Boon generates energy
		// Thunderspeaker can ride dahan into the ocean
	}

	// Swallow the Land Dwellers - "Drown" generates energy for ocean
	// Tidal Boon - If targetted spirit Pushes Dahan into ocean, you can push them into another costal
	// Pushing Dahan into ocean destorys them - except for Tidal Boon

	// May move presence into oceans.
	// May not add/move presence into inlands.
	// Drown Destorys invader pieces, for every (# of players) invaders drowned, gain 1 energy


}
