namespace SpiritIsland.NatureIncarnate;

public class HabsburgMiningExpedition : AdversaryBase, IAdversary {

	public const string Name = "Habsburg Mining Expedition";
	
	public override AdversaryLevel[] Levels => _scenarioMods;

	AdversaryLevel[] _scenarioMods = new AdversaryLevel[] { Escalation, L1, L2, L3, L4, L5, L6 };

	#region Escalation / Win-Loss Condition

	static AdversaryLevel Escalation => new AdversaryLevel( 1, 3,3,3, "Mining Tunnels", 
		"After Advancing Invader Cards: On each board, Explore in 2 lands whose terrains don't match a Ravage or Build Card (no source required)."
	){ 
		InitFunc = (gs,_) => {
			// At the end of the Fast Phase
			gs.StartOfInvaderPhase.Add( LandStrippedBare_WinLossCheck );
		}
	}.WithEscalation(MiningTunnels);
	static Task MiningTunnels(GameState gs ) {
		// After Advancing Invader Cards
		// On each board,
		// Explore in 2 lands whose terrains don't match a Ravage or Build Card (no source required).
		return new SpaceAction("Explore",ctx=>ctx.Tokens.AddDefault(Human.Explorer,1))
			.In().NDifferentLandsPerBoard(2)
			.Which( Is.NotExploreOrBuildCardMatch ) // Doing this before we advance cars so they are in the Explore/Build slot
			.ForEachBoard()
			.ActAsync( gs );
	}
	static void LandStrippedBare_WinLossCheck( GameState gameState ) {
		var landStrippedBare = gameState.Spaces_Existing.FirstOrDefault( ss => 8 <= ss.SumAny( Human.Invader ));
		// if any land has at least 8 total Invaders/ Blight( combined ),
		if(landStrippedBare is not null) {
			// the Invaders win
			int count = landStrippedBare.SumAny( Human.Invader );
			GameOverException.Lost( $"Land Stripped Bare - {count} Invaders on {landStrippedBare.Space.Text}." );
		}
	}

	#endregion Escalation / Win-Loss Condition

	#region Level 1

	static AdversaryLevel L1 => new AdversaryLevel( 3, 3,3,3, "Avarice Rewarded/Ceaseless Mining", 
		"When Blight added by a Ravage Action would cascade, instead Upgrade 1 Explorer/Town( before dahan counterattack). "+
		"Lands with 3 or more Invaders are Mining lands.In Mining lands: (1) Disease and modifiers to Disease affect Ravage Actions as though they were Build Actions. (2) During the Build Step, Build Cards cause Ravage Actions (instead of Build Actions)."
	){       
		InitFunc = (gs, _) => {
			// Avarice Rewarded:
			// When Blight added by a Ravage Action would cascade, instead Upgrade 1 Explorer / Town( before dahan counterattack ).
			gs.AddIslandMod( new AvariceRewardedMod() );

			// Ceaseless Mining: Lands with 3 or more Invaders are Mining lands. In Mining lands: 

			// Disease and modifiers to Disease affect Ravage Actions as though they were Build Actions.
			gs.AddIslandMod(new DiseaseStopsRavageInMiningLands());

			// During the Build Step, Build Cards cause Ravage Actions( instead of Build Actions ).
			gs.InvaderDeck.Build.Engine = new RavageInMiningLandsDuringBuild();
		}
	};

	/// <summary>
	/// Replaces a Ravage-Blight-Cascade with an Invader upgrade
	/// </summary>
	class AvariceRewardedMod : BaseModEntity, IModifyAddingToken, IHandleTokenAddedAsync {

		// !!! If there is another Mod that stops the cascade, it should run first so we don't trigger this effect.

		public void ModifyAdding( AddingTokenArgs args ) {
			if(
				// When Blight added 
				args.Token == Token.Blight
				// by a Ravage Action 
				&& args.Reason == AddReason.Ravage
				// would cascade
				&& 1 <= args.To.Blight.Count
				&& BlightToken.ForThisAction.ShouldCascade
			) {
				// stop cascade
				BlightToken.ForThisAction.ShouldCascade = false;
				// instead Upgrade 1 Explorer / Town( before dahan counterattack ).
				ShouldUpgrade = true;
			}
		}

