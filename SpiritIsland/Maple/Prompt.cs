namespace SpiritIsland;

public class Prompt {
	/// <summary> Does not require MaxCount  </summary>
	static public Func<PromptData,string> RemainingParts(string prefix) => (x) => $"{prefix} ({x.RemainingPartsStr})";
	/// <summary> Requires non-null MaxCount  </summary>
	static public Func<PromptData,string> RemainingCount(string prefix) => (x) => $"{prefix} ({x.RemainingCount} remaining)";
	/// <summary> Requires non-null MaxCount  </summary>
	static public Func<PromptData,string> XofY(string prefix) => (x) => $"{prefix} ({x.Index+1} of {x.MaxCount})";
}

