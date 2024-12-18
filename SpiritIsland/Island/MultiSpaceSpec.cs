#nullable enable
namespace SpiritIsland;

public class MultiSpaceSpec : SpaceSpec {

	#region Layout

	/// <summary>
	/// Lazy loading this. Not needed for most unit tests.
	/// </summary>
	public override SpaceLayout Layout => _layout ??= CalcLayout(OrigSpaces);
	SpaceLayout? _layout = null;
	static public SpaceLayout CalcLayout(SpaceSpec[] spaces) {
		XY[] corners = spaces[0].Layout.Corners;
		for( int i = 1; i < spaces.Length; ++i )
			corners = Polygons.JoinAdjacentPolgons(corners, spaces[i].Layout.Corners);
		return new SpaceLayout(corners);
	}

	#endregion Layout

	public override int InvaderActionCount => OrigSpaces.Sum(s => s.InvaderActionCount) / OrigSpaces.Length;

	#region constructor

	static public MultiSpaceSpec Build( params SpaceSpec[] spaces) {
		var origSpaces = CollectOrigSpaces(spaces);
		string label = string.Join(":", origSpaces.Select(p => p.Label) );
		var boards = origSpaces.SelectMany(bob => bob.Boards).Distinct().ToArray();
		return new MultiSpaceSpec(origSpaces,label,boards);
	}

	MultiSpaceSpec( SingleSpaceSpec[] origSpaces, string label, Board[] boards )
		:base( label, boards ) 
	{
		OrigSpaces = origSpaces;
		// Special case: If we are joining Ocean to anything, it is coastal.
		// We have to do this hear because normal Adjacency check won't be able to detect ocean.
		if(origSpaces.Any(x=>x.IsOcean) && origSpaces.Any(x=>!x.IsOcean))
			IsCoastal = true;
	}

	#endregion

	static SingleSpaceSpec[] CollectOrigSpaces( SpaceSpec[] spaces ) {
		List<SingleSpaceSpec> parts = [];
		foreach(var space in spaces)
			if(space is SingleSpaceSpec one)
				parts.Add( one );
			else if(space is MultiSpaceSpec many)
				parts.AddRange( many.OrigSpaces );
		return [..parts.OrderBy(p=>p.Label)];
	}

	public override bool Is( Terrain terrain ) => OrigSpaces.Any(part => part.Is(terrain));
	public override bool IsOneOf( params Terrain[] options ) => OrigSpaces.Any(part => part.IsOneOf(options));

	public SingleSpaceSpec[] OrigSpaces { get; }

	public void AddToBoardsAndSetAdjacent(IEnumerable<SpaceSpec> adjacents) {
		foreach(var board in Boards)
			board.AddSpace( this );

		SetAdjacentToSpaces( adjacents.ToArray() );
	}

}