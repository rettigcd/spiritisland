namespace SpiritIsland.Select;

public class Arrow {
	public IVisibleToken Token;
	public SpiritIsland.Space From;
	public SpiritIsland.Space To;
}

public interface IHaveArrows {
	IEnumerable<Arrow> Arrows { get; } 
}
