using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace SpiritIsland.Tests {

	[TestFixture]
	public class BoardSpace_Tests {

		[TestCase( 0 )]
		[TestCase( 1 )]
		[TestCase( 2 )]
		public void Space_Is0DistanceFromSelf( int distance ) {
			var space = new BoardSpace();
			var spaces = space.SpacesWithin( distance );
			Assert.That( spaces, Contains.Item( space ) );
		}

		[Test]
		public void Adjacentcy_IsTransitive() {
			// Given: land-1 is adjacent to land-2
			var land1 = new BoardSpace();
			var land2 = new BoardSpace();
			land1.SetAdjacentTo( land2 );
			// Then: land2 is adjacent to land 1
			Assert.That( land2.SpacesExactly( 1 ), Contains.Item( land1 ) );
			Assert.That( land1.SpacesExactly( 1 ), Contains.Item( land2 ) );
		}

		[Test]
		public void MultipleNeighbors() {
			// Given space has 2 neighbors
			var main = new BoardSpace();
			var neighbor1 = new BoardSpace();
			var neighbor2 = new BoardSpace();
			main.SetAdjacentTo( neighbor1 );
			main.SetAdjacentTo( neighbor2 );
			// Then: it is adjacent to both
			var neighbors = main.SpacesWithin( 1 );
			Assert.That( neighbors, Contains.Item( neighbor1 ) );
			Assert.That( neighbors, Contains.Item( neighbor2 ) );
			//  And: Neighbors are 2 away from each other
			Assert.That( neighbor1.SpacesWithin( 2 ), Contains.Item( neighbor2 ) );
			Assert.That( neighbor2.SpacesExactly( 2 ), Contains.Item( neighbor1 ) );
		}

		[Test]
		public void BoardA_Connectivity() {
			BoardSpace[] spaces = BoardTile.GetBoardA().spaces;

			Assert_HasSpaces( spaces[0], 0, spaces[0] );
			Assert_HasSpaces( spaces[0], 1, spaces[1], spaces[2], spaces[3] );
			Assert_HasSpaces( spaces[0], 2, spaces[4], spaces[5], spaces[6] );
			Assert_HasSpaces( spaces[0], 3, spaces[7], spaces[8] );

			Assert.True( spaces[1].IsCostal );
			Assert.True( spaces[2].IsCostal );
			Assert.True( spaces[3].IsCostal );

		}

		[Test]
		public void BoardB_Connectivity() {
			BoardSpace[] spaces = BoardTile.GetBoardA().spaces;

			Assert_HasSpaces( spaces[0], 0, spaces[0] );
			Assert_HasSpaces( spaces[0], 1, spaces[1], spaces[2], spaces[3] );
			Assert_HasSpaces( spaces[0], 2, spaces[4], spaces[5], spaces[6] );
			Assert_HasSpaces( spaces[0], 3, spaces[7], spaces[8] );

			Assert.True( spaces[1].IsCostal );
			Assert.True( spaces[2].IsCostal );
			Assert.True( spaces[3].IsCostal );

		}

		[Test]
		public void PlaceTiles() {

			var tileB = BoardTile.GetBoardB();
			var tileD = BoardTile.GetBoardD();

			tileB.Sides[2].AlignTo( tileD.Sides[0] );

			Assert.That( tileB.spaces[3].SpacesExactly( 1 ), Contains.Item( tileD.spaces[1] ) );
			Assert.That( tileB.spaces[4].SpacesExactly( 1 ), Contains.Item( tileD.spaces[1] ) );
			Assert.That( tileB.spaces[4].SpacesExactly( 1 ), Contains.Item( tileD.spaces[8] ) );
			Assert.That( tileB.spaces[7].SpacesExactly( 1 ), Contains.Item( tileD.spaces[8] ) );
		}

		#region private

		void Assert_HasSpaces( BoardSpace source, int distance, params BoardSpace[] needles ) {
			Assert.That( source.SpacesExactly( distance ), Is.EquivalentTo( needles ) );
		}

		#endregion

	}

}
