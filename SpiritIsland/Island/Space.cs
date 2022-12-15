using System.Drawing;

namespace SpiritIsland;

public interface Restoreable {
	public void Restore();
}

public abstract class Space 
	: IOption 
	, HasNeighbors<Space>
{

	readonly List<Space> adjacent = new List<Space>();
	Board board;

	protected Space(string label) {
		this.Label = label;
	}

	public Board Board {
		get { return board; }
		set { if(board != null) throw new InvalidOperationException( "cannot set board twice" ); board = value; }
	}

	public string Label { get; }

	// SpaceFilterMap

	public bool IsSand => Is( Terrain.Sand );

	public bool IsJungle => Is( Terrain.Jungle );

	public bool IsWetland => Is( Terrain.Wetland );

	public bool IsMountain => Is( Terrain.Mountain );	

	public bool IsOcean => Is( Terrain.Ocean );

	public bool IsCoastal { get; set; }

	public IEnumerable<Space> Adjacent => adjacent;
	public string Text => Label;

	public void Disconnect() {
		// Remove us from neighbors adjacent list
		foreach(var a in adjacent)
			a.adjacent.Remove( this );
		// Remove neighbors from our list.
		adjacent.Clear();
	}

	public abstract bool Is( Terrain terrain );
	public abstract bool IsOneOf( params Terrain[] options );
	public abstract SpaceLayout Layout { get; }

	public IEnumerable<Space> Range( int maxDistance ) => this.CalcDistances( maxDistance ).Keys;

	/// <summary> If adjacent to ocean, sets is-costal </summary>
	public void SetAdjacentToSpaces( params Space[] spaces ) {
		foreach(var land in spaces) {
			Connect( land );
			land.Connect( this );
		}
	}

	public IEnumerable<Space> SpacesExactly( int distance ) { // !!! this should be deprecated or moved to Test project - only used in tests
		return distance switch {
			0 => new Space[] { this },
			1 => adjacent,
			_ => this.CalcDistances( distance ).Where( p => p.Value == distance ).Select( p => p.Key ),
		};
	}

	public override string ToString() => Label;

	void Connect( Space adjacent ) {
		this.adjacent.Add( adjacent );
		if(adjacent.IsOcean)
			this.IsCoastal = true;
	}

	class DisconnectSpaceResults : Restoreable {
		public Space space { get; set; }
		public Space[] OldAdjacents { get; set; }

		public void Restore() {
			space.Board.Add( space, OldAdjacents );
		}
	}

	public virtual Restoreable RemoveFromBoard() {
		return new DisconnectSpaceResults {
			OldAdjacents = Board.Remove(this),
			space = this,
		};
	}

}