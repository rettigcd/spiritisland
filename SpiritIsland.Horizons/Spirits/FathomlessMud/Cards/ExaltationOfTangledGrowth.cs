namespace SpiritIsland.Horizons;

public class ExaltationOfTangledGrowth {

	public const string Name = "Exaltation of Tangled Growth";

	[SpiritCard(Name, 0, Element.Water, Element.Earth, Element.Plant), Slow, AnotherSpirit]
	[Instructions("Target Spirit may pay 1 Energy to gain a Power Card. You may pay 2 Energy to gain a Power Card."), Artist(Artists.MoroRogers)]
	static public async Task ActAsync(TargetSpiritCtx ctx) {
		// Target Spirit may pay 1 Energy to gain a Power Card.
		await PayToDrawCard(ctx.Other, 1);
		// You may pay 2 Energy to gain a Power Card.
		await PayToDrawCard(ctx.Self, 2);
	}

	static async Task PayToDrawCard(Spirit spirit, int energyRequired) {
		if( 1 <= spirit.Energy 
			&& await spirit.UserSelectsFirstText($"Pay {energyRequired} energy to gain Power Card", "Yes", "No") 
		) {
			spirit.Energy -= energyRequired;
			await spirit.Draw.Card();
		}
	}
}
