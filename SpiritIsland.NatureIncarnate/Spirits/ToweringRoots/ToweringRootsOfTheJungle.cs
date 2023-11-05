namespace SpiritIsland.NatureIncarnate;

public class ToweringRootsOfTheJungle : Spirit {

	public const string Name = "Towering Roots of the Jungle";

	// 3 Innates - stub
	// 4 Cards -stub
	// 5 Incarna
		// Range from Incarna
	// 6 Growth - finish
	// 6 Innates - finish
	// 7 Cards - finish
	// 8 empowered incarna
	// 9 incarna acts as a presence
	public ToweringRootsOfTheJungle() : base(
		new ToweringRootsPresence()
		, PowerCard.For<EntwineTheFatesOfAll>()
		, PowerCard.For<RadiantAndHallowedGrove>()
		, PowerCard.For<BloomingOfTheRocksAndTrees>()
		, PowerCard.For<BoonOfResilientPower>()
	) {
		// Growth
		GrowthTrack = new GrowthTrack(
			new GrowthOption( new ReclaimAll(), new PlacePresence(0) ),
			new GrowthOption( new GainPowerCard(), new PlacePresence(1), new AddVitalityToIncarna() ),
			new GrowthOption( new GainPowerCard(), new PlacePresence(3), new ReplacePresenceWithIncarna(), new GainEnergy(1) )
		);

		// Innates
		InnatePowers = new InnatePower[] {
			InnatePower.For<ShelterUnderToweringBranches>(),
			InnatePower.For<RevokeSanctuaryAndCastOut>()
		};

		PowerRangeCalc = new IncarnaRangeCalculator( this );
	}

	public override string Text => Name;

	public override SpecialRule[] SpecialRules => new SpecialRule[]{
		new SpecialRule("Enduring Vitality", "Some of your Actions Add Vitality Tokens."),
		new SpecialRule("Heart-Tree Guards the Land", "You have an Incarna. Your Powers get +1 range if Incarna is in the origin land.  Invaders/Dahan/Beast can't be damaged or destroyed at Incarna.  Empower Encarna the first time it's in a land with 3 or more vitality.  Skip all Build Actions at empowered Incarna.")
	};

	protected override void InitializeInternal( Board board, GameState gameState ) {
		var highestFirst = board.Spaces.Reverse().Tokens().ToArray();
		// 1 in highest-numbered jungle without blight
		SpaceState jungle = highestFirst.First(x=>x.Space.IsJungle && !x.Blight.Any);
		jungle.Init(Presence.Token,1);
		// 1 in the highest-numbered mountain
		highestFirst
			.First( x => x.Space.IsMountain )
			.Init( Presence.Token, 1 );
		// 1 in the highetst numbered wetland
		highestFirst
			.First( x => x.Space.IsWetland )
			.Init( Presence.Token, 1 );
		// Incarna goes in the jungle with presence
		jungle.Init(Incarna,1); 

	}

	public ToweringRootsIncarna Incarna => ((ToweringRootsPresence)Presence).Incarna;

}
