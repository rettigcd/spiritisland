namespace SpiritIsland;

public enum Present {
	/// <summary> Shows for 1 or more items.</summary>
	Always,

	/// <summary> If 1, autoselects it.  Otherwise reverts to 'Always'.</summary>
	AutoSelectSingle,

	/// <summary>
	/// Shows for 1 or more items, plus has done, cancel
	/// </summary>
	Done,
}