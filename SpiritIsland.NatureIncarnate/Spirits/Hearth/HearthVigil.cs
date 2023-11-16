namespace SpiritIsland.NatureIncarnate;

public class HearthVigil : Spirit {

	// Pictures
	// Presence color

	public const string Name = "Hearth Vigil";

	public override SpecialRule[] SpecialRules => new SpecialRule[] { HearthToken.Rooted, HearthToken.FortifyHeart, HearthToken.LoyalGuardian, };

	public override string Text => Name;

	public HearthVigil():base(
		new HearthPresence(),
		PowerCard.For<FavorsOfStoryAndSeason>(),
		PowerCard.For<SurroundedByTheDahan>(),
		PowerCard.For<CoordinatedRaid>(),
		PowerCard.For<CallToVigilance>()
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
