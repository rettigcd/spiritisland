namespace SpiritIsland;

/// <summary>
/// The details to present to the user
/// </summary>
public interface IDecision {
	public string Prompt { get; }
	public IOption[] Options { get; }
}

public interface IDecisionPlus : IDecision {
	public bool AllowAutoSelect { get; }
}
