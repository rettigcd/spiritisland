namespace SpiritIsland.Select;

public class AdjacentInfo { 
	public AdjacentDirection Direction { get; set; }

	/// <summary> The common spot the arrows point to/from. </summary>
	public SpiritIsland.Space Central { get; set; }

	/// <summary> The many spots the arrows point to/from. </summary>
	public SpiritIsland.Space[] Adjacent { get; set; }
}

public interface IHaveTokenInfo {
	Token Token { get; }
}

public interface IHaveAdjacentInfo {
	AdjacentInfo AdjacentInfo { get; }
}

public enum AdjacentDirection { None, Incoming, Outgoing }
