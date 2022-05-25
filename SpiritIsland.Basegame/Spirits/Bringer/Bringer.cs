namespace SpiritIsland.Basegame;

public class Bringer : Spirit {

	public const string Name = "Bringer of Dreams and Nightmares";

	public override string Text => Name;

	public override SpecialRule[] SpecialRules => new SpecialRule[] {
		new SpecialRule("TO DREAM A THOUSAND DEATHS","Your Powers never cause Damage, nor can they Destroy anything other than your own Presence. When your Powers would Destroy Invaders, instead generate 0/2/5 Fear and Pushes Invaders")
	} ;

	public Bringer():base(
		new SpiritPresence(
			new PresenceTrack( Track.Energy2, Track.AirEnergy, Track.Energy3, Track.MoonEnergy, Track.Energy4, Track.AnyEnergy, Track.Energy5 ),
			new PresenceTrack( Track.Card2, Track.Card2, Track.Card2, Track.Card3, Track.Card3, Track.AnyEnergy )
		),
		PowerCard.For<CallOnMidnightsDream>(),
		PowerCard.For<DreadApparitions>(),
		PowerCard.For<DreamsOfTheDahan>(),
		PowerCard.For<PredatoryNightmares>()
	) {

		Growth = new(
			// reclaim, +1 power card
			new GrowthOption(new ReclaimAll(),new DrawPowerCard(1)),
			// reclaim 1, add presence range 0
			new GrowthOption(new ReclaimN(), new PlacePresence(0) ),
			// +1 power card, +1 pressence range 1
			new GrowthOption(new DrawPowerCard(1), new PlacePresence(1) ),
			// add presense range Dahan or Invadors, +2 energy
			new GrowthOption(new GainEnergy(2), new PlacePresence(4,Target.DahanOrInvaders))
		);

		this.InnatePowers = new InnatePower[]{
			InnatePower.For<SpiritsMayYetDream>(),
			InnatePower.For<NightTerrors>()
		};

	}

	protected override void InitializeInternal( Board board, GameState gs ) {
		// Setup: 2 presense in highest numbered sands
		var startingIn = board.Spaces.Where(x=>x.IsSand).Last();
		Presence.PlaceOn( startingIn, gs );
		Presence.PlaceOn( startingIn, gs );
	}

	//public override Task DestroyInvaderForPowers( GameState gs, Space space, int count, Token dahanToken ) {
	//	return Task.CompletedTask;
	//}

	public override SelfCtx BindMyPower( GameState gameState ) => new BringerCtx(this,gameState,Guid.NewGuid());

}

class BringerCtx : SelfCtx {
	public BringerCtx( Bringer bringer, GameState gs, Guid actionId ):base( bringer, gs, Cause.MyPowers, actionId ) {}
	public override TargetSpaceCtx Target( Space space ) => new BringerSpaceCtx(this, space);
}

class BringerSpaceCtx : TargetSpaceCtx {
	public BringerSpaceCtx(BringerCtx ctx,Space space ) : base( ctx, space ) { }

	protected override InvaderBinding GetInvaders() {
		return new InvaderBinding(
			new TokenCountDictionary( Tokens ),
			new ToDreamAThousandDeaths_DestroyStrategy( GameState.Fear.AddDirect, this ),
			this.CurrentActionId
		);
	}

}
