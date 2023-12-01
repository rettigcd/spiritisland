namespace SpiritIsland.NatureIncarnate;

public class HearthVigil : Spirit {

	// Pictures
	// Presence color

	public const string Name = "Hearth Vigil";

	public override SpecialRule[] SpecialRules => new SpecialRule[] { HearthToken.Rooted, HearthToken.FortifyHeart, HearthToken.LoyalGuardian, };

	public override string Text => Name;

	static Track Energy0_GatherDahanBonus => new Track( "energy0" ) {
		Icon = new IconDescriptor {
			BackgroundImg = Img.Coin,
			Text = "0",
		},
		Action = Cmd.GatherUpToNDahan( 1 ).To().SpiritPickedLand().Which( Has.YourPresence )
	};


	public HearthVigil():base(
		spirit => new SpiritPresence( spirit,
			new PresenceTrack( Energy0_GatherDahanBonus, Track.MkEnergy( 1, Element.Sun ), Track.Energy2, Track.MkEnergy( 3, Element.Animal ), Track.Energy4, Track.MkEnergy( 5, Element.Sun ) ),
			new PresenceTrack( Track.Card1, Track.Card2, Track.AirEnergy, Track.Card3, Track.AnimalEnergy, Track.Card4 ),
			new HearthToken( spirit )
		)
		,PowerCard.For(typeof(FavorsOfStoryAndSeason))
		,PowerCard.For(typeof(SurroundedByTheDahan))
		,PowerCard.For(typeof(CoordinatedRaid))
		,PowerCard.For(typeof(CallToVigilance))
	){
		GrowthTrack = new(
			new GrowthOption(
				new ReclaimAll(),
				new PlacePresence( 0 )
			),
			new GrowthOption(
				new GainPowerCard(),
				new PlacePresence( 3, Target.Dahan )
			),
			new GrowthOption(
				new PlacePresence(2),
				new GainEnergy(3)
			)
		);

		InnatePowers = new InnatePower[]{
			InnatePower.For(typeof(WarnOfImpendingConflict)), 
			InnatePower.For(typeof(KeepWatchForNewIncursions))
		};

	}

	protected override void InitializeInternal( Board board, GameState gs ) {
		// 1 in highest numbered land with dahan
		SpaceState[] tokens = board.Spaces.Tokens().ToArray();
		SpaceState highest = tokens.Last( t => t.Dahan.Any );
		highest.Adjust( Presence.Token, 1 );

		// 2 in the lowest numbered land with at least 2 dahan
		SpaceState lowest = tokens.First( t => 2 <= t.Dahan.CountAll);
		lowest.Adjust( Presence.Token, 2 );

		// Add 1 dahan in each of those lands
		highest.AdjustDefault( Human.Dahan, 1 );
		lowest.AdjustDefault( Human.Dahan, 1 );

		HearthToken.GrantHealthBoost( lowest );
		HearthToken.GrantHealthBoost( highest );

		// start with 1 energy
		Energy = 1;
	}

}
