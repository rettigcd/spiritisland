
namespace SpiritIsland.FeatherAndFlame;

// Mod that lets Absorbed-Presence spirits use Serpent's Incarna for targeting.
class LocusOfTheSerpentsRegard : IHandleCardPlayed {

	public const string Name = "Locus of the Serpent's Regard";
	const string Description = "Spirits with absorbed Presence can use your Presence at your Incarna for targeting.";
	static public SpecialRule Rule => new SpecialRule(Name, Description);

	public Task Handle(Spirit serpent, PowerCard card) {
		if( card.Title == AbsorbEssence.Name && serpent.Presence is SerpentPresence sp ) {

			// spirit who's presence is absorbed can target using Serpents incarna
			Spirit lastSpirit = sp.AbsorbedPresences.Last();
			if( lastSpirit != serpent && sp.AbsorbedPresences.Count(x => x == lastSpirit) == 1 )
				lastSpirit.TargetingSourceStrategy = new IncludeSerpentsIncarna(lastSpirit.TargetingSourceStrategy, serpent);

		}
		return Task.CompletedTask;
	}

	// New Targetting Source Strategy
	class IncludeSerpentsIncarna(ITargetingSourceStrategy old,Spirit serpent) : ITargetingSourceStrategy {
		public IEnumerable<Space> EvaluateFrom(IKnowSpiritLocations presence, TargetFrom from) {
			var normal = old.EvaluateFrom(presence, from).ToList();
			if( serpent.Incarna.IsPlaced && from switch {
				TargetFrom.SacredSite => serpent.Incarna.AsSpaceToken().IsSacredSite,
				_ => true
			} ) {
				normal.Add(serpent.Incarna.Space);
			}
			return normal;
		}
	}

}


// Replaces Innate Power: Serpent Wakes in Power
// Complexity  Higher Complexity
// Innate Name Strength of the Waking Island
// Speed   Fast — Fast
// Range   N/A
// Target  Yourself
// Innate Thresholds	2 Water 1 Earth — 2 Water, 1 Earth
// After a Spirit uses a land-targeting Power Card, they may Repeat it at your Incarna by paying its cost. (Max. 1 Power Card per turn for each of their absorbed Presence.) You may help pay some or all of this cost. (These Repeats ignore Range and target requirements.)

// 2 Moon 2 Earth 1 Plant — 2 Moon, 2 Earth, 1 Plant
// Add 1 Vitality at your Incarna. If a Spirit has 2 or more absorbed Presence, you may instead Add 1 Vitality in one of their lands without Blight.
