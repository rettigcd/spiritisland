namespace SpiritIsland;

/// <summary>
/// An action together with what it targets, offered as ONE choice (the way <see cref="Move"/> offers a token together with where it
/// goes), so a UI can let the user drag the action onto its target, and everything an action could target is visible at once.
/// Only offered when <see cref="Spirit.CombineActionsAndTargets"/> is on; otherwise the action and its target are chosen in two steps.
/// </summary>
/// <param name="Action">A unique, minor or major power, an innate power, or a growth action.</param>
/// <param name="Target">What it targets: a <see cref="Space"/> for a power that targets a land, or a <see cref="Spirit"/> for a power
/// that targets a spirit - and for a growth action, the spirit growing (even though there is only one choice, so that every action
/// has the same form: drop it on its target).</param>
public record TargetedAction( IActionFactory Action, IOption Target ) : IOption {

	string IOption.Text => $"{((IOption)Action).Text} => {Target.Text}";

}
