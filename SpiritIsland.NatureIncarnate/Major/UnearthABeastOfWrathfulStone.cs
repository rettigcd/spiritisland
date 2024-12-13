namespace SpiritIsland.NatureIncarnate;

public class UnearthABeastOfWrathfulStone {

	public const string Name = "Unearth a Beast of Wrathful Stone";

	[MajorCard(Name,5,"moon,fire,earth,animal"),Fast]
	[FromSacredSite(1,Filter.Invaders)]
	[Instructions( "After the next Invader Phase with no Ravage/Build Actions in target land: 3 Fear. 12 Damage. Add 1 Beast. You may Push that Beast. 1 Fear and 2 Damage in its land. -If you have- 2 Moon,3 Earth,3 Animal: Mark it. Marked Beast can't leave the island. Each Slow phase: You may Push Marked Beast. 1 Fear and 2 Damage at Marked Beast" ), Artist( Artists.DavidMarkiwsky )]
	static public async Task ActAsync(TargetSpaceCtx ctx){

		// -If you have- 2 Moon,3 Earth,3 Animal:
		IToken beastToken = await ctx.YouHave("2 moon,3 earth, 3 animal") // determine threshold now, not later.
			// Mark it. Marked Beast can't leave the island.
			// Each Slow phase: You may Push Marked Beast. 1 Fear and 2 Damage at Marked Beast
			? new MarkedBeast(ctx.Self)
			: Token.Beast;

		// After the next Invader Phase with no Ravage/Build Actions in target land:
		var noRavageOrBuildTrigger = new TriggerAfterNoRavageOrBuild( ctx.Self, TriggeredAction );
		ctx.Space.Adjust(noRavageOrBuildTrigger,1);
		GameState.Current.AddPostInvaderPhase( noRavageOrBuildTrigger );

		async Task TriggeredAction(TargetSpaceCtx ctxx ) {
			// 3 Fear.
			await ctx.AddFear(3);

			// 12 Damage.
			await ctx.DamageInvaders( 12 );

			// Add 1 Beast. (either normal or the marked one)
			await ctx.Space.AddAsync( beastToken, 1 );

			// You may Push that Beast.
			await ctx.SourceSelector
				.AddGroup( 1, Token.Beast )
				.FilterSpaceToken( st => st.Token == beastToken )
				.ConfigDestination(d=>d.Track( async to => {
					// 1 Fear and 2 Damage in its land.
					await to.AddFear(1);
					await to.UserSelected_DamageInvadersAsync( ctxx.Self, 2 );
				} ))
				.PushN( ctx.Self );
		}
		
	}

}

/// <summary>
/// Sits on a space waiting for there to be no Ravage nor build,
/// Then adds the correct beast token.
/// </summary>
class TriggerAfterNoRavageOrBuild( Spirit spirit, Func<TargetSpaceCtx, Task> triggeredAction ) : ISpaceEntity, ISkipBuilds, IConfigRavages, IRunAfterInvaderPhase {
	
	readonly Spirit _spirit = spirit;
	readonly Func<TargetSpaceCtx,Task> _triggeredAction = triggeredAction;
	bool _hadRavageOrBuild;


	public string Text => "Tigger Action following no-build-nor-ravage.";

	#region detect build or ravage

	UsageCost ISkipBuilds.Cost => UsageCost.Extreme; // tries to go last

	Task<bool> ISkipBuilds.Skip( Space space ) {  _hadRavageOrBuild = true; return Task.FromResult(false);}

	void IConfigRavages.Config( Space space ) { _hadRavageOrBuild = true; }

	#endregion detect build or ravage

	#region IRunAfterInvaderPhase imp

	async Task IRunAfterInvaderPhase.AfterInvaderPhase( GameState gameState ) {
		if(_hadRavageOrBuild) { _hadRavageOrBuild = false; return; }

		Space space = ActionScope.Current.Spaces.Single( ss => 0 < ss[this] );
		// Do action
		await using ActionScope actionScope = await ActionScope.StartSpiritAction( ActionCategory.Spirit_Power, _spirit );
		await _triggeredAction( _spirit.Target( space.SpaceSpec ) );

		// Remove
		_remove = true; // ??? what happens if we put 2 of these down?
		space.Init(this,0);
	}
	bool IRunAfterInvaderPhase.RemoveAfterRun => _remove;
	bool _remove = false;
	#endregion IRunAfterInvaderPhase imp
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
	string IToken.Badge => "🦏";

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
	Task IModifyRemovingToken.ModifyRemovingAsync( RemovingTokenArgs args ) {
		// cannot be removed from the island.
		if(args.Token==this && args.Reason != RemoveReason.MovedFrom)
			args.Count = 0;
		return Task.CompletedTask;
	}
	#endregion

	#region IActionFactory - push token every slow

	void AddSlowPushToSpirit(Spirit s ) {
		if(_space != null) // if the beast has been placed on the board.
			s.AddActionFactory( this );
	}

	string IActionFactory.Title => "Push Marked Beast";
	public bool CouldActivateDuring( Phase speed, Spirit spirit ) => speed == Phase.Slow;

	public async Task ActivateAsync( Spirit self ) {
		await _space!.SourceSelector
			.AddGroup(1,Token.Beast)
			.FilterSpaceToken(st=>st.Token==this)
			.PushUpToN(self);
		// 1 Fear and 2 Damage in its land.
		await _space!.AddFear( 1 ); // don't cache space-state, it might have moved
		await _space.UserSelected_DamageInvadersAsync(self,2);
	}

	#endregion

	#region Track Token Space
	Task IHandleTokenAdded.HandleTokenAddedAsync( Space to, ITokenAddedArgs args ) {
		if(args.Added == this) _space = to;
		return Task.CompletedTask;
	}
	#endregion

	#region private fields
	Space? _space;
	#endregion

}