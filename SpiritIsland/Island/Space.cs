namespace SpiritIsland;

public interface Restoreable {
	public void Restore();
}

public abstract class Space 
	: IOption 
	, ISeeAllNeighbors<Space>
{

	readonly List<Space> adjacent = new List<Space>();
	Board _board;

	protected Space(string label) {
		this.Label = label;
	}

	public Board Board {
		get => _board;
		set { if(_board != null) throw new InvalidOperationException( "cannot set board twice" ); _board = value; }
	}

	public string Label { get; }

	public bool IsSand => Is( Terrain.Sand );

	public bool IsJungle => Is( Terrain.Jungle );

	public bool IsWetland => Is( Terrain.Wetland );

	public bool IsMountain => Is( Terrain.Mountain );	

	public bool IsOcean => Is( Terrain.Ocean );

	public bool IsCoastal { get; set; }

	public override string ToString() => Label;

	public string Text => Label;

	public abstract bool Is( Terrain terrain );
	public abstract bool IsOneOf( params Terrain[] options );
	public abstract SpaceLayout Layout { get; }

	//public bool InStasis { get => !Exists; }
	public bool DoesExists { get; set; } = true;
	public static bool Exists( Space space ) => space.DoesExists;

	#region Connectivity
	public IEnumerable<Space> Adjacent_Existing => adjacent.Where(Exists);

	public IEnumerable<Space> Range( int maxDistance ) => this.CalcDistances( maxDistance ).Keys;

	#endregion

	/// <summary> If adjacent to ocean, sets is-coastal </summary>
	public void SetAdjacentToSpaces( params Space[] spaces ) {
		foreach(var land in spaces) {
			Connect( land );
			land.Connect( this );
		}
	}

	public SpaceState Tokens => ActionScope.Current.AccessTokens(this);
	public static implicit operator SpaceState( Space space) => space.Tokens;

	#region Connect / Disconnect

	void Connect( Space adjacent ) {
		this.adjacent.Add( adjacent );
		if(adjacent.IsOcean)
			this.IsCoastal = true;
	}
	public void Disconnect() {
		// Remove us from neighbors adjacent list
		foreach(var a in adjacent)
			a.adjacent.Remove( this );
		// Remove neighbors from our list.
		adjacent.Clear();
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

	#endregion

}