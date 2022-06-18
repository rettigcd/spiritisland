﻿namespace SpiritIsland.Basegame;

public class ASpreadOfRampantGreen : Spirit {

	public const string Name = "A Spread of Rampant Green";

	public override string Text => Name;

	public override SpecialRule[] SpecialRules => new SpecialRule[] {
		new SpecialRule("Choke the land with green","Whenever invaders would ravage or build in a land with your sacred site, you may prevent it by destroying one of your presense in that land."),
		SteadyRegeneration
	};

	static SpecialRule SteadyRegeneration => new SpecialRule(
		"Steady Regeneration",
		"When adding Presence to the board via Growth, you may optionally use your destroyed Presence. If the island is Healthy, do so freely. If the island is Blighted, doing so costs 1 Energy per destroyed Presence you add."
	);

	public ASpreadOfRampantGreen():base(
		new RampantGreenPresence(),
		PowerCard.For<FieldsChokedWithGrowth>(),
		PowerCard.For<GiftOfProliferation>(),
		PowerCard.For<OvergrowInANight>(),
		PowerCard.For<StemTheFlowOfFreshWater>()
	) {
		// Special rules: steady regeneration

		Growth = new Growth(
			new GrowthOption( new PlacePresence( 2, Target.JungleOrWetland ) )
		).Add( new GrowthOptionGroup( 1,
			// reclaim, +1 power card
			new GrowthOption(
				new ReclaimAll(), 
				new DrawPowerCard(1)
			),
			// +1 presense range 1, play +1 extra card this turn
			new GrowthOption(
				new PlacePresence(1),
				new PlayExtraCardThisTurn(1)
			),
			// +1 power card, +3 energy
			new GrowthOption(
				new GainEnergy(3), 
				new DrawPowerCard()
			)
		));

		this.InnatePowers = new InnatePower[] {
			InnatePower.For<CreepersTearIntoMortar>(),
			InnatePower.For<AllEnvelopingGreen>(),
		};

	}

	protected override PowerProgression GetPowerProgression() =>
		new (
			PowerCard.For<DriftDownIntoSlumber>(),
			PowerCard.For<GiftOfLivingEnergy>(),
			PowerCard.For<TheTreesAndStonesSpeakOfWar>(), // major
			PowerCard.For<LureOfTheUnknown>(),
			PowerCard.For<InfiniteVitality>(), // major
			PowerCard.For<EnticingSplendor>()
		);


	protected override void InitializeInternal( Board board, GameState gs ) {

		// Setup: 1 in the highest numbered wetland and 1 in the jungle without any dahan
		Presence.PlaceOn( board.Spaces.Reverse().First(x=>x.IsWetland), gs );
		Presence.PlaceOn( board.Spaces.Single(x=>x.IsJungle && gs.DahanOn(x).Count==0), gs );

		gs.PreRavaging.ForGame.Add( ChokeTheLandWithGreen_Ravage );
		gs.PreBuilding.ForGame.Add( ChokeTheLandWithGreen_Build );

	}

	async Task ChokeTheLandWithGreen_Ravage( RavagingEventArgs args ) {
		var stopped = await ChokeTheLandWithGreen( args.GameState, args.Spaces.ToArray(), "ravage" );
		foreach(var space in stopped)
			args.Skip1( space );
	}

	async Task ChokeTheLandWithGreen_Build( BuildingEventArgs args ) {
		// !!! This is out of order.
		// Needs to come after Fear-stops and Power-Stops
		Space[] buildSpaces = args.SpacesWithBuildTokens.Where( k => args.GameState.Tokens[k][TokenType.DoBuild] > 0 ).ToArray();
		Space[] stopped = await ChokeTheLandWithGreen( args.GameState, buildSpaces, "build" ); 
		foreach(var s in stopped)
			args.GameState.Skip1Build( s );
	}

	async Task<Space[]> ChokeTheLandWithGreen( GameState gs, Space[] spaces, string actionText ) {

		var stoppable = spaces.Intersect( Presence.SacredSites ).ToList();
		bool costs1 = gs.BlightCard.CardFlipped;
		int maxStoppable = costs1 ? Energy : int.MaxValue;

		var skipped = new List<Space>();
		while(maxStoppable > 0 && stoppable.Count > 0) {
			var stop = await this.Action.Decision( new Select.Space( $"Stop {actionText} by destroying 1 presence", stoppable.ToArray(), Present.Done ) );
			if(stop == null) break;

			await Presence.Destroy( stop, gs, DestoryPresenceCause.DahanDestroyed ); // it is the invader actions we are stopping

			skipped.Add( stop );
			stoppable.Remove( stop );
			--maxStoppable;

			if(costs1) --Energy;
		}
		return skipped.ToArray();
	}

}

public class RampantGreenPresence : SpiritPresence {

	public RampantGreenPresence() 
		: base(
			new PresenceTrack( Track.Energy0, Track.Energy1, Track.PlantEnergy, Track.Energy2, Track.Energy2, Track.PlantEnergy, Track.Energy3 ),
			new PresenceTrack( Track.Card1, Track.Card1, Track.Card2, Track.Card2, Track.Card3, Track.Card4 )
		) { }

	public override IEnumerable<Track> RevealOptions { get { 
		var options = base.RevealOptions.ToList();
		if( Destroyed>0 ) options.Add(Track.Destroyed);
		return options;
	} }

}