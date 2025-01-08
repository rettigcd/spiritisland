namespace SpiritIsland.NatureIncarnate;

public class HearthToken( Spirit spirit ) 
	: SpiritPresenceToken(spirit)
	, IModifyAddingToken	, IHandleTokenAdded
	, IModifyRemovingToken	, IHandleTokenRemoved
{
	Task IModifyAddingToken.ModifyAddingAsync( AddingTokenArgs args ) {
		RootedInCommunity_Adding( args );
		Fortify_AddingDahan( args );
		return Task.CompletedTask;
	}

	Task IHandleTokenAdded.HandleTokenAddedAsync( Space to, ITokenAddedArgs args ) {
		Fortify_AddedPresence( to, args );
		return Task.CompletedTask;
	}

	async Task IModifyRemovingToken.ModifyRemovingAsync( RemovingTokenArgs args ) {
		await Foritfy_RemovingDahanAsync( args);
	}

	public override async Task HandleTokenRemovedAsync( ITokenRemovedArgs args ) {
		await base.HandleTokenRemovedAsync( args );
		var from = (Space)args.From;
		await Foritfy_RemovedPresenceAsync( args, from );
		await LoyalGuardian_DahanMoved( args, from );
	}

	#region Loyal Guardian
	static public SpecialRule LoyalGuardian => new SpecialRule( "Loyal Guardian", "When all Dahan leave one of your lands, your presence may Move with those Dahan. (Each Dahan can Bring any number of Presence." );

	public async Task LoyalGuardian_DahanMoved( ITokenRemovedArgs args, Space from ) {

		if(args is not ITokenMovedArgs moved 
			|| args.Removed.Class != Human.Dahan
			|| moved.To is not Space to
		) return;

		var destinations = ActionScope.Current.SafeGet("LoyalDestinations",()=>new HashSet<Space>()); // Hash so we don't have to do .Distinct
		destinations.Add( to );
		if(!from.Dahan.Any)
			await BringUpToAllPresence(from,destinations);
	}

	async Task BringUpToAllPresence( Space from, IEnumerable<Space> destinationOptions ) {

		await new TokenMover( Self, "Bring",
			from.SourceSelector.UseQuota(new Quota().AddAll( Self.Presence )),
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

	static bool BonusAppliesToThis(HumanToken token) => token.HasTag(TokenCategory.Dahan);

	void Fortify_AddedPresence( Space to, ITokenAddedArgs args ) {
		// Adding presence where there wasn't any before.
		if(args.Added == this && to[this] == args.Count)
			GrantHealthBoost( to );
	}

	public static void GrantHealthBoost( Space to ) {
		foreach(HumanToken origDahan in to.HumanOfTag(TokenCategory.Dahan).ToArray()) {
			var result = to.AllHumans(origDahan).Adjust(BoostHealth);
			ActionScope.Current.LogDebug( $"Adjusting {result.Count} {result.OldToken.SpaceAbreviation} to {result.NewToken.SpaceAbreviation}" );
		}
	}
	static HumanToken BoostHealth( HumanToken x ) => x.AddHealth( _deltaHealth );


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
		if(args.Token is HumanToken healthToken && BonusAppliesToThis( healthToken )) {
			// Downgrade the existing tokens health
			// AND change what we are removing to be the downgraded token
			// tokens being destroyed may reduce the count also.
			var result = await args.From.Humans( args.Count, healthToken )
				.AdjustHealthAsync( -_deltaHealth );
			args.Token = result.NewToken; 
			args.Count = result.Count;
		}
	}

	async Task Foritfy_RemovedPresenceAsync( ITokenRemovedArgs args, Space from ) {
		// Removing Last Presence
		if(args.Removed == this && from[this] == 0)
			foreach(HumanToken token in from.HumanOfTag(TokenCategory.Dahan).ToArray())
				await from.AllHumans( token ).AdjustHealthAsync( -_deltaHealth );
	}

	#endregion Fortify Heart and Hearth

	#region Rooted in Community
	static public SpecialRule Rooted => new SpecialRule( "Rooted in the Community", "Blight added in your lands does not destroy your presence if Dahan are present. (Ravage destroys dahan before blight destorys presence and cascades.)" );

	static void RootedInCommunity_Adding( AddingTokenArgs args ) {
		// if Dahan are present.
		if(args.Token == Token.Blight && 0<args.To.Dahan.CountAll)
			// Blight added in your lands does not destroy your presence
			BlightToken.ScopeConfig.DestroyPresence = false;
	}

	#endregion Rooted in Community
}