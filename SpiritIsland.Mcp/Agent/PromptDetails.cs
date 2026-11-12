using System.Text.RegularExpressions;

namespace SpiritIsland.Agent;

/// <summary>
/// Spells out prompts whose text packs in unlabelled numbers, by matching the engine's prompt wording.
/// If the engine's wording changes the match fails quietly, so AgentGame_Tests checks it against a real prompt.
/// </summary>
public static partial class PromptDetails {

	/// <summary> Null when the prompt has nothing to spell out. </summary>
	public static string? For( string prompt ) {
		Match play = PlayPowerCard().Match( prompt );
		return play.Success
			? $"Energy: {play.Groups["energy"].Value}. Card plays left: {play.Groups["plays"].Value}."
			: null;
	}

	// Spirit.SelectAndPlay1: $"Play power card (${Energy} / {remainingToPlay})"
	[GeneratedRegex( @"^Play power card \(\$(?<energy>\d+) / (?<plays>\d+)\)$" )]
	private static partial Regex PlayPowerCard();
}
