namespace SpiritIsland;

abstract public class AdversaryBuilder(string name) : IAdversaryBuilder {

	string IAdversaryBuilder.Name => name;

	// Get: Available Levels
	public abstract AdversaryLevel[] Levels { get; }

	public virtual AdversaryLossCondition? LossCondition => null;

	public IAdversary Build(int level) => new Adversary(this, level);
}