		public async Task HandleTokenAddedAsync( SpaceState to, ITokenAddedArgs args ) {
			if(args.Added == Token.Blight         // only check ActionScope when adding a blight ravage
				&& args.Reason == AddReason.Ravage
				&& ShouldUpgrade
			)
				await ReplaceInvader.Upgrade1Token(
					to.Space.Boards.First().FindSpirit(),
					to,
					Present.Always,
					to.HumanOfAnyTag( Human.Explorer_Town ).ToArray()
				);
		}

		// Records should-upgrade in the ActionScope since we cannot detect it once we stop the cascade.
		bool ShouldUpgrade {
			get => ActionScope.Current.SafeGet<bool>( Key );
			set => ActionScope.Current[Key] = value;
		}
		const string Key = "Avarice Rewarded";

	}

	class DiseaseStopsRavageInMiningLands : BaseModEntity, ISkipRavages {
		public UsageCost Cost => Token.Disease.Cost;

		public async Task<bool> Skip( SpaceState space ) {
			if(!IsMiningLand(space)) return false; // Is Mining Land

			DiseaseToken[] x = space.OfType<DiseaseToken>().ToArray();
			foreach(var token in x)
				if(await token.Skip(space))
					return true;
			return false;
		}

	}

	class RavageInMiningLandsDuringBuild : BuildEngine {
		public override Task Do1Build( GameState gameState, SpaceState spaceState ) {
			return IsMiningLand(spaceState) 
				? spaceState.Ravage() 
				: new BuildOnceOnSpace( gameState, spaceState ).ExecAsync();
		}
	}
	#endregion Level 1

	#region Level 2
	static AdversaryLevel L2 = new AdversaryLevel( 4, 
		3, 3, 4, 
		"Miners Come From Far and Wide", 
		"Setup: (a) Add 1 Explorer in each land with no Dahan. (b) Add 1 Disease and 1 City in the highest-numbered land with a town symbol."
	) {
		AdjustFunc = ( gs, _ ) => {
			// Add 1 Explorer in each land with no Dahan.
			var spacesThatGetAnExplorer = gs.Spaces_Existing
				.Where( s => !s.Space.Is( Terrain.Ocean ) && s.Dahan.CountAll==0 )
				.ToArray();

			foreach(var ss in spacesThatGetAnExplorer)
				ss.AdjustDefault(Human.Explorer,1);
			
			// Add 1 Disease and 1 City in the highest-numbered land with a town symbol.
			var highestTowns = gs.Island.Boards
				.Select( board => board.Spaces.Where(s => s is Space1 s1 && s1.StartUpCounts.Towns != 0).Last() )
				.Tokens()
				.ToArray();
			foreach(var s in highestTowns) {
				s.Disease.Adjust(1);
				s.AdjustDefault(Human.City,1);
			}
		}
	};
	#endregion Level 2

	#region Level 3
	static AdversaryLevel L3 = new AdversaryLevel( 5, 
		3, 4, 4, 
		"Mining Boom (I)", 
		"After the Build Step, on each board: Choose a land with an Explorer. Upgrade 1 Explorer there."
	) {
		InitFunc = ( gs, adversary ) => {
			if(5 <= adversary.Level) return; // level 5 replaces this
			gs.InvaderDeck.Build.ActionComplete.Add( UpgradeExplorerOnEachLandAsync );
		}
	};
	static Task UpgradeExplorerOnEachLandAsync( GameState gs ) {
		return new SpaceAction("Upgrade explorer", async x=>{
			var token = await x.Self.SelectAsync(new A.SpaceToken("Select Explorer to Upgrade",x.Tokens.SpaceTokensOfTag(Human.Explorer),Present.Always));
			if(token != null)
				ReplaceInvader.UpgradeSelectedInvader(x.Tokens,token.Token.AsHuman()); 
		} )
			.In().OneLandPerBoard().Which(Has.Token(Human.Explorer))
			.ForEachBoard()
			.ActAsync(gs);
	}
	#endregion Level 3

	#region Level 4

	static AdversaryLevel L4 = new AdversaryLevel( 7, 
		4, 4, 4, 
		"Untapped Salt Deposits", 
		"111-2S22-33333 (Remove Stage-2 Coastal Lands, Place Salt Deposits in 2nd Stage-2 slot."
	) {
		InvaderDeckBuilder = new SaltDepositsInvaderDeckBuilder(1,1,1,2,SaltDepositsInvaderDeckBuilder.SaltDeposits,2,2,3,3,3,3,3),
		// !! The no-advance is baked in to the Ravage Slot for the moment.
	};

	/// <summary> Replaces Coastal with Salt Deposits </summary>
	class SaltDepositsInvaderDeckBuilder : InvaderDeckBuilder {

