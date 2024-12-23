namespace SpiritIsland.FeatherAndFlame;

[InnatePower(Name), Fast, Yourself]
public class StrengthOfTheWakingIsland {

	public const string Name = "Strength of the Waking Island";

	// After a Spirit uses a land-targeting Power Card, they may Repeat it at your Incarna by paying its cost. (Max. 1 Power Card per turn for each of their absorbed Presence.) You may help pay some or all of this cost. (These Repeats ignore Range and target requirements.)
	[InnateTier("2 water,1 earth", "-Not yet implemented.- After a Spirit uses a land-targeting Power Card, they may Repeat it at your Incarna by paying its cost. (Max. 1 Power Card per turn for each of their absorbed Presence.) You may help pay some or all of this cost. (These Repeats ignore Range and target requirements.)")]
	static public Task Option1Async(Spirit _) {
		// After a Spirit uses a land-targeting Power Card, they may Repeat it at your Incarna by paying its cost.
		// (Max. 1 Power Card per turn for each of their absorbed Presence.)
		// You may help pay some or all of this cost.
		// (These Repeats ignore Range and target requirements.)
		return Task.CompletedTask;
	}

	// 2 Moon 2 Earth 1 Plant — 2 Moon, 2 Earth, 1 Plant
	// Add 1 Vitality at your Incarna. If a Spirit has 2 or more absorbed Presence, you may instead Add 1 Vitality in one of their lands without Blight.
	[InnateTier("2 moon,2 earth,1 plant", "Add 1 Vitality at your Incarna. If a Spirit has 2 or more absorbed Presence, you may instead Add 1 Vitality in one of their lands without Blight.",1)]
	static public async Task Option2Async(Spirit self) {
		// Add 1 Vitality
		var spaces = new List<Space>();
		// at your Incarna.
		if( self.Incarna.IsPlaced)
			spaces.Add(self.Incarna.Space);

		// If a Spirit has 2 or more absorbed Presence,
		var ss = (SerpentPresence)self.Presence;
		var absorbed = new CountDictionary<Spirit>(ss.AbsorbedPresences);
		foreach(var spirit in absorbed.Keys ) {
			if(absorbed[spirit]<2) continue;
			// you may instead Add 1 Vitality in one of their lands without Blight.
			spaces.AddRange(spirit.Presence.Lands.Where(s => !s.Blight.Any));
		}
		if(spaces.Count == 0) return;
		var space = await self.Select("Add Vitality", spaces.Distinct().OrderBy(x=>x.Label),Present.Done);
		if(space is not null)
			await space.Vitality.AddAsync(1);
	}

}
