using SpiritIsland.Basegame;
using SpiritIsland.BranchAndClaw;
using SpiritIsland.JaggedEarth;

#if DEBUG

namespace SpiritIsland.Maui.ManualTesting;

internal class SchoolSpirit : Spirit {

	public override string SpiritName => Name;

	public const string Name = "river surges in sunlight";
	// public const string Name = "School Spirit";

	#region constructors

	/// <summary> Spirit with Energy and Card Plays, but unknown Cards </summary>
	public SchoolSpirit()
		: base(x => new SpiritPresence(x,
					new TestPresenceTrack(Track.Energy9, Track.SunEnergy, Track.Energy9, Track.Energy9, Track.Energy9),
					new TestPresenceTrack(Track.Card2, Track.Card2, Track.Card2, Track.Card2, Track.Card2)
				)
			, new GrowthTrack(new GrowthGroup(new ReclaimAll(),new PlacePresence(2)))
			, PowerCard.For(typeof(InfiniteVitality))
			, PowerCard.For(typeof(DreamOfTheUntouchedLand))
			, PowerCard.For(typeof(WeaveTogetherTheFabricOfPlace))
			, PowerCard.For(typeof(CastDownIntoTheBrinyDeep))
			, PowerCard.For(typeof(UncannyMelting))
			, PowerCard.For(typeof(SettleIntoHuntingGrounds))
		) {
		Energy=20;
		InnatePowers = [InnatePower.For(typeof(PepRally))];
	}

	#endregion constructors

	protected override void InitializeInternal(Board board, GameState gameState) {
		// Has sacred site on space 5
		board[5].ScopeSpace.Setup(Presence.Token,2);
	}

	class TestPresenceTrack(params Track[] t) : PresenceTrack(t) {
		public void OverrideTrack(int index, Track t) { _slots[index] = t; }

	}

}

[InnatePower(Name), Fast, AnySpirit]
public class PepRally {

	public const string Name = "Pep Rally";

	[InnateTier("1 sun", "Target Spirit gains 5 of each element.")]
	static public Task Option1Async(TargetSpiritCtx ctx) {
		CountDictionary<Element> bob = ElementStrings.Parse("5 sun,5 moon,5 air,5 fire,5 water,5 plant,5 earth,5 animal");
		ctx.Other.Elements.Add(bob);
		var ted = ctx.Other.Elements.Summary();
		return Task.CompletedTask;
	}

}

#endif