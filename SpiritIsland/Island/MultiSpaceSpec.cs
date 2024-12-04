namespace SpiritIsland;

public class MultiSpaceSpec : SpaceSpec {

	public override SpaceLayout Layout {
		get {
			SingleSpaceSpec[] spaces = OrigSpaces;
			XY[] corners = spaces[0].Layout.Corners;
			for( int i = 1; i < spaces.Length; ++i )
				corners = Polygons.JoinAdjacentPolgons(corners, spaces[i].Layout.Corners);
			return new SpaceLayout(corners);
		}
	}

	public override int InvaderActionCount => OrigSpaces.Sum(s => s.InvaderActionCount) / OrigSpaces.Length;

	#region constructor

	public MultiSpaceSpec(params SpaceSpec[] spaces)
		:base( string.Join(":", CollectOrigSpaces(spaces).Select(p=>p.Label)) ) 
	{
		OrigSpaces = CollectOrigSpaces( spaces );
		Boards = OrigSpaces.SelectMany(s=>s.Boards).Distinct().ToArray();

		// Special case: If we are joining Ocean to anything, it is coastal.
		// We have to do this hear because normal Adjacency check won't be able to detect ocean.
		if(spaces.Any(x=>x.IsOcean) && spaces.Any(x=>!x.IsOcean))
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