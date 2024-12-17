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
			new GrowthGroup( new GainPowerCard(), new PlacePresence( 1 ), new EnduringVitality() ),
			new GrowthGroup( new GainPowerCard(), new PlacePresence( 3 ), new ReplacePresenceWithIncarna(), new GainEnergy( 1 ) )
		)
		, PowerCard.ForDecorated(RadiantAndHallowedGrove.ActAsync)
		, PowerCard.ForDecorated(EntwineTheFatesOfAll.ActAsync)
		, PowerCard.ForDecorated(BloomingOfTheRocksAndTrees.ActAsync)
		, PowerCard.ForDecorated(BoonOfResilientPower.ActAsync)
	) {
		InnatePowers = [
			InnatePower.For(typeof(ShelterUnderToweringBranches)), 
			InnatePower.For(typeof(RevokeSanctuaryAndCastOut))
		];
		SpecialRules = [EnduringVitality.Rule, HeartTree.Rule];
		PowerRangeCalc = new HeartTree( this );
	}

	public override string SpiritName => Name;

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
