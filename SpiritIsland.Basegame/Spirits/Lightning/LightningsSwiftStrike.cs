namespace SpiritIsland.Basegame;

public class LightningsSwiftStrike : Spirit {

	public const string Name = "Lightning's Swift Strike";
	public override string SpiritName => Name;

	public LightningsSwiftStrike():base(
		spirit => new SpiritPresence( spirit,
			new PresenceTrack( Track.Energy1, Track.Energy1, Track.Energy2, Track.Energy2, Track.Energy3, Track.Energy4, Track.Energy4, Track.Energy5 ),
			new PresenceTrack( Track.Card2, Track.Card3, Track.Card4, Track.Card5, Track.Card6 )
		),
		new GrowthTrack(
			new GrowthGroup( new ReclaimAll(), new GainPowerCard(), new GainEnergy( 1 ) ),
			new GrowthGroup( new PlacePresence( 2 ), new PlacePresence( 0 ) ),
			new GrowthGroup( new GainEnergy( 3 ), new PlacePresence( 1 ) )
		),
		PowerCard.ForDecorated(HarbingersOfTheLightning.ActAsync),
		PowerCard.ForDecorated(LightningsBoon.ActAsync),
		PowerCard.ForDecorated(RagingStorm.ActAsync),
		PowerCard.ForDecorated(ShatterHomesteads.ActAsync)
	){

		InnatePowers = [InnatePower.For(typeof(ThunderingDestruction)) ];
		SpecialRules = [SwiftnessOfLightning.Rule];

		Mods.Add(new SwiftnessOfLightning(this));
	}

	/// <summary> Fast phase: say how many Slow Powers can still be made Fast, because the Fast list shows them all. </summary>
	protected override string ResolvePrompt( Phase phase ) {
		string prompt = base.ResolvePrompt( phase );
		SwiftnessOfLightning? swiftness = Mods.OfType<SwiftnessOfLightning>().FirstOrDefault(); // the Wind aspect can remove it
		return phase == Phase.Fast && swiftness is not null ? $"{prompt} ({swiftness.AllowanceStatus})" : prompt;
	}

	protected override void InitializeInternal( Board board, GameState gs ) {
		// Setup: put 2 pressence in highest numbered sands
		var space = board.Spaces.Reverse().First(x=>x.IsSand);
		var tokens = space.ScopeSpace;
		tokens.Setup(Presence.Token, 2);
	}

}