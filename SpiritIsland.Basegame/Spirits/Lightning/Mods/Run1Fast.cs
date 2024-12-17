
namespace SpiritIsland.Basegame;

/// <summary>
/// Resolve one slow, non-Major power during fast phase THIS ROUND ONLY
/// </summary>
/// <param name="spirit"></param>
class Run1SlowNonMajorAsFast(Spirit spirit) : RunSlowCardsAsFastMod_EveryRound(spirit), IEndWhenTimePasses {

	protected override int AllowedCount => 1;

	protected override bool EvaluateAction(IActionFactory action)
		=> base.EvaluateAction(action) && IsNonMajor(action);

	static bool IsNonMajor(IActionFactory slowAction) => slowAction is not PowerCard pc || pc.PowerType != PowerType.Major;

}
