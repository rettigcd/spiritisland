namespace SpiritIsland.Select;

public class AdjacentInfo { 
	public AdjacentDirection Direction { get; set; }

	public SpiritIsland.Space Original { get; set; }

	public SpiritIsland.Space[] Adjacent { get; set; }
}

public interface IHaveAdjacentInfo {
	AdjacentInfo AdjacentInfo { get; }
}

public enum AdjacentDirection { None, Incoming, Outgoing }
