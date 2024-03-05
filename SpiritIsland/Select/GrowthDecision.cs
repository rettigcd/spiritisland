namespace SpiritIsland.A;

/// <summary>
/// this Special class makes it easier for UI to know when to display Growth Option pop-up
/// </summary>
public class GrowthDecision(string prompt, IActionFactory[] options, Present present) 
	: TypedDecision<IActionFactory>(prompt, options, present)
{
}
