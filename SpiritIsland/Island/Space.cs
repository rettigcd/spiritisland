namespace SpiritIsland;

public interface Restoreable {
	public void Restore();
}

public abstract class Space 
	: IOption 
	, ISeeAllNeighbors<Space>
	, IEquatable<Space>
	, ILocation // make dragging/moving tokens easier
{

	readonly List<Space> adjacent = [];

	protected Space(string label) {
		this.Label = label;
	}

	public virtual Board[] Boards { get; protected set; }

	public string Label { get; }

	public bool IsSand => Is( Terrain.Sands );

	public bool IsJungle => Is( Terrain.Jungle );

	public bool IsWetland => Is( Terrain.Wetland );

	public bool IsMountain => Is( Terrain.Mountain );	

	public bool IsOcean => Is( Terrain.Ocean );
	public bool IsDestroyed => Is( Terrain.Destroyed );

	public bool IsCoastal { get; set; }

	public override string ToString() => Label;

	public string Text => Label;

	public abstract bool Is( Terrain terrain );
	public abstract bool IsOneOf( params Terrain[] options );

	public abstract int InvaderActionCount { get; }

	public bool DoesExists { get; set; } = true; // absolute Stasis
	/// <summary> Is not in stasis.</summary>
	public static bool Exists( Space space ) => space.DoesExists;

	#region Connectivity
	/// <summary> Existing </summary>
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
	public static implicit operator SpaceState( Space space) => space?.Tokens;

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
			foreach(var board in space.Boards) board.AddSpace( space );
			space.SetAdjacentToSpaces( OldAdjacents );
		}
	}

	public virtual Restoreable RemoveFromBoard() {
		return new DisconnectSpaceResults {
			OldAdjacents = Boards.SelectMany(b=>b.Remove(this)).Distinct().ToArray(),
			space = this,
		};
	}

	#endregion

	#region override Equality
	// Overriding so that when game is rewound and board state is restored, tokens from old spaces appear on new spaces.
	public override int GetHashCode() => Text.GetHashCode();
	public override bool Equals( object obj ) => Equals( obj as Space );
	public bool Equals( Space other ) => other is not null && other.Text == Text;
	static public bool operator==(Space left, Space right) => Object.ReferenceEquals(left,right) || left is not null && left.Equals( right );
	static public bool operator!=(Space left, Space right) => !Object.ReferenceEquals( left, right ) && (left is null || !left.Equals( right ));
	#endregion

	#region Moving

	/// <summary> Triggers IModifyRemoving but does NOT publish TokenRemovedArgs. </summary>
	public Task<(ITokenRemovedArgs,Func<ITokenRemovedArgs,Task>)> SourceAsync( IToken token, int count, RemoveReason reason = RemoveReason.Removed )
		=> Tokens.SourceAsync(token,count,reason);
	public Task<(ITokenAddedArgs, Func<ITokenAddedArgs,Task>)> SinkAsync( IToken token, int count, AddReason addReason = AddReason.Added ) 
		=> Tokens.SinkAsync(token, count, addReason);

	#endregion

}
