#nullable enable
namespace SpiritIsland;

static public class BoardFactory {

	// These cannot be reused because when they get connected to other boards, 
	// there neighbor state changes.
	static readonly public string[] AvailableBoards = ["A", "B", "C", "D", "E", "F"];
	static public Board Build(string boardName, BoardOrientation? orientation=null) {
		orientation ??= BoardOrientation.Home;
		return boardName switch {
			"A" => BuildA(orientation),
			"B" => BuildB(orientation),
			"C" => BuildC(orientation),
			"D" => BuildD(orientation),
			"E" => BuildE(orientation),
			"F" => BuildF(orientation),
			_ => throw new ArgumentException($"Unknown board '{boardName}'", nameof(boardName))
		};
	}

	static public Board BuildA(BoardOrientation? orientation = null) {

		Board board = new Board("A"
			, orientation ?? BoardOrientation.Home
			, new SSS(Terrain.Ocean, "A0")
			, new SSS(Terrain.Mountain, "A1") // 
			, new SSS(Terrain.Wetland, "A2", "CD")  // city, dahan
			, new SSS(Terrain.Jungle, "A3", "DD")   // 2 dahan
			, new SSS(Terrain.Sands, "A4", "B")     // blight
			, new SSS(Terrain.Wetland, "A5")
			, new SSS(Terrain.Mountain, "A6", "D") // 1 dahan
			, new SSS(Terrain.Sands, "A7", "DD")     // 2 dahan
			, new SSS(Terrain.Jungle, "A8", "T")   // town
		);

		board.SetNeighbors(0, 1, 2, 3);
		board.SetNeighbors(1, 2, 4, 5, 6);
		board.SetNeighbors(2, 3, 4);
		board.SetNeighbors(3, 4);
		board.SetNeighbors(4, 5);
		board.SetNeighbors(5, 6, 7, 8);
		board.SetNeighbors(6, 8);
		board.SetNeighbors(7, 8);

		board.DefineSide(7, 5, 4, 3).BreakAt(1, 3, 7);// Side 0
		board.DefineSide(8, 7).BreakAt(5);            // Side 1
		board.DefineSide(1, 6, 8).BreakAt(3, 7);              // Side 2

		return board;
	}

	static public Board BuildB(BoardOrientation? orientation = null) {

		Board board = new Board("B"
			, orientation ?? BoardOrientation.Home
			, new SSS(Terrain.Ocean, "B0")
			, new SSS(Terrain.Wetland, "B1", "D")  // 1 dahan
			, new SSS(Terrain.Mountain, "B2", "C") // city
			, new SSS(Terrain.Sands, "B3", "DD")     // 2 dahan
			, new SSS(Terrain.Jungle, "B4", "B")   // blight
			, new SSS(Terrain.Sands, "B5")
			, new SSS(Terrain.Wetland, "B6", "T")  // 1 town
			, new SSS(Terrain.Mountain, "B7", "D") // 1 dahan
			, new SSS(Terrain.Jungle, "B8", "DD")   // 2 dahan
		);

		board.SetNeighbors(0, 1, 2, 3);
		board.SetNeighbors(1, 2, 4, 5, 6);
		board.SetNeighbors(2, 3, 4);
		board.SetNeighbors(3, 4);
		board.SetNeighbors(4, 5, 7);
		board.SetNeighbors(5, 6, 7);
		board.SetNeighbors(6, 7, 8);
		board.SetNeighbors(7, 8);

		board.DefineSide(7, 4, 3).BreakAt(1, 5);    // Side 0
		board.DefineSide(8, 7).BreakAt(5);          // Side 1
		board.DefineSide(1, 6, 8).BreakAt(1, 7);            // Side 2

		return board;
	}

	static public Board BuildC(BoardOrientation? orientation = null) {

		var board = new Board("C"
			, orientation ?? BoardOrientation.Home
			, new SSS(Terrain.Ocean, "C0")
			, new SSS(Terrain.Jungle, "C1", "D")   // 1 dahan
			, new SSS(Terrain.Sands, "C2", "C")     // city
			, new SSS(Terrain.Mountain, "C3", "DD") // 2 dahan
			, new SSS(Terrain.Jungle, "C4")
			, new SSS(Terrain.Wetland, "C5", "DDB")  // 2 dahan, blight
			, new SSS(Terrain.Sands, "C6", "D")     // 1 dahan
			, new SSS(Terrain.Mountain, "C7", "T") // 1 town
			, new SSS(Terrain.Wetland, "C8")
		);

		board.SetNeighbors(0, 1, 2, 3);
		board.SetNeighbors(1, 2, 5, 6);
		board.SetNeighbors(2, 3, 4, 5);
		board.SetNeighbors(3, 4);
		board.SetNeighbors(4, 5, 7);
		board.SetNeighbors(5, 6, 7);
		board.SetNeighbors(6, 7, 8);
		board.SetNeighbors(7, 8);

		board.DefineSide(4, 3).BreakAt(7);          // Side 0
		board.DefineSide(8, 7, 4).BreakAt(7, 11);   // Side 1
		board.DefineSide(1, 6, 8).BreakAt(3, 7);            // Side 2

		return board;
	}

