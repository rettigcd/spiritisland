namespace SpiritIsland.A;
using Orig_Space = SpiritIsland.SpaceSpec;

public class Arrow {
	public required IToken Token;
	public required Orig_Space From;
	public required Orig_Space To;
}

public interface IHaveArrows {
	IEnumerable<Arrow> Arrows { get; } 
}
