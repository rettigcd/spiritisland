namespace SpiritIsland;

public interface IRestoreable {
	public void Restore();
}

public abstract class SpaceSpec 
	: IOption 
	, ISeeAllNeighbors<SpaceSpec>
	, IEquatable<SpaceSpec>
{

	readonly List<SpaceSpec> adjacent = [];

	protected SpaceSpec(string label) {
		Label = label;
	}

	public abstract SpaceLayout Layout { get; }

	public virtual Board[] Boards { get; protected set; }

	string IOption.Text => Label;
	public string Label { get; }

	public bool IsSand => Is( Terrain.Sands );

	public bool IsJungle => Is( Terrain.Jungle );

	public bool IsWetland => Is( Terrain.Wetland );

	public bool IsMountain => Is( Terrain.Mountain );	

	public bool IsOcean => Is( Terrain.Ocean );
	public bool IsDestroyed => Is( Terrain.Destroyed );

	public bool IsCoastal { get; set; }

	public override string ToString() => Label;

	public abstract bool Is( Terrain terrain );
	public abstract bool IsOneOf( params Terrain[] options );

	public abstract int InvaderActionCount { get; }

	public bool DoesExists { get; set; } = true; // absolute Stasis
	/// <summary> Is not in stasis.</summary>
	public static bool Exists( SpaceSpec space ) => space.DoesExists;

	#region Connectivity
	/// <summary> Existing </summary>
	public IEnumerable<SpaceSpec> Adjacent_Existing => adjacent.Where(Exists);

	public IEnumerable<SpaceSpec> Range( int maxDistance ) => this.CalcDistances( maxDistance ).Keys;

	#endregion

	/// <summary> If adjacent to ocean, sets is-coastal </summary>
	public void SetAdjacentToSpaces( params SpaceSpec[] spaces ) {
		foreach(var land in spaces) {
			Connect( land );
			land.Connect( this );
		}
	}

	/// <summary> Used the Current ActionScope to get the tokens </summary>
	public Space ScopeSpace => ActionScope.Current.AccessTokens(this);

	//  This is not safe for UI:  	public static implicit operator Space( Space space ) => space?.Tokens;

	#region Connect / Disconnect

	void Connect( SpaceSpec adjacent ) {
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

	class DisconnectSpaceResults : IRestoreable {
		public SpaceSpec Space { get; set; }
		public SpaceSpec[] OldAdjacents { get; set; }

		public void Restore() {
			foreach(var board in Space.Boards) board.AddSpace( Space );
			Space.SetAdjacentToSpaces( OldAdjacents );
		}
	}

	public virtual IRestoreable RemoveFromBoard() {
		return new DisconnectSpaceResults {
			OldAdjacents = Boards.SelectMany(b=>b.Remove(this)).Distinct().ToArray(),
			Space = this,
		};
	}

	#endregion

	#region override Equality
	// Overriding so that when game is rewound and board state is restored, tokens from old spaces appear on new spaces.
	public override int GetHashCode() => Label.GetHashCode();
	public override bool Equals( object obj ) => Equals( obj as SpaceSpec );
	public bool Equals( SpaceSpec other ) => other is not null && other.Label == Label;
	static public bool operator==(SpaceSpec left, SpaceSpec right) => Object.ReferenceEquals(left,right) || left is not null && left.Equals( right );
	static public bool operator!=(SpaceSpec left, SpaceSpec right) => !Object.ReferenceEquals( left, right ) && (left is null || !left.Equals( right ));
	#endregion

	#region Moving

	/// <summary> Triggers IModifyRemoving but does NOT publish TokenRemovedArgs. </summary>
	public Task<(ITokenRemovedArgs,Func<ITokenRemovedArgs,Task>)> SourceAsync( IToken token, int count, RemoveReason reason = RemoveReason.Removed )
		=> ScopeSpace.SourceAsync(token,count,reason);
	public Task<(ITokenAddedArgs, Func<ITokenAddedArgs,Task>)> SinkAsync( IToken token, int count, AddReason addReason = AddReason.Added ) 
		=> ScopeSpace.SinkAsync(token, count, addReason);

	#endregion

}