	static public Board BuildD(BoardOrientation? orientation = null) {
		var board = new Board("D"
			, orientation ?? BoardOrientation.Home
			, new SSS(Terrain.Ocean, "D0")
			, new SSS(Terrain.Wetland, "D1", "DD")   // 2 dahan
			, new SSS(Terrain.Jungle, "D2", "CD")    // city, 1 dahan
			, new SSS(Terrain.Wetland, "D3")
			, new SSS(Terrain.Sands, "D4")
			, new SSS(Terrain.Mountain, "D5", "DB")  // 1 dahan, blight
			, new SSS(Terrain.Jungle, "D6")
			, new SSS(Terrain.Sands, "D7", "TDD")      // 1 town, 2 dahan
			, new SSS(Terrain.Mountain, "D8")
		);

		board.SetNeighbors(0, 1, 2, 3);
		board.SetNeighbors(1, 2, 5, 7, 8);
		board.SetNeighbors(2, 3, 4, 5);
		board.SetNeighbors(3, 4);
		board.SetNeighbors(4, 5, 6);
		board.SetNeighbors(5, 6, 7);
		board.SetNeighbors(6, 7);
		board.SetNeighbors(7, 8);

		board.DefineSide(6, 4, 3).BreakAt(3, 9);  // Side 0
		board.DefineSide(8, 7, 6).BreakAt(5, 11); // Side 1
		board.DefineSide(1, 8).BreakAt(11);           // Side 2

		return board;
	}

	static public Board BuildE(BoardOrientation? orientation = null) {

		var board = new Board("E"
			, orientation ?? BoardOrientation.Home
			, new SSS(Terrain.Ocean, "E0")
			, new SSS(Terrain.Sands, "E1", "D")   // 1 dahan
			, new SSS(Terrain.Mountain, "E2", "C")    // city
			, new SSS(Terrain.Jungle, "E3", "DD")  // 2 dahan
			, new SSS(Terrain.Wetland, "E4", "B")      // 1 blight
			, new SSS(Terrain.Mountain, "E5", "D")  // 1 dahan
			, new SSS(Terrain.Sands, "E6")
			, new SSS(Terrain.Jungle, "E7", "T")      // 1 town
			, new SSS(Terrain.Wetland, "E8", "DD") // 2 dahan
		);

		board.SetNeighbors(0, 1, 2, 3);
		board.SetNeighbors(1, 2, 5, 7);
		board.SetNeighbors(2, 3, 5);
		board.SetNeighbors(3, 4, 5);
		board.SetNeighbors(4, 5, 6, 7);
		board.SetNeighbors(5, 7);
		board.SetNeighbors(6, 7, 8);
		board.SetNeighbors(7, 8);

		board.DefineSide(6, 4, 3).BreakAt(1, 7); // Side 0
		board.DefineSide(8, 6).BreakAt(9);       // Side 1
		board.DefineSide(1, 7, 8).BreakAt(5, 9); // Side 2

		return board;
	}

	static public Board BuildF(BoardOrientation? orientation = null) {

		var board = new Board("F"
			, orientation ?? BoardOrientation.Home
			, new SSS(Terrain.Ocean, "F0")
			, new SSS(Terrain.Sands, "F1", "DD")
			, new SSS(Terrain.Jungle, "F2", "C")
			, new SSS(Terrain.Wetland, "F3", "D")
			, new SSS(Terrain.Mountain, "F4", "B")
			, new SSS(Terrain.Jungle, "F5", "D")
			, new SSS(Terrain.Mountain, "F6", "DD")
			, new SSS(Terrain.Wetland, "F7", "")
			, new SSS(Terrain.Sands, "F8", "T")
		);

		board.SetNeighbors(0, 1, 2, 3);
		board.SetNeighbors(1, 2, 5, 6);
		board.SetNeighbors(2, 3, 4, 5);
		board.SetNeighbors(3, 4);
		board.SetNeighbors(4, 5, 7, 8);
		board.SetNeighbors(5, 6, 8);
		board.SetNeighbors(6, 8);
		board.SetNeighbors(7, 8);

		board.DefineSide(7, 4, 3).BreakAt(3, 9);  // Side 0
		board.DefineSide(8, 7).BreakAt(7);        // Side 1
		board.DefineSide(1, 6, 8).BreakAt(5, 11); // Side 2

		return board;
	}

}
