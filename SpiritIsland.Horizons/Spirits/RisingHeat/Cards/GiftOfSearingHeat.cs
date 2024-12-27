namespace SpiritIsland.Horizons;

public class GiftOfSearingHeat {

	public const string Name = "Gift of Searing Heat";

	[SpiritCard(Name, 0, Element.Sun,Element.Fire,Element.Air), Fast, AnotherSpirit]
	[Instructions("Target Spirit gains 2 Energy --or-- Target Spirit may pay 1 Energy to do 1 Damage in one of their lands."), Artist(Artists.LucasDurham)]
	static public Task ActAsync(TargetSpiritCtx ctx) {
		
		return Cmd.Pick1(
			// Target Spirit gains 2 Energy
			new SpiritAction("Gain 2 Energy.", spirit => spirit.Energy += 2),
			// --or--
			// Target Spirit may pay 1 Energy to do 1 Damage in one of their lands.
			PayForDamage
		).ActAsync(ctx.Other);
	}

	static SpiritAction PayForDamage => new SpiritAction(
		"Pay 1 Energy to do 1 Damage in one of their lands.", 
		spirit => {
			--spirit.Energy;
			return Cmd.DamageInvaders(1).In().SpiritPickedLand().Which(Has.YourPresence).ActAsync(spirit);
		}
	).OnlyExecuteIf(s=>1<=s.Energy);

}
