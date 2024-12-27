namespace SpiritIsland.Horizons;

public class SunBrightWhirlwind : Spirit {

	public const string Name = "Sun Bright Whirlwind";
	public override string SpiritName => Name;

	#region custom Track
	static Track FourEnergyAir => Track.MkEnergy(4, Element.Air);
	static Track FiveCardSun => new Track("5 cardplay sun", Element.Sun) { CardPlay = 5 };
	#endregion custom Track

	#region Stiff Wind Growth

	static SpecialRule AStiffWindAtTheirBacks_Rule => new SpecialRule(
		"A Stiff Wind at Their Backs", 
		"After you Add Presence during Growth, Push up to 1 Explorer/Dahan from that land."
	);

	void AddStiffWind() {
		foreach(var pp in GrowthTrack.GrowthActions.OfType<PlacePresence>() ) {
			pp.Placed.Add(args => this.Target((Space)(args.To)).PushUpTo(1, Human.Explorer, Human.Dahan));
		}
	}

	#endregion Stiff Wind Growth

	public SunBrightWhirlwind():base(
		spirit => new SpiritPresence( spirit,
			new PresenceTrack(Track.Energy1,Track.Energy2,Track.SunEnergy,Track.Energy3,FourEnergyAir,Track.Energy6),
			new PresenceTrack(Track.Card1,Track.Card2,Track.Card3,Track.AirEnergy,Track.Card4,FiveCardSun )
		),
		new GrowthTrack(
			new GrowthGroup( new ReclaimAll(), new GainPowerCard(), new GainEnergy(1) ),
			new GrowthGroup( new PlacePresence(1), new GainEnergy(4) ),
			new GrowthGroup( new GainPowerCard(), new PlacePresence(4) )
		),
		PowerCard.ForDecorated(GiftOfTheSunlitAir.ActAsync),		// Fast
		PowerCard.ForDecorated(GiftOfWindSpedSteps.ActAsync),		// Fast
		PowerCard.ForDecorated(TempestOfLeavesAndBranches.ActAsync),// Fast
		PowerCard.ForDecorated(ScatterToTheWinds.ActAsync)			// Slow
	) {
		InnatePowers = [InnatePower.For(typeof(ViolentWindstorms))];
		SpecialRules = [AStiffWindAtTheirBacks_Rule];
		AddStiffWind();
	}

	protected override void InitializeInternal(Board board, GameState gameState) {
		// Put 3 Presence on your starting board:
		// 1 in the highest-numbered Sands,
		board.Spaces.Last(s => s.IsSand).ScopeSpace.Setup(Presence.Token, 1);
		// 2 in the lowest-numbered Mountain.
		board.Spaces.First(s => s.IsMountain).ScopeSpace.Setup(Presence.Token, 2);
	}

}
