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

	void IHandleTokenAdded.HandleTokenAdded( ITokenAddedArgs args ) {
		Fortify_AddedPresence( args );
	}

	async Task IModifyRemovingTokenAsync.ModifyRemovingAsync( RemovingTokenArgs args ) {
		await Foritfy_RemovingDahanAsync( args);
	}

	public override async Task HandleTokenRemovedAsync( ITokenRemovedArgs args ) {
		await base.HandleTokenRemovedAsync( args );
		await Foritfy_RemovedPresenceAsync( args );
		await LoyalGuardian_DahanMoved( args );
	}

	#region Loyal Guardian
	static public SpecialRule LoyalGuardian => new SpecialRule( "Loyal Guardian", "When all Dahan leave one of your lands, your presence may Move with those Dahan. (Each Dahan can Bring any number of Presence." );

	public async Task LoyalGuardian_DahanMoved( ITokenRemovedArgs args ) {

		if(args is not ITokenMovedArgs moved || args.Removed.Class != Human.Dahan) return;

		var destinations = ActionScope.Current.SafeGet("LoyalDestinations",()=>new List<SpaceState>());
		destinations.Add( moved.To );
		if(!args.From.Dahan.Any)
			await PushUpToAllPresence(args.From,destinations);
	}

	async Task PushUpToAllPresence( SpaceState from, IEnumerable<SpaceState> destinationOptions ) {

		while(Self.Presence.IsOn(from)) {
			// #pushpresence
			Space destination = await Self.SelectAsync( A.Space.ToPushPresence( from.Space, destinationOptions.Downgrade(), Present.Done, this ) );
			if(destination is null) break;

			// apply...
			await from.MoveTo( this, destination );
		}
	}

	#endregion Loyal Guardian

	#region Fortify Heart and Hearth

	static public SpecialRule FortifyHeart => new SpecialRule( "Fortify Heart and Hearth", "Dahan have +4 Health (each) while in your lands.  Event and Blight Card Actions don't damage, destroy, or replace dahan in your lands." );

	const int _deltaHealth = 4;

	static bool BonusAppliesToThis(HumanToken token) => token.HumanClass.HasTag(TokenCategory.Dahan);

	void Fortify_AddedPresence( ITokenAddedArgs args ) {
		// Adding presence where there wasn't any before.
		if(args.Added == this && args.To[this] == args.Count)
			GrantHealthBoost( args.To );
	}

	public static void GrantHealthBoost( SpaceState to ) {
		foreach(HumanToken token in to.HumanOfTag(TokenCategory.Dahan).ToArray()) {
			to.Adjust( token.AddHealth( _deltaHealth ), to[token] );
			to.Init( token, 0 );
		}
	}

	/// <summary> Intercepts in-coming dahan and grants them additional health. </summary>
	static void Fortify_AddingDahan( AddingTokenArgs args ) {
		// !!! Any Dahan added with .Init() will not get this added health bonus.  'Call of the Dahan Ways'????
		// Adding a Dahan
		if(args.Token is HumanToken healthToken && BonusAppliesToThis(healthToken))
			args.Token = healthToken.AddHealth( _deltaHealth );
	}

	/// <summary> Intercepts out-going dahan and returns them to normal health. </summary>
	static async Task Foritfy_RemovingDahanAsync( RemovingTokenArgs args ) {
		if(args.Mode == RemoveMode.Test) return;

		// Removing Dahan
		if(args.Token is HumanToken healthToken && BonusAppliesToThis( healthToken ))
			// Downgrade the existing tokens health
			// AND change what we are removing to be the downgraded token
			// tokens being destroyed may reduce the count also.
			(args.Token, args.Count) = await args.From.AdjustHealthOf( healthToken, -_deltaHealth, args.Count );
	}

	async Task Foritfy_RemovedPresenceAsync( ITokenRemovedArgs args ) {
		// Removing Last Presence
		if(args.Removed == this && args.From[this] == 0)
			foreach(HumanToken token in args.From.HumanOfTag(TokenCategory.Dahan).ToArray())
				await args.From.AdjustHealthOf( token, -_deltaHealth, args.From[token] );
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