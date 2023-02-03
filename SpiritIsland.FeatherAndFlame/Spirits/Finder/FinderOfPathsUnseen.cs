namespace SpiritIsland.FeatherAndFlame;

public class FinderOfPathsUnseen : Spirit {

	public const string Name = "Finder Of Paths Unseen";

	public override string Text => Name;

	public override SpecialRule[] SpecialRules => new SpecialRule[] { ResponsibilityToTheDead_Rule, OpenTheWays.Rule };

	#region constructor / initilization

	public FinderOfPathsUnseen() : base(
		new FinderPresence()
		, PowerCard.For<ACircuitousAndWendingJourney>()
		, PowerCard.For<AidFromTheSpiritSpeakers>()
		, PowerCard.For<OfferPassageBetweenWorlds>()
		, PowerCard.For<PathsTiedByNature>()
		, PowerCard.For<TravelersBoon>()
		, PowerCard.For<WaysOfShoreAndHeartland>()
	) {
		GrowthTrack = new(
			new GrowthOption(
				new ReclaimAll(),
				new DrawPowerCard( 1 ),
				new IgnoreRange()
			),
			new GrowthOption(
				new PlacePresence( 1 ),
				new PlayExtraCardThisTurn( 1 )
			),
			new GrowthOption(
				new DrawPowerCard( 1 ),
				new PlacePresence( 2 )
			),
			new GrowthOption(
				new PlacePresence( 100 ),
				new GainEnergy( 2 )
			)
		);

		InnatePowers = new[] {
			InnatePower.For<LayPathsTheyCannotHelpButWalk>(),
			InnatePower.For<CloseTheWays>()
		};

	}

	protected override void InitializeInternal( Board board, GameState gameState ) {

		// Put 1 PResence on your starting board in land #3.
		Presence.Adjust( gameState.Tokens[board[3]], 1 );

		// Put 1 presence on any board in land #1.
		// !!! change this to a setup-action where spirit places a presence on space 1 of 1 board.
		Presence.Adjust( gameState.Tokens[board[1]], 1 );

		gameState.AddToAllActiveSpaces( new TokenRemovedHandler( ResponsibilityToTheDead_Handler, true ) );

		_openTheWays = new OpenTheWays( this );
		gameState.EndOfAction.ForGame.Add( _openTheWays.CheckPresenceAtBothEnds );

	}

	#endregion

	#region Responsibility To The Dead

	static readonly SpecialRule ResponsibilityToTheDead_Rule = new SpecialRule(
		"Responsibilities to the Dead",
		"After one of your Actions Destroys 1 or more Dahan/Invaders, or directly triggers their Destruction my moving them, Destroy 1 of your Presnce and lose 1 Energy.  If you have no Energy to lose, Destroy another Presence."
	);
	async Task ResponsibilityToTheDead_Handler( ITokenRemovedArgs args ) {
		const string AllReadyDestroyedSomePresence = "FinderAlreadyDestroyedPresence";
		if(args.ActionScope.ContainsKey( AllReadyDestroyedSomePresence )
			// After one of your Actions
			&& args.ActionScope.Owner == this
			// Destroys 1 or more
			&& args.Reason == RemoveReason.Destroyed
			// Dahan/Invaders,
			&& args.Token.Class.Category.IsOneOf( TokenCategory.Invader, TokenCategory.Dahan )
		// or directly triggers their Destruction my moving them,
		) {
			// Destroy 1 of your Presnce and lose 1 Energy.
			int preseneceToDestroy = 1;
			// If you have no Energy to lose, Destroy another Presence.
			if(Energy > 0) --Energy; else ++preseneceToDestroy;

			// Do presence destroy
			var gameState = args.RemovedFrom.AccessGameState();
			var presence = new BoundPresence( this, gameState, gameState.Island.Terrain_ForPower, args.ActionScope );
			while(preseneceToDestroy-- > 0)
				await presence.DestroyOneFromAnywhere( DestoryPresenceCause.SpiritPower );

			// only once per action
			args.ActionScope[AllReadyDestroyedSomePresence] = true;
		}
	}

	#endregion Responsibility To The Dead

	public override IEnumerable<IActionFactory> GetAvailableActions( Phase speed ) {
		var normalActions = base.GetAvailableActions( speed ).ToList();
		if(normalActions.Any())
			normalActions.Add( _openTheWays );
		return normalActions;
	}

	public override void RemoveFromUnresolvedActions( IActionFactory selectedActionFactory ) {
		if(selectedActionFactory != _openTheWays)
			base.RemoveFromUnresolvedActions( selectedActionFactory );
	}

	OpenTheWays _openTheWays;

}
