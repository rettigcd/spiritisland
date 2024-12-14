
namespace SpiritIsland.JaggedEarth;

public class ExplosiveInnate : InnatePower {

	public ExplosiveInnate(Type actionType) : base(actionType) {}

	protected override Task<bool> HasMetTierThreshold(Spirit spirit, IDrawableInnateTier option) {
		return option is ExplosiveInnateOptionAttribute explosiveAttribute
			&& VolcanoPresence.GetPresenceDestroyedThisAction() < explosiveAttribute.DestroyedPresenceThreshold
			? Task.FromResult(false)
			: base.HasMetTierThreshold(spirit, option);
	}

}