namespace SpiritIsland.JaggedEarth;

public partial class ManyMindsMoveAsOne : Spirit {

	public const string Name = "Many Minds Move as One";
	public override string SpiritName => Name;

	#region custom Track
	static Track CardBoost => new Track( "Pay2ForExtraPlay" ) { 
		Action = new Pay2EnergyToGainAPowerCard(),
		Icon = new IconDescriptor { ContentImg = Img.GainCard,
			Super = new IconDescriptor { BackgroundImg = Img.Coin, Text= "—2" }
		}
	};
	#endregion custom Track

	#region custom Growth

	class PlacePresenceAndBeast : SpiritAction {

		public PlacePresenceAndBeast() : base("Place Presence and Beast") { }

		public override async Task ActAsync(Spirit self) {
			// Range 3
			Space[] toOptions = DefaultRangeCalculator.Singleton.GetTargetingRoute_MultiSpace(self.Presence.Lands, new TargetCriteria(3)).Targets;
			var move = await self.SelectAlways(Prompts.SelectPresenceTo(), self.DeployablePresence().BuildMoves(_ => toOptions).ToArray());
			await move.Apply();

			// Add beast
			await ((Space)move.Destination).Beasts.AddAsync(1);
		}

	}

	#endregion custom Growth

	public ManyMindsMoveAsOne()
		:base(
			spirit => new SpiritPresence( spirit,
				new PresenceTrack(Track.Energy0,Track.Energy1,Track.MkEnergyElements(Element.Air),Track.Energy2,Track.MkEnergyElements(Element.Animal),Track.Energy3,Track.Energy4),
				new PresenceTrack(Track.Card1,Track.Card2,CardBoost,Track.Card3,Track.Card3,Track.Card4,Track.Card5),
				new AJoiningOfSwarmsAndFlocks( spirit )
			)
			, new GrowthTrack(
				new GrowthGroup( new ReclaimAll(), new GainPowerCard() ),
				new GrowthGroup( new PlacePresence( 1 ), new PlacePresence( 0 ) ),
				new GrowthGroup( new PlacePresenceAndBeast(), new GainEnergy( 1 ), new Gather1Token( 2, Token.Beast ) )
			)
			, PowerCard.ForDecorated(ADreadfulTideOfScurryingFlesh.ActAsync)
			, PowerCard.ForDecorated(BoonOfSwarmingBedevilment.ActAsync)
			, PowerCard.ForDecorated(EverMultiplyingSwarm.ActAsync)
			, PowerCard.ForDecorated(GuideTheWayOnFeatheredWings.ActAsync)
			, PowerCard.ForDecorated(PursueWithScratchesPecksAndStings.ActAsync)
		) {

		InnatePowers = [
			InnatePower.For(typeof(TheTeemingHostArrives)), 
			InnatePower.For(typeof(BesetAndConfoundTheInvaders))
		];
		SpecialRules = [FlyFastAsThought.Rule, AJoiningOfSwarmsAndFlocks.Rule];

		Mods.Add(new FlyFastAsThought());
	}

	protected override void InitializeInternal( Board board, GameState gameState ) {
		// Put 1 presence and 1 beast on yoru starting borad, in a land with beast.
		var land = board.Spaces.ScopeTokens().First( x => x.Beasts.Any );

		land.Setup(Presence.Token, 1);
		land.Beasts.Init(1);

	}

}

