
namespace SpiritIsland.FeatherAndFlame;

public class FinderOfPathsUnseen : Spirit, ISpiritMod, IModifyAvailableActions {

	public const string Name = "Finder Of Paths Unseen";

	public override string SpiritName => Name;

	public GatewayToken? GatewayToken;

	#region constructor / initilization

	public FinderOfPathsUnseen() : base(
		spirit => new FinderPresence( spirit )
		, new GrowthTrack(
			new GrowthGroup(
				new ReclaimAll(),
				new GainPowerCard(),
				new IgnoreRange()
			),
			new GrowthGroup(
				new PlacePresence( 1 ),
				new PlayExtraCardThisTurn( 1 )
			),
			new GrowthGroup(
				new GainPowerCard(),
				new PlacePresence( 2 )
			),
			new GrowthGroup(
				new PlacePresence(),
				new GainEnergy( 2 )
			)
		)
		, PowerCard.ForDecorated(AidFromTheSpiritSpeakers.ActAsync)		// fast - 2
		, PowerCard.ForDecorated(OfferPassageBetweenWorlds.ActAsync)		// fast - 1
		, PowerCard.ForDecorated(TravelersBoon.ActAsync)					// fast - 0
		, PowerCard.ForDecorated(WaysOfShoreAndHeartland.ActAsync)		// slow - 1
		, PowerCard.ForDecorated(ACircuitousAndWendingJourney.ActAsync)	// slow - 0
		, PowerCard.ForDecorated(PathsTiedByNature.ActAsync)				// slow - 0
	) {
		InnatePowers = [
			InnatePower.For(typeof(LayPathsTheyCannotHelpButWalk)),
			InnatePower.For(typeof(CloseTheWays))
		];
		SpecialRules = [ResponsibilityToTheDead_Rule, OpenTheWays.Rule];
		_openTheWays = new OpenTheWays();
		Mods.Add(this);
	}

	protected override void InitializeInternal( Board board, GameState gameState ) {

		// Put 1 Presence on your starting board in land #3.
		board[3].ScopeSpace.Setup(Presence.Token, 1);

		// Put 1 presence on any board in land #1.
		if(gameState.Island.Boards.Length == 1)
			board[1].ScopeSpace.Setup(Presence.Token, 1);
		else 
			AddActionFactory( new PlacePresenceOnSpace1().ToGrowth() ); // let user pick initial space

		gameState.AddIslandMod( new TokenRemovedHandlerAsync_Persistent( ResponsibilityToTheDead_Handler ) );
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

	void IModifyAvailableActions.Modify(List<IActionFactory> orig, Phase phase) {
		if( orig.Count != 0 )
			orig.Add(_openTheWays);
	}

	protected override object? CustomMementoValue {
		get => GatewayToken ?? new object();
		set => GatewayToken = (GatewayToken?)value;
	}

	readonly OpenTheWays _openTheWays;

}
