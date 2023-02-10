namespace SpiritIsland.Select;

public class Arrow {
	public IToken Token;
	public Space From;
	public Space To;
}

public interface IHaveArrows {
	IEnumerable<Arrow> Arrows { get; } 
}
