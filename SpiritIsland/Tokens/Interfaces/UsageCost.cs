namespace SpiritIsland;

/// <summary> Used to order which stops to use/present first </summary>
public enum UsageCost {

	/// <summary>
	/// Most Skips are Free, because the space has already been assigned and it is a use it or lose it.
	/// </summary>
	Free = 0,
	
	/// <summary>
	/// Almost free but not quite. (not currently used)
	/// </summary>
	AlmostFree = 1,

	/// <summary>
	/// Using it consumes some minor resource or opportunity.
	/// </summary>
	Something = 2,

	/// <summary>
	/// Using it consumes some major resource or opportunity.
	/// </summary>
	Heavy = 3,

	/// <summary>
	/// Least desirable. For example, destroys presence. 
	/// </summary>
	Extreme = 4,
}

