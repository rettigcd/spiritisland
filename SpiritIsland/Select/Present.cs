namespace SpiritIsland;

/// <summary>
/// How a decision is presented to the user.
/// </summary>
public enum Present {
	/// <summary> 
	/// Shows for 1 or more items - User must select something
	/// 0 items => returns null.
	/// </summary>
	Always,

	/// <summary>
	/// If 1, autoselects it - not present user with a choice.
	/// Otherwise reverts to 'Always'.
	/// </summary>
	AutoSelectSingle,

	/// <summary>
	/// Shows for 1 or more items.
	/// Has 'Done' option making selection optional/cancelable.
	/// </summary>
	Done,
}