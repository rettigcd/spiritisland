#nullable enable
namespace SpiritIsland;

public class Board {

	/// <summary>Non-stasis spaces that are 'InPlay' for current Terrain/Power.</summary>
	public IEnumerable<SpaceSpec> Spaces => Spaces_Existing.Where( x=>TerrainMapper.Current.IsInPlay(x.ScopeSpace) );

	/// <summary>Non-stasis spaces </summary>
	public IEnumerable<SpaceSpec> Spaces_Existing => _spaces.Where( SpaceSpec.Exists );

	/// <summary>All spaces, including the ones that are not in play and are in Stasis.</summary>
	public IEnumerable<SpaceSpec> Spaces_Unfiltered => _spaces;

	public int InvaderActionCount { get; set; } = 1;

	public SingleSpaceSpec Ocean => (SingleSpaceSpec)Spaces_Existing.Single( s => s.IsOcean );
	
	public SpaceSpec this[int index]{ get => _spaces[index]; }

	public BoardSide[] Sides => [.. _sides];

	public BoardOrientation Orientation { get; }

	public BoardLayout Layout => _layout ??= GetTransformedLayout();

	BoardLayout GetTransformedLayout() {
		var layout = BoardLayout.Get(Name);
		layout.ReMap(Orientation.GetTransformMatrix());
		return layout;
	}

	#region constructor

	public Board(
		string name,
		BoardOrientation orientation,
		params SSS[] spaces
	) {
		Name = name;
		Orientation = orientation;
		if( spaces.Length == 0 )
			throw new Exception("Each Board should have 9 spaces but this one has 0.");

		// attach spaces
		_spaces = spaces
			.Select(s => new SingleSpaceSpec(s.terrain, s.label, this, s.startingItems))
			.ToArray();
	}

	#endregion

	public string Name { get; }

	/// <summary>
	/// public so we can build a test board.
	/// </summary>
	public void SetNeighbors(int srcIndex, params int[] neighborIndex){
		_spaces[srcIndex].SetAdjacentToSpaces( neighborIndex.Select(i=>_spaces[i] ).ToArray());
	}

	/// <summary> Used by Weave Together... </summary>
	public void AddSpace(SpaceSpec space) {
		_spaces = _spaces.Union(new[] { space }).ToArray();
	}

	/// <returns> Used by Weave Together... </returns>
	public SpaceSpec[] Remove(SpaceSpec space) {
		var oldAdj = space.Adjacent_Existing.ToArray();
		space.Disconnect();
		_spaces = _spaces.Where(s => s != space).ToArray();
		return oldAdj;
	}

	public BoardSide DefineSide( params int[] spaceIndexes ) {
		var side = new BoardSide( this, spaceIndexes.Select( i => _spaces[i] ).ToArray() );
		_sides.Add( side );
		return side;
	}

	BoardLayout? _layout;
	readonly List<BoardSide> _sides = [];
	SpaceSpec[] _spaces;



}

#nullable disable