		public const int SaltDeposits = 99;

		public SaltDepositsInvaderDeckBuilder( params int[] levelSelection ) : base( levelSelection ) { }

		protected override IEnumerable<InvaderCard> SelectLevel2Cards()
			=> base.SelectLevel2Cards().Where( x => !x.Text.Contains( CoastalFilter.Name ) );

		protected override void SelectCard( List<InvaderCard> dst, Queue<InvaderCard>[] src, int level ) {
			if(level == SaltDeposits)
				dst.Add( new InvaderCard(new SaltDepositsFilter(),2) );
			else
				base.SelectCard( dst, src, level );
		}

	}
	/// <summary>
	/// Explore & Build => matches non-Mining lands
	/// Ravage => matches Mining Lands
	/// </summary>
	class SaltDepositsFilter : InvaderCardSpaceFilter {
		public string Text => "Salt Deposits";
		public bool Matches( Space space ) {
			return IsInRavageThisAction() == IsMiningLand(space.Tokens);
		}
		bool IsInRavageThisAction() {
			var scope = ActionScope.Current;
			if(scope.ContainsKey(Key)) return scope.SafeGet<bool>( Key );
			GameState gs = GameState.Current;
			bool isInRavage = gs.InvaderDeck.Ravage.Cards.Any( c=>c.Text.Contains(Text) );
			scope[Key] = isInRavage;
			return isInRavage;
		}
		const string Key = "Salt Deposits Ravaging";
	}
	#endregion Level 4

	#region Level 5

	static AdversaryLevel L5 = new AdversaryLevel( 9, 
		4, 5, 4, 
		"Mining Boom (II)", 
		"Instead of Mining Boom(I), after the Build Step, on each board: Choose a land with Explorer. Build there, then Upgrade 1 Explorer( Build normally in a Mining land.)"
	) {
		InitFunc = ( gs, _ ) => {
			gs.InvaderDeck.Build.ActionComplete.Add( BuildThenUpgradeExplorer ); 
		}
	};
	static Task BuildThenUpgradeExplorer( GameState gs ) {
		return new SpaceAction( "Upgrade explorer", async x => {
			await new BuildOnceOnSpace(gs,x.Tokens).ExecAsync();
			var token = await x.Self.SelectAsync( new A.SpaceToken( "Select Explorer to Upgrade", x.Tokens.SpaceTokensOfTag( Human.Explorer ), Present.Always ) );
			if(token != null)
				ReplaceInvader.UpgradeSelectedInvader( x.Tokens, token.Token.AsHuman() );
		} )
			.In().OneLandPerBoard().Which( Has.Token( Human.Explorer ) )
			.ForEachBoard()
			.ActAsync( gs );
	}

	#endregion Level 5

	#region Level 6
	static AdversaryLevel L6 = new AdversaryLevel( 10, 
		4, 5, 4, 
		"The Empire Ascendant", 
		"Setup and During the Explore Step: On boards with 3 or fewer Blight: Add +1 Explorer in each land successfully explored.  ( Max. 2 lands per board per Explore Card.)"
	) {
		InitFunc = ( gs, Level ) => {
			gs.InvaderDeck.Explore.Engine = new EmpireAscendantExploreEngine();
		}
	};

	class EmpireAscendantExploreEngine : ExploreEngine {
		CountDictionary<Board> _bonusExplorers = new CountDictionary<Board>();
		public override async Task ActivateCard( InvaderCard card, GameState gameState ) {
			InitBonusExplorers( gameState );
			await base.ActivateCard( card, gameState );
		}

		void InitBonusExplorers( GameState gameState ) {
			// On boards with 3 or fewer Blight:
			// Add +1 Explorer in each land successfully explored.  
			_bonusExplorers.Clear();
			foreach(var board in gameState.Island.Boards) {
				if(board.Spaces.Tokens().Sum( t => t.Blight.Count ) <= 3)
					// Max. 2 lands per board per Explore Card.
					_bonusExplorers[board] = 2;
			}
		}

		protected override async Task AddToken( SpaceState tokens ) {
			int count = 1;
			var board = tokens.Space.Boards.First();
			if(0 < _bonusExplorers[board]) {
				++count;
				--_bonusExplorers[board];
			}
			await tokens.AddDefault( Human.Explorer, count, AddReason.Explore );
		}

	}

	#endregion Level 6

	// Helper used throught adversary.
	static public bool IsMiningLand( SpaceState space ) => 3 <= space.SumAny( Human.Invader );

}

