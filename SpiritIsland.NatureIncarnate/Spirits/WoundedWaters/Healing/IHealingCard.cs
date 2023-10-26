namespace SpiritIsland.NatureIncarnate;

interface IHealingCard : IOption {
	bool MeetsRequirement( WoundedWatersBleeding spirit );
	void Claim( WoundedWatersBleeding spirit );
	bool IsClaimed( WoundedWatersBleeding spirit );
}
