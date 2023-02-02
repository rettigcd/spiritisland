namespace SpiritIsland.Basegame;

public class Ocean : Spirit {

	public const string Name = "Ocean's Hungry Grasp";

	public override SpecialRule[] SpecialRules => new SpecialRule[] { OceanInPlay, DrowningRule	};

	readonly SpecialRule OceanInPlay = new SpecialRule(
		"Ocean In Play",
		"You may add/move Presence into Oceans, but may not add/move Presence into Inland lands. On boards where you have 1 or more Presence, Oceans are treated as Coastal Wetlands for Spirit Powers/Special Rules and Blight. You Drown any Invaders or Dahan moved to those Oceans."
	);

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
			// Option 3 - gain power card, push 1 presense from each ocean,  add presense on coastal land range 1
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
		Presence.Adjust(gameState.Tokens[board.Ocean], 1);

		AddActionFactory( new Setup_PlacePresenceInCostal() ); // let user pick initial ocean

		var drownMod = new TokenAddedHandler( "Ocean", Drowning, true);
		foreach(Board b in gameState.Island.Boards)
			gameState.Tokens[b.Ocean].Adjust( drownMod, 1 );
	}

	readonly SpecialRule DrowningRule = new SpecialRule(
		"Drowning",
		"Destroy Drowned pieces.  At any time you may exchange [# of players] Health of these Invaders for 1 Energy."
	);

	async Task Drowning( ITokenAddedArgs args ) {
		if( args.Token is not HumanToken ht ) return;

		// If we are saving a dahan
		if( ht.Class.Category == TokenCategory.Dahan && ShouldSaveDahan(args.ActionScope) && Presence.IsOn( args.AddedTo )	) {
			var moveOptions = args.GameState.Island.Boards
				.Select(x=>args.GameState.Tokens[x.Ocean])
				.SelectMany(x=>x.Adjacent)
				.Distinct()
				.ToArray();;
			// And Ocean chooses to save it
			var destination = await this.Gateway.Decision(Select.Space.PushToken((IVisibleToken)args.Token,args.AddedTo.Space,moveOptions, Present.Done));
			if( destination != null ) {
				// Move them at the end of the Action. (Let everyone handle the move-event before we move them again)
				args.ActionScope.AtEndOfThisAction(async _ => {
					//don't use original because that may or may not have been for a power.
					await using UnitOfWork childAction = args.GameState.StartAction( ActionCategory.Default );
					await BindSelf( args.GameState, childAction )
						.Move( args.Token, args.AddedTo.Space, destination );
				} );
				return; // the move it, don't drown it
			}
		}

		// Drown them immediately
		args.GameState.Log( new Log.Debug($"Drowning {args.Count}{ht.SpaceAbreviation} on {args.AddedTo.Space}") );
		await new InvaderBinding( args.GameState.Tokens[args.AddedTo.Space], args.ActionScope ).DestroyNTokens( ht, args.Count );

		// Track drowned invaders' health
		if(args.Token.Class.Category == TokenCategory.Invader)
			drownedInvaderHealthAccumulator += (ht.FullHealth * args.Count);
		CashInDrownedHealthForEnergy( args.GameState );
	}

	void CashInDrownedHealthForEnergy( GameState gs ) {
		int spiritCount = gs.Spirits.Length;
		int earnedEnergy = drownedInvaderHealthAccumulator / spiritCount;
		if( earnedEnergy == 0 ) return;

		int cashedInHealth = spiritCount * earnedEnergy;
		gs.Log( new Log.Debug($"Ocean gained {earnedEnergy} energy from cashing in {cashedInHealth} health of drowned invaders."));

		// Update Ocean
		drownedInvaderHealthAccumulator -= cashedInHealth;
		Energy += earnedEnergy;
	}

	int drownedInvaderHealthAccumulator = 0;

	// ! Hook for Tidal Boon
	static public void EnableSavingDahan( UnitOfWork actionScope ) { actionScope[SaveDahan] = true; }
	static public bool ShouldSaveDahan( UnitOfWork actionScope ) => actionScope.ContainsKey( SaveDahan );
	const string SaveDahan = "SaveDahanFromDrowning";
}