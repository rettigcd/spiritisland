using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {

	public class Space : IOption{

		#region SetUp
		int _highestDistanceCalculated = 0;
		void CalculateDistancesUpTo( int distance ) {
			while(_highestDistanceCalculated < distance) {
				int nextLevel = _highestDistanceCalculated + 1;
				var newSpaces = _distanceTo
					.Where( p => p.Value == _highestDistanceCalculated )
					.SelectMany( p => p.Key._distanceTo.Where(n=>n.Value==1).Select(q=>q.Key) ) // must get this directly
					.Distinct()
					.Where( space => !_distanceTo.ContainsKey( space )  // new space
						|| _distanceTo[space] > nextLevel ) // has a longer path than this path
					.ToList(); // force iteration so we can make changes to _distanceTo
				foreach(var newSpace in newSpaces)
					_distanceTo[newSpace] = nextLevel;
				_highestDistanceCalculated = nextLevel;
			}
		}
		/// <summary> If adjacent to ocean, sets is-costal </summary>
		public void SetAdjacentTo( params Space[] spaces ) {
			foreach(var land in spaces) {
				SetNeighbor( land );
				land.SetNeighbor( this );
			}

			_highestDistanceCalculated = 1;
		}

		void SetNeighbor( Space neighbor ) {
			_distanceTo.Add( neighbor, 1 ); // @@@ 
			if(neighbor.Terrain == Terrain.Ocean)
				this.IsCostal = true;
		}
		#endregion

		#region constructor

		public Space(Terrain terrain, string label,string startingItems=""){
			this.Terrain = terrain;
			this.Label = label;
			this.StartUpCounts = new StartUpCounts(startingItems);
			_distanceTo.Add(this,0);
		}

		#endregion

		public Board Board {
			get{ return board; }
			set{ if(board != null) throw new InvalidOperationException("cannot set board twice"); board = value; }
		}
		Board board;

		public string Label { get; }

		#region for Power

		public Terrain Terrain { get; }
		public Terrain TerrainForPower {
			get => terrainForPower ?? Terrain;
			set => terrainForPower = value;
		}
		Terrain? terrainForPower;
		public bool IsCostalForPower { 
			get => isCostalForPower ?? IsCostal;
			set => isCostalForPower = value;
		}
		bool? isCostalForPower;

		#endregion

		public bool IsCostal { get; set; }

		string IOption.Text => Label;

		public StartUpCounts StartUpCounts { get; }

		public IEnumerable<Space> Range( int distance ) {
			CalculateDistancesUpTo( distance );
			return _distanceTo.Where( p => p.Value <= distance ).Select( p => p.Key );
		}

		public IEnumerable<Space> SpacesExactly( int distance ) {
			CalculateDistancesUpTo( distance );
			return _distanceTo.Where( p => p.Value == distance ).Select( p => p.Key );
		}

		public IEnumerable<Space> Adjacent => SpacesExactly(1);

		public override string ToString() =>Label;

		readonly Dictionary<Space, int> _distanceTo = new Dictionary<Space, int>();

		public void InitTokens( TokenCountDictionary counts ) {
			StartUpCounts initialCounts = StartUpCounts;
			counts[ Invader.City[3] ]         = initialCounts.Cities;
			counts[ Invader.Town[2] ]         = initialCounts.Towns;
			counts[ Invader.Explorer[1] ]     = initialCounts.Explorers;
			counts[ TokenType.Dahan.Default ] = initialCounts.Dahan;
			counts.Blight.Count               = initialCounts.Blight; // don't use AddBlight because that pulls it from the card and triggers blighted island
		}

	}

}