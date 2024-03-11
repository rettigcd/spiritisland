namespace SpiritIsland.Basegame;

public class LightningsSwiftStrike : Spirit {

	public const string Name = "Lightning's Swift Strike";

	public override SpecialRule[] SpecialRules => new SpecialRule[] { new SpecialRule("SWIFTNESS OF LIGHTNING", "For every Simple air you have, you may use 1 Slow Power as if it were fast") };

	public LightningsSwiftStrike():base(
		spirit => new SpiritPresence( spirit,
			new PresenceTrack( Track.Energy1, Track.Energy1, Track.Energy2, Track.Energy2, Track.Energy3, Track.Energy4, Track.Energy4, Track.Energy5 ),
			new PresenceTrack( Track.Card2, Track.Card3, Track.Card4, Track.Card5, Track.Card6 )
		),
		new GrowthTrack(
			new GrowthGroup(
				new ReclaimAll(),
				new GainPowerCard(),
				new GainEnergy( 1 )
			),
			// +1 presence range 2, +1 presence range 0( 
			new GrowthGroup(
				new PlacePresence( 2 ),
				new PlacePresence( 0 )
			),
			// +1 presense range 1, +3 energy
			new GrowthGroup( new GainEnergy( 3 ), new PlacePresence( 1 ) )
		),
		PowerCard.For(typeof(HarbingersOfTheLightning)),
		PowerCard.For(typeof(LightningsBoon)),
		PowerCard.For(typeof(RagingStorm)),
		PowerCard.For(typeof(ShatterHomesteads))
	){

		InnatePowers = [ InnatePower.For(typeof(ThunderingDestruction)) ];

	}

	public override string Text => Name;

	protected override void InitializeInternal( Board board, GameState gs ) {
		// Setup: put 2 pressence in highest numbered sands
		var space = board.Spaces.Reverse().First(x=>x.IsSand);
		var tokens = space.ScopeTokens;
		tokens.Setup(Presence.Token, 2);
	}

	#region IRunWhenTimePasses
	public override Task TimePasses( GameState gameState ) {
		_usedAirForFastCount = 0;
		return base.TimePasses( gameState );
	}
	#endregion IRunWhenTimePasses

	public override IEnumerable<IActionFactory> GetAvailableActions( Phase speed ) {

		bool canMakeSlowFast = speed == Phase.Fast && _usedAirForFastCount < Elements.Get(Element.Air);

		foreach(var h in AvailableActions)
			if( h.CouldActivateDuring( speed, this ) || canMakeSlowFast && h.CouldActivateDuring(Phase.Slow,this) )
				yield return h;

	}

	public override Task TakeActionAsync( IActionFactory factory, Phase phase ) {

		// we can decrement any time a slow card is used,
		// even during slow because we no longer care about this
		if(phase == Phase.Fast
			&& factory.CouldActivateDuring( Phase.Slow, this )
			&& factory is IFlexibleSpeedActionFactory flexSpeedFactory
		) {
			++_usedAirForFastCount;
			TemporarySpeed.Override( flexSpeedFactory, Phase.Fast, GameState.Current );
		}

		return base.TakeActionAsync(factory,phase);
	}

	protected override object CustomMementoValue {
		get => _usedAirForFastCount;
		set => _usedAirForFastCount = (int)value;
	}

	int _usedAirForFastCount = 0;

}