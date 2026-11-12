namespace SpiritIsland.Agent;

/// <summary>
/// One pending decision as presented to an agent. Option text is unique within a decision (engine
/// guarantee), so it doubles as the option's ID.
/// </summary>
public record DecisionSnapshot( int Version, string Prompt, string[] Options );
