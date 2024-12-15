namespace SpiritIsland.Basegame;

[InnatePower(Name), Fast, Yourself]
public class EarthMovesWithVigorAndMight {

	public const string Name = "Earth Moves with Vigor and Might";

	static public void InitAspect(Spirit spirit) {
		spirit.InnatePowers = [.. spirit.InnatePowers, InnatePower.For(typeof(EarthMovesWithVigorAndMight))];
	}

	[InnateTier("1 plant", "You may play an additional Power Card by paying 1 Energy plus its cost.", 0)]
	static public Task Option1(Spirit spirit) {
		// You may play an additional Power Card by paying 1 Energy plus its cost.
		// (Its Elements apply for the rest of this Innate Power's thresholds.)
		return new PlayCardForCost(Present.Done,1).ActAsync(spirit);
	}

	[InnateTier("1 sun,2 earth", "You do +1 Damage with each Damage-dealing Power you use this turn.", 1)]
	static public Task Option2(Spirit spirit) {
		// You do +1 Damage with each Damage-dealing Power you use this turn.
		++spirit.BonusDamage;
		return Task.CompletedTask;
	}

	[InnateTier("2 plant,3 earth", "You do +1 Damage with each Damage-dealing Power you use this turn.", 2)]
	static public Task Option3(Spirit spirit) {
		// You do +1 Damage with each Damage-dealing Power you use this turn.
		++spirit.BonusDamage;
		return Task.CompletedTask;
	}

	[InnateTier("1 sun,3 plant", "Gain a Power Card.", 3)]
	static public Task Option4(Spirit spirit) {
		// Gain a Power Card.
		return spirit.Draw.Card();
	}

}