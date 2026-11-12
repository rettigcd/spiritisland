using System.Text.Json.Nodes;

namespace SpiritIsland.Agent;

/// <summary>
/// One pending decision as presented to an agent. Option text is unique within a decision (engine
/// guarantee), so it doubles as the option's ID.
/// </summary>
/// <param name="Alerts">Game log entries (other than decisions made) recorded since the previous decision.</param>
/// <param name="OptionDetails">Option ID -> what the option is, for options whose text alone isn't enough to choose (e.g. a card's elements and instructions).</param>
/// <param name="PromptDetails">The prompt's packed-in numbers spelled out, when the prompt has any (see <see cref="PromptDetails"/>).</param>
public record DecisionSnapshot( int Version, string Prompt, string[] Options, Alert[] Alerts, IReadOnlyDictionary<string, JsonObject> OptionDetails, string? PromptDetails = null );

/// <summary> A game log entry, with the same grouping the Maui log view uses for its colors. </summary>
public record Alert( string Kind, string Text );
