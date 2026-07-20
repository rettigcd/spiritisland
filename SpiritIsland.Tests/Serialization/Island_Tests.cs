namespace SpiritIsland.Tests.Serialization;

/// <summary>
/// Round-trips for Island.ToJson/FromJson - docs/GameSerialization-Roadmap.md section 8.
/// </summary>
public class Island_Tests {

	[Fact]
	public void RoundTrips_SingleBoard_OrientationAndInvaderActionCount() {
		var board = BoardFactory.BuildC( new BoardOrientation( new BoardCoord( 2, -1 ), 4 ) );
		board.InvaderActionCount = 3;
		var island = new Island( board );

		JsonArray json = island.ToJson();
		Island restored = Island.FromJson( json );

		restored.Boards.Length.ShouldBe( 1 );
		Board restoredBoard = restored.Boards[0];
		restoredBoard.Name.ShouldBe( "C" );
		restoredBoard.Orientation.Offset.ShouldBe( new BoardCoord( 2, -1 ) );
		restoredBoard.Orientation.RotationSteps.ShouldBe( 4 );
		restoredBoard.InvaderActionCount.ShouldBe( 3 );
	}

	[Fact]
	public void RoundTrips_MultiBoard_PreservesCrossBoardConnections() {
		// Proves the FromJson-goes-through-the-constructor design decision (see the doc comment on
		// FromJson): boards reconstructed from JSON are actually connected to each other, not just
		// individually valid. Mirrors BoardSpace_Tests.Island_2Boards's known-good layout.
		var layout = GameBuilder.TwoBoardLayout;
		Board boardC = BoardFactory.Build( "C", layout[0] );
		Board boardD = BoardFactory.Build( "D", layout[1] );
		var island = new Island( boardC, boardD );

		JsonArray json = island.ToJson();
		Island restored = Island.FromJson( json );

		Board restoredC = restored.Boards.Single( b => b.Name == "C" );
		Board restoredD = restored.Boards.Single( b => b.Name == "D" );

		restoredC[3].Adjacent_Existing.ShouldContain( restoredD[6] );
		restoredC[3].Adjacent_Existing.ShouldContain( restoredD[4] );
		restoredC[4].Adjacent_Existing.ShouldContain( restoredD[4] );
		restoredC[4].Adjacent_Existing.ShouldContain( restoredD[3] );
	}

	[Fact]
	public void MementoRestore_MultiBoard_PreservesCrossBoardConnections() {
		// Real bug, not a deliberate design choice as originally assumed: MyMemento.Restore used to
		// assign src.Boards directly, skipping ConnectSides() - so in-memory undo never re-established
		// cross-board adjacency on the freshly-rebuilt Board objects it constructs. Fixed to call
		// src.ConnectSides() after reassigning src.Boards, same as FromJson already did. Mirrors
		// RoundTrips_MultiBoard_PreservesCrossBoardConnections above, but through IHaveMemento instead of JSON.
		var layout = GameBuilder.TwoBoardLayout;
		Board boardC = BoardFactory.Build( "C", layout[0] );
		Board boardD = BoardFactory.Build( "D", layout[1] );
		var island = new Island( boardC, boardD );

		object memento = ( (IHaveMemento)island ).Memento;
		( (IHaveMemento)island ).Memento = memento;

		Board restoredC = island.Boards.Single( b => b.Name == "C" );
		Board restoredD = island.Boards.Single( b => b.Name == "D" );

		restoredC[3].Adjacent_Existing.ShouldContain( restoredD[6] );
		restoredC[3].Adjacent_Existing.ShouldContain( restoredD[4] );
		restoredC[4].Adjacent_Existing.ShouldContain( restoredD[4] );
		restoredC[4].Adjacent_Existing.ShouldContain( restoredD[3] );
	}

	[Fact]
	public void RoundTrips_NativeTerrainOverrides() {
		var island = new Island( Boards.C );
		var space = (SingleSpaceSpec)island.Boards[0][3];
		space.NativeTerrain = Terrain.Destroyed; // e.g. Cast Down

		JsonArray json = island.ToJson();
		Island restored = Island.FromJson( json );

		var restoredSpace = (SingleSpaceSpec)restored.Boards[0][3];
		restoredSpace.NativeTerrain.ShouldBe( Terrain.Destroyed );

		// an unmodified space keeps its original terrain, not overwritten with the default
		var untouchedOriginal = (SingleSpaceSpec)island.Boards[0][1];
		var untouchedRestored = (SingleSpaceSpec)restored.Boards[0][1];
		untouchedRestored.NativeTerrain.ShouldBe( untouchedOriginal.NativeTerrain );
	}

}
