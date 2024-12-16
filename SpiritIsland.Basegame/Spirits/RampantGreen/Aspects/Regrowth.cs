
namespace SpiritIsland.Basegame.Spirits.RampantGreen.Aspects;

public class Regrowth : IAspect {

	// https://spiritislandwiki.com/index.php?title=Regrowth

	static public AspectConfigKey ConfigKey => new AspectConfigKey(ASpreadOfRampantGreen.Name, Name);
	public const string Name = "Regrowth";
	public string[] Replaces => [SteadyRegeneration.Name, AllEnvelopingGreen.Name];


	public void ModSpirit(Spirit spirit) {
		// add 13 Destroyed Presence
		spirit.Presence.Destroyed.Count += 13;

		// Remove Steady Regeneration
		var old = spirit.Presence;
		spirit.Presence = new SpiritPresence(spirit, old.Energy, old.CardPlays, old.Token);

		// Swap innates.
		spirit.InnatePowers[1] = InnatePower.For(typeof(UnbelievableGrowth));

		spirit.SpecialRules = [.. spirit.SpecialRules.Where(x => x.Title != SteadyRegeneration.Name)];
	}
}
