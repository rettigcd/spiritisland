namespace SpiritIsland.BranchAndClaw;

public class Keeper : Spirit {

	public const string Name = "Keeper of the Forbidden Wilds";

	public override string Text => Name;

	public override SpecialRule[] SpecialRules => new SpecialRule[] {
		new SpecialRule("Forbidden Ground","Any time you create a sacred site, push all dahan from that land.  Dahan events never move dahan to you sacred site but powers can do so.")
	} ;

	public Keeper():base(
		spirit => new SpiritPresence( spirit,
			new PresenceTrack( Track.Energy2, Track.SunEnergy, Track.Energy4, Track.Energy5, Track.PlantEnergy, Track.Energy7, Track.Energy8, Track.Energy9 ),
			new PresenceTrack( Track.Card1, Track.Card2, Track.Card2, Track.Card3, Track.Card4, Track.Card5Reclaim1 ),
			new KeeperToken( spirit )
		),
		PowerCard.For(typeof(BoonOfGrowingPower)),
		PowerCard.For(typeof(RegrowFromRoots)),
		PowerCard.For(typeof(SacrosanctWilderness)),
		PowerCard.For(typeof(ToweringWrath))
	) {
		GrowthTrack = new GrowthTrack( 2,
			new GrowthOption( new ReclaimAll() ,new GainEnergy(1) ){ GainEnergy = 1 },
			new GrowthOption( new GainPowerCard() ),
			new GrowthOption( new GainEnergy(1), new PlacePresence(3,Target.Presence, Target.Wilds ) ){ GainEnergy = 1 },
			new GrowthOption( new GainEnergy(-3), new GainPowerCard() ,new PlacePresence(3,Target.NoBlight) ){ GainEnergy = -3 }
		);

		InnatePowers = new InnatePower[] {
			InnatePower.For(typeof(PunishThoseWhoTrespass)),
			InnatePower.For(typeof(SpreadingWilds)),
		};
	}

	protected override void InitializeInternal( Board board, GameState gs ){
		// In the highest-numbered Jungle.
		var highestNumberedJungle = board.Spaces.Where( x => x.IsJungle ).OrderBy( x => x.Label ).Last().Tokens;
		// Put 1 Presence
		highestNumberedJungle.Adjust(Presence.Token, 1);
		// 1 Wild 
		highestNumberedJungle.Wilds.Init(1);
	}

}

