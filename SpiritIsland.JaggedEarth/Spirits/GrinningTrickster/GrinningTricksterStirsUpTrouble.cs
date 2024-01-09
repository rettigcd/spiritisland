namespace SpiritIsland.JaggedEarth;

public class GrinningTricksterStirsUpTrouble : Spirit {

	public const string Name = "Grinning Trickster Stirs Up Trouble";
	public override string Text => Name;

	public override SpecialRule[] SpecialRules => new SpecialRule[] {  
		TricksterTokens.ARealFlairForDiscord_Rule,  
		CleaningUpMessesIsADrag
	};

	static readonly SpecialRule CleaningUpMessesIsADrag = new SpecialRule(
		"Cleaning up Messes is a Drag", 
		"After one of your Powers Removes blight, Destroy 1 of your presence.  Ignore this rule for Let's See What Happens."
	);

	public GrinningTricksterStirsUpTrouble()
		:base(
			x => new SpiritPresence( x,
				new PresenceTrack(Track.Energy1,Track.MoonEnergy,Track.Energy2,Track.AnyEnergy,Track.FireEnergy,Track.Energy3),
				new PresenceTrack(Track.Card2,Track.Push1Dahan,Track.Card3,Track.Card3,Track.Card4,Track.AirEnergy,Track.Card5)
			),
				new GrowthTrack( 2,
				new GrowthOption( new GainEnergy( -1 ), new ReclaimAll(), new MovePresence( 1 ) ) { GainEnergy = -1 },
				new GrowthOption( new PlacePresence( 2 ) ),
				new GrowthOption( new GainPowerCard() ),
				new GrowthOption( new GainEnergyEqualToCardPlays() )
			)
			, PowerCard.For(typeof(ImpersonateAuthority))
			,PowerCard.For(typeof(InciteTheMob))
			,PowerCard.For(typeof(OverenthusiasticArson))
			,PowerCard.For(typeof(UnexpectedTigers))
		)
	{

		// Innates
		InnatePowers = new InnatePower[] {
			InnatePower.For(typeof(LetsSeeWhatHappens)),
			InnatePower.For(typeof(WhyDontYouAndThemFight))
		};
	}

	protected override void InitializeInternal( Board board, GameState gs ) {
		// Place presence on highest numbered land with dahan
		board.Spaces.Tokens().Where( s => s.Dahan.Any ).Last().Setup(Presence.Token, 1);
		// and in land #4
		board[4].Tokens.Setup(Presence.Token, 1);

	}

	// Cleanup Up Messes is such a drag
	public override async Task RemoveBlight( TargetSpaceCtx ctx, int count=1 ) {
		await CleaningUpMessesIsSuckADrag( ctx.Self, ctx.Tokens );
		await base.RemoveBlight( ctx,count );
	}

	static public async Task CleaningUpMessesIsSuckADrag( Spirit spirit, SpaceState tokens ) {
		if(tokens.Blight.Any)
			await Cmd.DestroyPresence( $"{CleaningUpMessesIsADrag.Title} Destroy presence for blight cleanup" )
				.ActAsync(spirit);
	}

	public override void InitSpiritAction( ActionScope scope ) {
		if( scope.Category == ActionCategory.Spirit_Power )
			scope.Upgrader = x => new TricksterTokens( this, x );
	}


}

