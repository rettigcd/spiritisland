namespace SpiritIsland.A;
using Orig_Space = SpiritIsland.SpaceSpec;

public class Arrow {
	public IToken Token;
	public Orig_Space From;
	public Orig_Space To;
}

public interface IHaveArrows {
	IEnumerable<Arrow> Arrows { get; } 
}
