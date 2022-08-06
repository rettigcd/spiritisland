namespace SpiritIsland.Basegame;

public class Ocean : Spirit {

	public const string Name = "Ocean's Hungry Grasp";

	public override SpecialRule[] SpecialRules => new SpecialRule[] { new SpecialRule("OCEAN IN PLAY", "You may add/move Presence into Oceans, but may not add/move Presence into Inland lands. On boards where you have 1 or more Presenceicon.png, Oceans are treated as Coastal Wetlands for Spirit Powers/Special Rules and Blighticon.png.You Drown any Invaders or Dahanicon.png moved to those Oceans."), new SpecialRule("DROWNING", "Destroy Drowned pieces, placing Drowned Invaders here. At any time you may exchange (X) Health of these Invaders for 1 Energy, where X = number of players.") };

	public Ocean():base(
		new OceanPresence(
			new PresenceTrack( Track.Energy0, Track.MoonEnergy, Track.WaterEnergy, Track.Energy1, Track.EarthEnergy, Track.WaterEnergy, Track.Energy2 ),
			new PresenceTrack( Track.Card1, Track.Card2, Track.Card2, Track.Card3, Track.Card4, Track.Card5 )
		),
		PowerCard.For<CallOfTheDeeps>(),
		PowerCard.For<GraspingTide>(),
		PowerCard.For<SwallowTheLandDwellers>(),
		PowerCard.For<TidalBoon>()
	) {

		GrowthTrack = new(
			// Option 1 - reclaim, +1 power, gather 1 presense into EACH ocean, +2 energy
			new GrowthOption(
				new GatherPresenceIntoOcean(),
				new ReclaimAll(),
				new DrawPowerCard(),
				new GainEnergy(2)
			), 
			// Option 2 - +1 presence range any ocean, +1 presense in any ociean, +1 energy
			new GrowthOption(
				new GainEnergy(1),
				new PlaceInOcean(),
				new PlaceInOcean()
			), 
			// Option 3 - gain power card, push 1 presense from each ocean,  add presense on costal land range 1
			new GrowthOption( 
				new PushPresenceFromOcean(),
				new DrawPowerCard(),
				new PlacePresence(1, Target.Coastal )
			)
		);

		InnatePowers = new InnatePower[]{
			InnatePower.For<OceanBreaksTheShore>(),
			InnatePower.For<PoundShipsToSplinters>()
		};

	}

	public override string Text => Name;

	protected override void InitializeInternal( Board board, GameState gameState ) {
		// Swap out Terrain evaluator for power
		gameState.Island.Terrain_ForPower = new OceanTerrainForPower( gameState.Island.Terrain_ForPower,this );
		gameState.Island.Terrain_ForBlight = new OceanTerrainForPower( gameState.Island.Terrain_ForBlight, this );

		// Place in Ocean
		Presence.PlaceOn(board.Ocean, gameState);

		this.AddActionFactory( new Setup_PlacePresenceInCostal() ); // let user pick initial ocean

		gameState.Tokens.TokenAdded.ForGame.Add(InvadersAdded);
		gameState.TimePasses_WholeGame += RemoveDrownedDahan;
	}

	void RemoveDrownedDahan( GameState gs ) {
		var actionId = Guid.NewGuid();
		foreach(var board in gs.Island.Boards)
			gs.Tokens[board.Ocean].Dahan.Bind(actionId).Drown();
	}

	async Task InvadersAdded( ITokenAddedArgs args ) {
		if( !args.Space.IsOcean ) return;
		var gs = args.GameState;
		var grp = args.Token.Class;

		if( grp == Invader.City || grp == Invader.Town || grp == Invader.Explorer ) { // Could created an Invader subclass that is easier to test.
			var ht = args.Token as HealthToken;
			// Drown Invaders for points
			drownedCount += ht.FullHealth;
			await gs.Invaders.On( args.Space, args.ActionId ).Destroy( 1, (HealthToken)args.Token );

			int spiritCount = gs.Spirits.Length;
			while(spiritCount <= drownedCount) {
				++Energy;
				drownedCount -= spiritCount;
			}
		}

	}

	int drownedCount = 0;

}