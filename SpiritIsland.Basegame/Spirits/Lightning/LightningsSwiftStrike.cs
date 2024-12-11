
namespace SpiritIsland.Basegame;

public class LightningsSwiftStrike : Spirit, IModifyAvailableActions {

	public const string Name = "Lightning's Swift Strike";

	static readonly SpecialRule SwiftnessOfLightning = new SpecialRule("Swiftness of Lightning", "For every Simple air you have, you may use 1 Slow Power as if it were fast");

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
		SpecialRules = [SwiftnessOfLightning];

		AvailableActionMods.Add(this);
	}

	public override string SpiritName => Name;

	protected override void InitializeInternal( Board board, GameState gs ) {
		// Setup: put 2 pressence in highest numbered sands
		var space = board.Spaces.Reverse().First(x=>x.IsSand);
		var tokens = space.ScopeSpace;
		tokens.Setup(Presence.Token, 2);
	}

	#region IRunWhenTimePasses
	public override Task TimePasses( GameState gameState ) {
		_usedAirForFastCount = 0;
		return base.TimePasses( gameState );
	}
	#endregion IRunWhenTimePasses

	void IModifyAvailableActions.Modify(List<IActionFactory> orig, Phase phase) {
		bool canMakeSlowFast = phase == Phase.Fast && _usedAirForFastCount < Elements.Get(Element.Air);
		if( canMakeSlowFast )
			orig.AddRange( AvailableActions.Where( slowAction => slowAction.CouldActivateDuring(Phase.Slow, this)));
	}

	protected override object CustomMementoValue {
		get => _usedAirForFastCount;
		set => _usedAirForFastCount = (int)value;
	}

	int _usedAirForFastCount = 0;

}