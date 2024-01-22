namespace SpiritIsland;

public class MultiSpace : Space {

	public MultiSpace(params Space[] spaces)
		:base( string.Join(":", CollectOrigSpaces(spaces).Select(p=>p.Label)) ) 
	{
		OrigSpaces = CollectOrigSpaces( spaces );
		Boards = OrigSpaces.SelectMany(s=>s.Boards).Distinct().ToArray();
	}

	public override int InvaderActionCount => OrigSpaces.Sum(s=>s.InvaderActionCount) / OrigSpaces.Length;

	static Space1[] CollectOrigSpaces( Space[] spaces ) {
		var parts = new List<Space1>();
		foreach(var space in spaces)
			if(space is Space1 one)
				parts.Add( one );
			else if(space is MultiSpace many)
				parts.AddRange( many.OrigSpaces );
		return [..parts.OrderBy(p=>p.Text)];
	}

	public override bool Is( Terrain terrain ) => OrigSpaces.Any(part => part.Is(terrain));
	public override bool IsOneOf( params Terrain[] options ) => OrigSpaces.Any(part => part.IsOneOf(options));

	public Space1[] OrigSpaces { get; }

	public void AddToBoardsAndSetAdjacent(IEnumerable<Space> adjacents) {
		foreach(var board in Boards)
			board.AddSpace( this );

		SetAdjacentToSpaces( adjacents.ToArray() );

	}

}