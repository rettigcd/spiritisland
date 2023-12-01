namespace SpiritIsland.NatureIncarnate;

public class UnearthABeastOfWrathfulStone {

	public const string Name = "Unearth a Beast of Wrathful Stone";

	[MajorCard(Name,5,"moon,fire,earth,animal"),Fast]
	[FromSacredSite(1,Target.Invaders)]
	[Instructions( "After the next Invader Phase with no Ravage/Build Actions in target land: 3 Fear. 12 Damage. Add 1 Beast. You may Push that Beast. 1 Fear and 2 Damage in its land. -If you have- 2 Moon,3 Earth,3 Animal: Mark it. Marked Beast can't leave the island. Each Slow phase: You may Push Marked Beast. 1 Fear and 2 Damage at Marked Beast" ), Artist( Artists.DavidMarkiwsky )]
	static public async Task ActAsync(TargetSpaceCtx ctx){

		// -If you have- 2 Moon,3 Earth,3 Animal:
		IToken beastToken = await ctx.YouHave("2 moon,3 earth, 3 animal") // determine threshold now, not later.
			// Mark it. Marked Beast can't leave the island.
			// Each Slow phase: You may Push Marked Beast. 1 Fear and 2 Damage at Marked Beast
			? new MarkedBeast(ctx.Self)
			: Token.Beast;

		// After the next Invader Phase with no Ravage/Build Actions in target land:
		ctx.Tokens.Adjust(new TriggerAfterNoRavageOrBuild( ctx.Self, TriggeredAction ),1);

		async Task TriggeredAction(TargetSpaceCtx ctxx ) {
			// 3 Fear.
			ctx.AddFear(3);

			// 12 Damage.
			await ctx.DamageInvaders( 12 );

			// Add 1 Beast. (either normal or the marked one)
			await ctx.Tokens.AddAsync( beastToken, 1 );

			// You may Push that Beast.
			await ctx.SourceSelector
				.AddGroup( 1, Token.Beast )
				.FilterSpaceToken( st => st.Token == beastToken )
				.ConfigDestination(d=>d.Track( async to => {
					// 1 Fear and 2 Damage in its land.
					GameState.Current.Fear.AddDirect( new FearArgs( 1 ) { space = to.Space } );
					await to.DamageInvaders( ctxx.Self, 2 );
				} ))
				.PushN( ctx.Self );
		}
		
	}

}

/// <summary>
/// Sits on a space waiting for there to be no Ravage nor build,
/// Then adds the correct beast token.
/// </summary>
class TriggerAfterNoRavageOrBuild : ISpaceEntity, ISkipBuilds, IConfigRavages, IRunAfterInvaderPhase {
	
	readonly Spirit _spirit;
	readonly Func<TargetSpaceCtx,Task> _triggeredAction;

	public TriggerAfterNoRavageOrBuild(Spirit spirit, Func<TargetSpaceCtx,Task> triggeredAction) {
		_spirit = spirit;
		_triggeredAction = triggeredAction;
	}

	bool _hadRavageOrBuild;


	public string Text => "Tigger Action following no-build-nor-ravage.";

	#region detect build or ravage

	UsageCost ISkipBuilds.Cost => UsageCost.Extreme; // tries to go last
	Task<bool> ISkipBuilds.Skip( SpaceState space ) {  _hadRavageOrBuild = true; return Task.FromResult(false);}

	void IConfigRavages.Config( SpaceState space ) { _hadRavageOrBuild = true; }

	#endregion detect build or ravage

	async Task IRunAfterInvaderPhase.ActAsync( SpaceState space ) {
		if(_hadRavageOrBuild) { _hadRavageOrBuild = false; return; }

		// Do action
		await using ActionScope actionScope = await ActionScope.StartSpiritAction( ActionCategory.Spirit_Power, _spirit );
		await _triggeredAction( _spirit.Bind().Target( space ) );

		// Remove
		space.Adjust( this, -1 ); // !!! what happens if we put 2 of these down?
	}

}

/// <summary>
/// Beast created from Unearth-a-Beast-of-wrathful-stone
/// </summary>
/// <remarks>
/// Is both the Beast-token AND is the ActionFactory that pushes it each slow phase.
/// </remarks>
public class MarkedBeast : IToken
	, IModifyRemovingToken
	, IHandleTokenAdded
	, IAppearInSpaceAbreviation
	, IActionFactory
{
	public MarkedBeast(Spirit controlSpirit) {
		
		// Each Slow phase: You may Push Marked Beast. 1 Fear and 2 Damage at Marked Beast
		controlSpirit.EnergyCollected.Add( AddSlowPushToSpirit );

	}

	#region IToken stuff
	public Img Img => Img.Beast;
	ITokenClass IToken.Class => Token.Beast;
	bool IToken.HasTag( ITag tag ) => Token.Beast.HasTag( tag );
	public string Text => "Marked-Beast";
	public string SpaceAbreviation => Text;

	#endregion

	#region Cannot be Removed from island
	void IModifyRemovingToken.ModifyRemoving( RemovingTokenArgs args ) {
		// cannot be removed from the island.
		if(args.Token==this && args.Reason != RemoveReason.MovedFrom)
			args.Count = 0;
	}
	#endregion

	#region IActionFactory - push token every slow

	void AddSlowPushToSpirit(Spirit s ) {
		if(_spaceState != null) // if the beast has been placed on the board.
			s.AddActionFactory( this );
	}

	string IActionFactory.Name => "Push Marked Beast";
	public bool CouldActivateDuring( Phase speed, Spirit spirit ) => speed == Phase.Slow;
	public async Task ActivateAsync( SelfCtx ctx ) {
		await _spaceState!.SourceSelector
			.AddGroup(1,Token.Beast)
			.FilterSpaceToken(st=>st.Token==this)
			.PushUpToN(ctx.Self);
		// 1 Fear and 2 Damage in its land.
		GameState.Current.Fear.AddDirect(new FearArgs(1) { space = _spaceState!.Space }); // don't cache space-state, it might have moved
		await _spaceState.DamageInvaders(ctx.Self,2);
	}
	#endregion

	#region Track Token Space
	void IHandleTokenAdded.HandleTokenAdded( ITokenAddedArgs args ) {
		if(args.Added == this) _spaceState = args.To;
	}
	#endregion

	#region private fields
	SpaceState? _spaceState;
	#endregion

}