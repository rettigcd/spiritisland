
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
			AddActionFactory( new AddBagPresenceOn1Space().ToGrowth() ); // let user pick initial space

		gameState.AddIslandMod( new ResponsibilityToTheDeadMod( this ) );
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

	public class ResponsibilityToTheDeadMod( FinderOfPathsUnseen spirit ) : BaseModEntity, IHandleTokenRemoved, ISerializableSpaceEntity {
		public Task HandleTokenRemovedAsync( ITokenRemovedArgs args ) => spirit.ResponsibilityToTheDead_Handler( args );

		JsonArray ISerializableSpaceEntity.ToJson( ISerializationContext ctx ) => new JsonArray( Tag, ctx.IndexOf( spirit ) );

		const string Tag = "ResponsibilityToTheDeadMod";

		[ModuleInitializer]
		internal static void RegisterSerialization()
			=> SpaceEntitySerialization.Register( Tag, ( json, ctx ) => new ResponsibilityToTheDeadMod( (FinderOfPathsUnseen)ctx.SpiritAt( (int)json[1]! ) ) );
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

	/// <summary> GatewayToken is already ISerializableSpaceEntity and sits on 2 spaces, so
	/// Tokens_ForIsland.FromJson restores an instance there on its own - this only needs enough to find
	/// that live instance again, not reconstruct a second one. Reuses GatewayToken's own ToJson (index 2
	/// is the "_from" space's label) rather than duplicating its field list - pulled out as a plain
	/// string, since a JsonNode already owned by that temporary array can't also be attached here. </summary>
	protected override JsonNode? CustomStateToJson( ISerializationContext ctx )
		=> GatewayToken is null ? null : ( (ISerializableSpaceEntity)GatewayToken ).ToJson( ctx )[2]!.GetValue<string>();

	protected override void RestoreCustomStateFromJson( JsonNode? json, ISerializationContext ctx )
		=> GatewayToken = json is null ? null : ctx.ExistingSpaceEntity<GatewayToken>( ctx.SpaceSpecByLabel( json.GetValue<string>() ) );

	readonly OpenTheWays _openTheWays;

}
