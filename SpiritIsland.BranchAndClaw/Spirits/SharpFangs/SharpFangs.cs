namespace SpiritIsland.BranchAndClaw;

public class SharpFangs : Spirit {

	public const string Name = "Sharp Fangs Behind the Leaves";


	public override SpecialRule[] SpecialRules => new SpecialRule[] { new SpecialRule("Ally of the Beasts", "Your presensee may move with beast.") } ;

	public override string Text => Name;

	public SharpFangs():base(
		new FollowingPresence(
			new PresenceTrack( Track.Energy1, Track.AnimalEnergy, Track.PlantEnergy, Track.Energy2, Track.AnimalEnergy, Track.Energy3, Track.Energy4 ),
			new PresenceTrack( Track.Card2, Track.Card2, Track.Card3, Track.CardReclaim1, Track.Card4, Track.Card5Reclaim1 ),
			Token.Beast
		),
		PowerCard.For<PreyOnTheBuilders>(),
		PowerCard.For<TeethGleamFromDarkness>(),
		PowerCard.For<TerrifyingChase>(),
		PowerCard.For<TooNearTheJungle>()
	) {
		
		var beastOrJungleRange3 = new PlacePresence(3, Target.Beast, Target.Jungle );

		GrowthTrack = new GrowthTrack( 2,
			new GrowthOption( new ReclaimAll(), new GainEnergy(-1), new DrawPowerCard(1) ){ GainEnergy=-1 },
			new GrowthOption( beastOrJungleRange3 ),
			new GrowthOption( new DrawPowerCard(1), new GainEnergy(1) ){ GainEnergy = 1 },
			new GrowthOption( new GainEnergy(3) ){ GainEnergy = 3 }
		);

		this.InnatePowers = new InnatePower[] {
			InnatePower.For<FrenziedAssult>(),
			InnatePower.For<RagingHunt>(),
		};

	}

	protected override void InitializeInternal( Board board, GameState gs ) {

		var highestJungle = board.Spaces.Where(x => x.IsJungle).Last().Tokens;
		highestJungle.Adjust(Presence.Token,1);
		highestJungle.Beasts.Init(1);

		// init special growth (note - we don't want this growth in Unit tests, so only add it if we call InitializeInternal())
		this.AddActionFactory(new Setup_PlacePresenceOnBeastLand());
	}

}