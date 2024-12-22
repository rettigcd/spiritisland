namespace SpiritIsland;

static public class ReplaceInvader {

	#region Downgrade

	public static async Task Downgrade1( Spirit spirit, Space space, Present present, params HumanTokenClass[] groups ) {
		HumanToken[] options = space.HumanOfAnyTag( groups );
		await Downgrade1Token( spirit, space, present, options );
	}

	public static async Task DowngradeAll( Spirit self, Space space, params HumanTokenClass[] groups ) {

		// downgrade any # of invaders
		var invadersThatCanBeDowngraded = space.HumanOfAnyTag( groups )
			.ToDictionary( t => t, t => space[t] )
			.ToCountDict();

		HumanToken[] options = [..invadersThatCanBeDowngraded.Keys];
		while(0 < options.Length) {
			var oldInvader = await Downgrade1Token( self, space, Present.Done, options );
			if(oldInvader == null) break;
			// this one downgraded, can't use again
			invadersThatCanBeDowngraded[oldInvader]--;
			// next
			options = [..invadersThatCanBeDowngraded.Keys];
		}
	}

	/// <summary> Offers Specific tokens, instead of token classes. </summary>
	/// <returns> Original token (before downgrade).</returns>
	static async Task<HumanToken?> Downgrade1Token( Spirit spirit, Space space, Present present, HumanToken[] options ) {
		var st = await spirit.Select( An.Invader.ToReplace( "downgrade", options.On(space), present ) );
		if(st is null) return null;
		HumanToken oldInvader = st.Token.AsHuman();

		await DowngradeSelectedInvader( space, oldInvader );
		return oldInvader;
	}

	public static async Task<HumanToken?> DowngradeSelectedInvader( Space space, HumanToken oldInvader ) {

		// Explorers just get removed
		if(oldInvader.HumanClass == Human.Explorer) {
			space.Adjust( oldInvader, -1 );
			return null;
		}

		TokenReplacedArgs replaced = await space.ReplaceHumanAsync( oldInvader, DowngradeType( oldInvader.HumanClass ) );
		return replaced.Added.AsHuman();
	}

	static HumanTokenClass DowngradeType( HumanTokenClass orig ) => orig == Human.City ? Human.Town
		: orig == Human.Town ? Human.Explorer
		: throw new ArgumentOutOfRangeException( nameof( orig ), "Must be Town or City." );

	#endregion Downgrade

	#region Upgrade

	/// <param name="oldInvader">Explorer or Town</param>
	/// <exception cref="ArgumentOutOfRangeException"></exception>
	public static async Task UpgradeSelectedInvader( Space space, HumanToken oldInvader ) {
		await space.ReplaceHumanAsync( oldInvader, UpgradeType( oldInvader.HumanClass ) );
	}

	/// <summary> Offers Specific tokens, instead of token classes. </summary>
	/// <returns> Original token (before upgrade).</returns>
	static public async Task<HumanToken?> Upgrade1Token( 
		Spirit spirit, 
		Space space, 
		Present present, 
		HumanToken[] options, 
		string actionSuffix = "" 
	) {
		var st = await spirit.Select( An.Invader.ToReplace( "upgrade" + actionSuffix, options.On( space ), present ) );
		if(st is null) return null;
		HumanToken oldInvader = st.Token.AsHuman();

		await UpgradeSelectedInvader( space, oldInvader );
		return oldInvader;
	}

	static HumanTokenClass UpgradeType( HumanTokenClass orig ) => orig == Human.Explorer ? Human.Town
			: orig == Human.Town ? Human.City
			: throw new ArgumentOutOfRangeException( nameof( orig ), "Must be Explorer or Town." );


	#endregion Upgrade

	#region Disolve

	/// <summary>
	/// Disolves Tows/Cities into explorers.
	/// </summary>
	public static async Task DisolveInvaderIntoExplorers( TargetSpaceCtx ctx, HumanTokenClass oldInvader, int replaceCount ) {

		var tokens = ctx.Space;
		var st = await ctx.Self.Select( An.Invader.ToReplace("disolve", tokens.HumanOfTag( oldInvader ).On(ctx.Space) ) );
		if(st is null) return;
		var tokenToRemove = st.Token.AsHuman();

		int explorersToAdd = replaceCount - tokenToRemove.Damage; // ignore nightmare damage because it can't really destory stuff

		await tokens.ReplaceAsync( tokenToRemove, explorersToAdd, tokens.GetDefault(Human.Explorer));

		// ! This is an approximation, to perfectly distribute strife, need to be able to replace with multiple types.
		await ctx.AddStrife( tokenToRemove.StrifeCount, Human.Explorer );
	}

	#endregion Disolve

	#region > Dahan

	public static async Task WithDahanAsync( Space space, HumanTokenClass[] invaderTypes ) {
		var tokenToRemove = space.BestInvaderToBeRidOf( invaderTypes );
		if(tokenToRemove is not null) {

			var result = await space.RemoveAsync(tokenToRemove,1, RemoveReason.Replaced);
			if(0<result.Count)
				await space.AddDefaultAsync( Human.Dahan, 1, AddReason.AsReplacement );

		}
	}

	#endregion > Dahan

}