namespace SpiritIsland.Basegame.Spirits.Lightning.Aspects;

public class Immense : IAspect {

	// https://spiritislandwiki.com/index.php?title=Immense

	static public AspectConfigKey ConfigKey => new AspectConfigKey(LightningsSwiftStrike.Name, Name);
	public const string Name = "Immense";
	public string[] Replaces => []; // Changes Track Presence

	public void ModSpirit(Spirit spirit) {

		// Your top Presence track grants you twice as much Energy during the Spirit Phase.
		// (Any ongoing modifiers are counted after doubling.)
		var p = spirit.Presence;
		foreach( Track t in spirit.Presence.Energy.Slots )
			if( t.Energy.HasValue ) {
				int newEnergy = t.Energy.Value * 2;
				t.Energy = newEnergy;
				t.Code = Track.EnergyOnlyCode(newEnergy);
			}

		// Playing Power Cards during the Spirit Phase costs 2 Card Plays instead of 1.
		// (So if you have 3 Card Plays, you will only be able to play 1 Power Card.)
		// If you have any unused Card Plays at the end of the Spirit Phase, gain 1 Element of your choice.
		var any = Track.AnyEnergy;
		int lastOrigCardPlays = -1;
		foreach( Track t in spirit.Presence.CardPlays.Slots ) {
			int orig = (int)t.CardPlay!; // all of Lightnings Card Plays have values
			int newPlays = orig / 2;
			// Half as many Card plays
			t.CardPlay = newPlays; // round down
			t.Code = Track.CardPlayOnlyCode(newPlays);

			if(orig != lastOrigCardPlays ) {
				lastOrigCardPlays = orig;
				// if this is the 1st odd card, switch to Any Element
				if( orig % 2 == 1 ) {
					t.Code = any.Code;
					t.Elements = any.Elements;
					t.Icon = any.Icon;
					t.CardPlay = null;
				}
			}
		}

		// For each Major Power you play, gain 2 different Elements of your choice.
		spirit.Mods.Add(new TwoElementsForMajorCards());

		spirit.SpecialRules = [.. spirit.SpecialRules, ImmenseRule];
	}

	static SpecialRule ImmenseRule => new SpecialRule(
		"An Immense Spirit, Towering and Slow",
		"Your energy Presence track has been doubled. • Card plays have been cut in half. Original odd Card plays gain Element of players choice. • For each Major Power you play, gain 2 different Elements of your choice."
	);

	class TwoElementsForMajorCards : IHandleCardPlayed {
		Task IHandleCardPlayed.Handle(Spirit spirit, PowerCard card) {
			if( card.PowerType == PowerType.Major )
				spirit.Elements.Add(Element.Any, Element.Any);
			return Task.CompletedTask;
		}
	}
}