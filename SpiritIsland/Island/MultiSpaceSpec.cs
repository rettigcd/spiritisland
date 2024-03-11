namespace SpiritIsland;

public class MultiSpaceSpec : SpaceSpec {

	public MultiSpaceSpec(params SpaceSpec[] spaces)
		:base( string.Join(":", CollectOrigSpaces(spaces).Select(p=>p.Label)) ) 
	{
		OrigSpaces = CollectOrigSpaces( spaces );
		Boards = OrigSpaces.SelectMany(s=>s.Boards).Distinct().ToArray();
	}

	public override int InvaderActionCount => OrigSpaces.Sum(s=>s.InvaderActionCount) / OrigSpaces.Length;

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