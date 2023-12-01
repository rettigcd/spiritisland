namespace SpiritIsland.FeatherAndFlame;

public class FinderOfPathsUnseen : Spirit {

	public const string Name = "Finder Of Paths Unseen";

	public override string Text => Name;

	public override SpecialRule[] SpecialRules => new SpecialRule[] { ResponsibilityToTheDead_Rule, OpenTheWays.Rule };

	public GatewayToken GatewayToken;

	protected override object _customSaveValue {
		get => GatewayToken;
		set => GatewayToken = (GatewayToken)value;
	}

	#region constructor / initilization

	public FinderOfPathsUnseen() : base(
		spirit => new FinderPresence( spirit )
		, PowerCard.For(typeof(ACircuitousAndWendingJourney))
		, PowerCard.For(typeof(AidFromTheSpiritSpeakers))
		, PowerCard.For(typeof(OfferPassageBetweenWorlds))
		, PowerCard.For(typeof(PathsTiedByNature))
		, PowerCard.For(typeof(TravelersBoon))
		, PowerCard.For(typeof(WaysOfShoreAndHeartland))
	) {
		GrowthTrack = new(
			new GrowthOption(
				new ReclaimAll(),
				new GainPowerCard(),
				new IgnoreRange()
			),
			new GrowthOption(
				new PlacePresence( 1 ),
				new PlayExtraCardThisTurn(1)
			),
			new GrowthOption(
				new GainPowerCard(),
				new PlacePresence( 2 )
			),
			new GrowthOption(
				new PlacePresence( 100 ),
				new GainEnergy(2)
			)
		);

		InnatePowers = new[] {
			InnatePower.For(typeof(LayPathsTheyCannotHelpButWalk)),
			InnatePower.For(typeof(CloseTheWays))
		};

	}

	protected override void InitializeInternal( Board board, GameState gameState ) {

		// Put 1 Presence on your starting board in land #3.
		board[3].Tokens.Adjust(Presence.Token, 1);

		// Put 1 presence on any board in land #1.
		AddActionFactory( new PlacePresenceOnSpace1().ToInit() ); // let user pick initial space

		gameState.AddIslandMod( new TokenRemovedHandlerAsync_Persistent( ResponsibilityToTheDead_Handler ) );

		_openTheWays = new OpenTheWays();

	}

	#endregion

	#region Responsibility To The Dead

	static readonly SpecialRule ResponsibilityToTheDead_Rule = new SpecialRule(
		"Responsibilities to the Dead",
		"After one of your Actions Destroys 1 or more Dahan/Invaders, or directly triggers their Destruction my moving them, Destroy 1 of your Presnce and lose 1 Energy.  If you have no Energy to lose, Destroy another Presence."
	);
	Task ResponsibilityToTheDead_Handler( ITokenRemovedArgs args ) {
		const string AllReadyDestroyedSomePresence = "FinderAlreadyDestroyedPresence";
		var scope = ActionScope.Current;
		if( !scope.ContainsKey( AllReadyDestroyedSomePresence )
			// After one of your Actions
			&& scope.Owner == this
			// Destroys 1 or more
			&& args.Reason == RemoveReason.Destroyed
			// Dahan/Invaders,
			&& args.Removed.HasAny(TokenCategory.Invader, TokenCategory.Dahan)
			// or directly triggers their Destruction my moving them,
		) {
			// only once per action
			scope[AllReadyDestroyedSomePresence] = true;

			// After
			scope.AtEndOfThisAction( async action => {
				// Destroy 1 of your Presnce and lose 1 Energy.
				int presenceToDestroy = 1;
				// If you have no Energy to lose, Destroy another Presence.
				if(0 < Energy) --Energy; else ++presenceToDestroy;

				// Do presence destroy
				while(0 < presenceToDestroy--)
					await Cmd.DestroyPresence().ActAsync(this);
			} );

		}
		return Task.CompletedTask;
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
