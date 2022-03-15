namespace SpiritIsland.BranchAndClaw;

public class Keeper : Spirit {

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
		PowerCard.For<TowingWrath>()
	) {
		(Presence as KeeperPresence).spirit = this;

		Growth = new Growth( 2,
			new GrowthOption( new ReclaimAll() ,new GainEnergy(1) ){ GainEnergy = 1 },
			new GrowthOption( new DrawPowerCard(1) ),
			new GrowthOption( new GainEnergy(1) ,new PlacePresence(3,Target.PresenceOrWilds) ){ GainEnergy = 1 },
			new GrowthOption( new GainEnergy(-3),new DrawPowerCard(1) ,new PlacePresence(3,Target.NoBlight) ){ GainEnergy = -3 }
		);

		InnatePowers = new InnatePower[] {
			InnatePower.For<PunishThoseWhoTrespass>(),
			InnatePower.For<SpreadingWilds>(),
		};
	}

	protected override void InitializeInternal( Board board, GameState gs ){
		// In the highest-numbered Jungle.
		var space = board.Spaces.OrderByDescending( x => x.IsJungle ).First();
		// Put 1 Presence
		Presence.PlaceOn( space, gs );
		// 1 Wild 
		gs.Tokens[space].Wilds.Init(1);
	}

	class KeeperPresence : SpiritPresence {
		public KeeperPresence()
			: base(
				new PresenceTrack( Track.Energy2, Track.SunEnergy, Track.Energy4, Track.Energy5, Track.PlantEnergy, Track.Energy7, Track.Energy8, Track.Energy9 ),
				new PresenceTrack( Track.Card1, Track.Card2, Track.Card2, Track.Card3, Track.Card4, Track.Card5Reclaim1 )
			) { }

		public override async Task Place( IOption from, Space to, GameState gs ) {
			await base.Place( from, to, gs );
			if(gs.DahanOn( to ).Any && SacredSites.Contains( to ))
				await spirit.Bind( gs, Cause.Growth ).Target( to ).PushDahan( int.MaxValue );
		}
		public Keeper spirit;
	}

}

