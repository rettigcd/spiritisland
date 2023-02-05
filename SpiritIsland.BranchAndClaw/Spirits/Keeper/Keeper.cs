namespace SpiritIsland.BranchAndClaw;

public partial class Keeper : Spirit {

	public const string Name = "Keeper of the Forbidden Wilds";

	public override string Text => Name;

	public override SpecialRule[] SpecialRules => new SpecialRule[] {
		new SpecialRule("Forbidden Ground","Any time you create a sacred site, push all dahan from that land.  Dahan events never move dahan to you sacred site but powers can do so.")
	} ;

	public Keeper():base(
		new KeeperPresence(),
		// PowerCard.For<SkyStretchesToShore>(),
		PowerCard.For<BoonOfGrowingPower>(),
		PowerCard.For<RegrowFromRoots>(),
		PowerCard.For<SacrosanctWilderness>(),
		PowerCard.For<ToweringWrath>()
	) {

		GrowthTrack = new GrowthTrack( 2,
			new GrowthOption( new ReclaimAll() ,new GainEnergy(1) ){ GainEnergy = 1 },
			new GrowthOption( new DrawPowerCard(1) ),
			new GrowthOption( new GainEnergy(1) ,new PlacePresence(3,Target.Presence, Target.Wilds ) ){ GainEnergy = 1 },
			new GrowthOption( new GainEnergy(-3),new DrawPowerCard(1) ,new PlacePresence(3,Target.NoBlight) ){ GainEnergy = -3 }
		);

		InnatePowers = new InnatePower[] {
			InnatePower.For<PunishThoseWhoTrespass>(),
			InnatePower.For<SpreadingWilds>(),
		};
	}

	protected override void InitializeInternal( Board board, GameState gs ){
		// In the highest-numbered Jungle.
		var highestNumberedJungle = gs.Tokens[ board.Spaces.Where( x => x.IsJungle ).OrderBy( x => x.Label ).Last() ];
		// Put 1 Presence
		highestNumberedJungle.Adjust(Presence.Token,1);
		// 1 Wild 
		highestNumberedJungle.Wilds.Init(1);
	}

}

