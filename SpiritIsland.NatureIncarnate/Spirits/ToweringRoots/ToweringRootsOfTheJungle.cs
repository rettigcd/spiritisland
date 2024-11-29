namespace SpiritIsland.NatureIncarnate;

// Roots
// S:	E Incarna in Town land
// A:	F,B,A
// B:	G,C,H
// C:	D

public class ToweringRootsOfTheJungle : Spirit {

	public const string Name = "Towering Roots of the Jungle";

	public ToweringRootsOfTheJungle() : base(
		spirit => new SpiritPresence(spirit,
			new PresenceTrack( Track.Energy1, Track.Energy2, Track.EarthEnergy, Track.Energy4, Track.PlantEnergy, Track.Energy6 ),
			new PresenceTrack( Track.Card1, Track.Card2, Track.SunEnergy, Track.Card3, Track.PlantEnergy, Track.Card4 ),
			new ToweringRootsIncarna( spirit )
		)
		, new GrowthTrack(
			new GrowthGroup( new ReclaimAll(), new PlacePresence( 0 ) ),
			new GrowthGroup( new GainPowerCard(), new PlacePresence( 1 ), new AddVitalityToIncarna() ),
			new GrowthGroup( new GainPowerCard(), new PlacePresence( 3 ), new ReplacePresenceWithIncarna(), new GainEnergy( 1 ) )
		)
		, PowerCard.For(typeof(RadiantAndHallowedGrove))
		, PowerCard.For(typeof(EntwineTheFatesOfAll))
		, PowerCard.For(typeof(BloomingOfTheRocksAndTrees))
		, PowerCard.For(typeof(BoonOfResilientPower))
	) {
		// Innates
		InnatePowers = [
			InnatePower.For(typeof(ShelterUnderToweringBranches)), 
			InnatePower.For(typeof(RevokeSanctuaryAndCastOut))
		];
		SpecialRules = [EnduringVitality, HeartTree];
		PowerRangeCalc = new IncarnaRangeCalculator( this );
	}

	public override string SpiritName => Name;

	static readonly SpecialRule EnduringVitality = new SpecialRule("Enduring Vitality", "Some of your Actions Add Vitality Tokens.");
	static readonly SpecialRule HeartTree = new SpecialRule("Heart-Tree Guards the Land", "You have an Incarna. Your Powers get +1 range if Incarna is in the origin land.  Invaders/Dahan/Beast can't be damaged or destroyed at Incarna.  Empower Encarna the first time it's in a land with 3 or more vitality.  Skip all Build Actions at empowered Incarna.");

	protected override void InitializeInternal( Board board, GameState gameState ) {
		var highestFirst = board.Spaces.Reverse().ScopeTokens().ToArray();
		// 1 in highest-numbered jungle without blight
		Space jungle = highestFirst.First(x=>x.SpaceSpec.IsJungle && !x.Blight.Any);
		jungle.Init(Presence.Token,1);
		// 1 in the highest-numbered mountain
		highestFirst
			.First( x => x.SpaceSpec.IsMountain )
			.Init( Presence.Token, 1 );
		// 1 in the highetst numbered wetland
		highestFirst
			.First( x => x.SpaceSpec.IsWetland )
			.Init( Presence.Token, 1 );
		// Incarna goes in the jungle with presence
		jungle.Init(Incarna,1); 

	}

}
