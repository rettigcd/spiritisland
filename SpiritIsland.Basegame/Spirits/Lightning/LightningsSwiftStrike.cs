namespace SpiritIsland.Basegame;

public class LightningsSwiftStrike : Spirit {

	public const string Name = "Lightning's Swift Strike";

	public override SpecialRule[] SpecialRules => new SpecialRule[] { new SpecialRule("SWIFTNESS OF LIGHTNING", "For every Simple air you have, you may use 1 Slow Power as if it were fast") };

	public LightningsSwiftStrike():base(
		new SpiritPresence(
			new PresenceTrack( Track.Energy1, Track.Energy1, Track.Energy2, Track.Energy2, Track.Energy3, Track.Energy4, Track.Energy4, Track.Energy5 ),
			new PresenceTrack( Track.Card2, Track.Card3, Track.Card4, Track.Card5, Track.Card6 )
		),
		PowerCard.For<HarbingersOfTheLightning>(),
		PowerCard.For<LightningsBoon>(),
		PowerCard.For<RagingStorm>(),
		PowerCard.For<ShatterHomesteads>()
	){
		GrowthTrack = new(
			new GrowthOption( 
				new ReclaimAll(), 
				new DrawPowerCard(1), 
				new GainEnergy(1)
			),
			// +1 presence range 2, +1 presence range 0( 
			new GrowthOption(
				new PlacePresence(2),
				new PlacePresence(0) 
			),
			// +1 presense range 1, +3 energy
			new GrowthOption( new GainEnergy(3), new PlacePresence(1) )
		);

		this.InnatePowers = new InnatePower[]{
			InnatePower.For<ThunderingDestruction>()
		};

	}

	public override string Text => Name;

	protected override void InitializeInternal( Board board, GameState gs ) {
		// Setup: put 2 pressence in highest numbered sands
		var space = board.Spaces.Reverse().First(x=>x.IsSand);
		var tokens = gs.Tokens[space];
		Presence.Adjust( tokens, 2 );

		gs.TimePasses_WholeGame += Gs_TimePassed;
	}

	Task Gs_TimePassed( GameState obj ) {
		usedAirForFastCount = 0;
		return Task.CompletedTask;
	}

	public override IEnumerable<IActionFactory> GetAvailableActions( Phase speed ) {

		bool canMakeSlowFast = speed == Phase.Fast 
			&& Elements[Element.Air] > usedAirForFastCount;

		foreach(var h in AvailableActions)
			if( IsActiveDuring( speed, h ) || canMakeSlowFast && IsActiveDuring( Phase.Slow, h ) )
				yield return h;

	}

	public override Task TakeAction( IActionFactory factory, SelfCtx ctx ) {

		// we can decrement any time a slow card is used,
		// even during slow because we no longer care about this
		if(ctx.GameState.Phase == Phase.Fast
			&& factory.CouldActivateDuring( Phase.Slow, this )
			&& factory is IFlexibleSpeedActionFactory flexSpeedFactory
		) {
			++usedAirForFastCount;
			TemporarySpeed.Override( flexSpeedFactory, Phase.Fast, ctx.GameState );
		}

		return base.TakeAction(factory,ctx);
	}


	int usedAirForFastCount = 0;

}