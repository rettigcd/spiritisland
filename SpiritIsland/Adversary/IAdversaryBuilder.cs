#nullable enable
namespace SpiritIsland;

public interface IAdversaryBuilder {
	string Name { get; }
	AdversaryLevel[] Levels { get; }
	AdversaryLossCondition? LossCondition { get; }
	IAdversary Build(int level);
}
