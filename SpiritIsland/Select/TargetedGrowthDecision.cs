namespace SpiritIsland.A;

/// <summary>
/// The Growth decision when actions and targets are combined (<see cref="Spirit.CombineActionsAndTargets"/>): each choice is a growth
/// action together with the spirit it grows (<see cref="TargetedAction"/>). Like <see cref="GrowthDecision"/>, it lets a UI know it is
/// the growth choice.
/// </summary>
public class TargetedGrowthDecision( string prompt, IEnumerable<IOption> options, Present present )
	: TypedDecision<IOption>( prompt, options, present )
{
}
