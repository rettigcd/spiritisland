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
			this.TerrainForPower = terrain;
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

		public Terrain Terrain { get; }
		public Terrain TerrainForPower { get; set; }

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

		public bool Matches( Spirit self, GameState gameState, Target filterEnum ) => StandardPowerEvaluator( self, gameState, filterEnum, this );

		public Func<Spirit, GameState, Target, Space, bool> matcher = StandardPowerEvaluator;

		static public bool StandardPowerEvaluator( Spirit self, GameState gameState, Target filterEnum, Space s ) {
			return AllowOceanForPower(self,gameState,filterEnum,s) && s.Terrain != Terrain.Ocean;
		}

		static public bool AllowOceanForPower( Spirit self, GameState gameState, Target filterEnum, Space s ) {
			return filterEnum switch {
				// Depends on Terrain only
				Target.Any => true,
				Target.Costal => s.IsCostal,
				Target.SandOrWetland => s.Terrain.IsIn( Terrain.Sand, Terrain.Wetland ),
				Target.Jungle => s.Terrain == Terrain.Jungle,
				Target.Wetland => s.Terrain == Terrain.Wetland,
				Target.JungleOrMountain => s.Terrain.IsIn( Terrain.Jungle, Terrain.Mountain ),
				Target.JungleOrWetland => s.Terrain.IsIn( Terrain.Jungle, Terrain.Wetland ),
				Target.MountainOrWetland => s.Terrain.IsIn( Terrain.Mountain, Terrain.Wetland ),
				// Add Gamestate
				Target.Dahan => gameState.HasDahan( s ),
				Target.Invaders => gameState.HasInvaders( s ),
				Target.Blight => gameState.HasBlight( s ),
				Target.Explorer => gameState.InvadersOn( s ).HasExplorer,
				Target.TownOrExplorer => gameState.InvadersOn( s ).HasAny( Invader.Explorer, Invader.Town ),
				Target.BeastOrJungle => s.Terrain == Terrain.Jungle || gameState.HasBeasts( s ),

				Target.NoInvader => !gameState.HasInvaders( s ),
				Target.NoBlight => !gameState.HasBlight( s ),
				Target.DahanOrInvaders => gameState.HasDahan( s ) || gameState.HasInvaders( s ),
				// Add self
				Target.PresenceOrWilds => (self.Presence.IsOn( s ) || gameState.HasWilds( s )),
				_ => throw new ArgumentException( "Unexpected filter", nameof( filterEnum ) ),
			} && s.Terrain != Terrain.Ocean;
		}


	}

}