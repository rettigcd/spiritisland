namespace SpiritIsland.NatureIncarnate;

public class HearthToken : SpiritPresenceToken
	, IModifyAddingToken
	, IHandleTokenAdded
	, IModifyRemovingTokenAsync
	, IHandleTokenRemovedAsync
{

	public HearthToken(Spirit spirit)
		:base(spirit)
	{
	}

	void IModifyAddingToken.ModifyAdding( AddingTokenArgs args ) {
		RootedInCommunity_Adding( args );
		Fortify_AddingDahan( args );
	}

	void IHandleTokenAdded.HandleTokenAdded( SpaceState to, ITokenAddedArgs args ) {
		Fortify_AddedPresence( to, args );
	}

	async Task IModifyRemovingTokenAsync.ModifyRemovingAsync( RemovingTokenArgs args ) {
		await Foritfy_RemovingDahanAsync( args);
	}

	public override async Task HandleTokenRemovedAsync( SpaceState from, ITokenRemovedArgs args ) {
		await base.HandleTokenRemovedAsync( from, args );
		await Foritfy_RemovedPresenceAsync( args, from );
		await LoyalGuardian_DahanMoved( args, from );
	}

	#region Loyal Guardian
	static public SpecialRule LoyalGuardian => new SpecialRule( "Loyal Guardian", "When all Dahan leave one of your lands, your presence may Move with those Dahan. (Each Dahan can Bring any number of Presence." );

	public async Task LoyalGuardian_DahanMoved( ITokenRemovedArgs args, SpaceState from ) {

		if(args is not ITokenMovedArgs moved 
			|| args.Removed.Class != Human.Dahan
			|| moved.To is not Space to
		) return;

		var destinations = ActionScope.Current.SafeGet("LoyalDestinations",()=>new HashSet<SpaceState>()); // Hash so we don't have to do .Distinct
		destinations.Add( to.Tokens );
		if(!from.Dahan.Any)
			await BringUpToAllPresence(from,destinations);
	}

	async Task BringUpToAllPresence( SpaceState from, IEnumerable<SpaceState> destinationOptions ) {

		await new TokenMover( Self, "Bring",
			from.SourceSelector.AddAll( Self.Presence ),
			new DestinationSelector( destinationOptions )
		).DoUpToN();

	}

	#endregion Loyal Guardian

	#region Fortify Heart and Hearth

	static public SpecialRule FortifyHeart => new SpecialRule( 
		"Fortify Heart and Hearth", 
		"Dahan have +4 Health (each) while in your lands.  Event and Blight Card Actions don't damage, destroy, or replace dahan in your lands." 
	);

	const int _deltaHealth = 4;

	static bool BonusAppliesToThis(HumanToken token) => token.HumanClass.HasTag(TokenCategory.Dahan);

	void Fortify_AddedPresence( SpaceState to, ITokenAddedArgs args ) {
		// Adding presence where there wasn't any before.
		if(args.Added == this && to[this] == args.Count)
			GrantHealthBoost( to );
	}

	public static void GrantHealthBoost( SpaceState to ) {
		foreach(HumanToken origDahan in to.HumanOfTag(TokenCategory.Dahan).ToArray()) {
			var newToken = UpgradeDahanAndLog( origDahan, to[origDahan] );
			to.Adjust( newToken, to[origDahan] );
			to.Init( origDahan, 0 );
		}
	}

	/// <summary> Intercepts in-coming dahan and grants them additional health. </summary>
	static void Fortify_AddingDahan( AddingTokenArgs args ) {
		// !!! Any Dahan added with .Init() will not get this added health bonus.  'Call of the Dahan Ways'????
		// Adding a Dahan
		if(args.Token is HumanToken healthToken && BonusAppliesToThis( healthToken ))
			args.Token = UpgradeDahanAndLog( healthToken, args.Count );
	}

	static HumanToken UpgradeDahanAndLog( HumanToken healthToken, int count ) {
		var newToken = healthToken.AddHealth( _deltaHealth );
		ActionScope.Current.LogDebug( $"Adjusting {count} {healthToken.SpaceAbreviation} to {newToken.SpaceAbreviation}" );
		return newToken;
	}

	/// <summary> Intercepts out-going dahan and returns them to normal health. </summary>
	static async Task Foritfy_RemovingDahanAsync( RemovingTokenArgs args ) {

		// Removing Dahan
		if(args.Token is HumanToken healthToken && BonusAppliesToThis( healthToken ))
			// Downgrade the existing tokens health
			// AND change what we are removing to be the downgraded token
			// tokens being destroyed may reduce the count also.
			(args.Token, args.Count) = await args.From.AdjustHealthOf( healthToken, -_deltaHealth, args.Count );
	}

	async Task Foritfy_RemovedPresenceAsync( ITokenRemovedArgs args, SpaceState from ) {
		// Removing Last Presence
		if(args.Removed == this && from[this] == 0)
			foreach(HumanToken token in from.HumanOfTag(TokenCategory.Dahan).ToArray())
				await from.AdjustHealthOf( token, -_deltaHealth, from[token] );
	}

	#endregion Fortify Heart and Hearth

	#region Rooted in Community
	static public SpecialRule Rooted => new SpecialRule( "Rooted in the Community", "Blight added in your lands does not destroy your presence if Dahan are present. (Ravage destroys dahan before blight destorys presence and cascades.)" );

	static void RootedInCommunity_Adding( AddingTokenArgs args ) {
		// if Dahan are present.
		if(args.Token == Token.Blight && 0<args.To.Dahan.CountAll)
			// Blight added in your lands does not destroy your presence
			BlightToken.ForThisAction.DestroyPresence = false;
	}

	#endregion Rooted in Community
}