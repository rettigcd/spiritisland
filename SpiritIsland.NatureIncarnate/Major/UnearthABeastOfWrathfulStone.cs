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
		var noRavageOrBuildTrigger = new TriggerAfterNoRavageOrBuild( ctx.Self, ctx, beastToken );
		ctx.Space.Adjust(noRavageOrBuildTrigger,1);
		GameState.Current.AddPostInvaderPhase( noRavageOrBuildTrigger );

	}

}

/// <summary>
/// Sits on a space waiting for there to be no Ravage nor build,
/// Then adds the correct beast token.
/// </summary>
public class TriggerAfterNoRavageOrBuild( Spirit spirit, TargetSpaceCtx ctx, IToken beastToken )
	: ISpaceEntity
	, ISkipBuilds
	, IConfigRavages
	, IRunAfterInvaderPhase
{

	readonly Spirit _spirit = spirit;
	bool _hadRavageOrBuild;


	public string Text => "Tigger Action following no-build-nor-ravage.";

	#region detect build or ravage

	UsageCost ISkipBuilds.Cost => UsageCost.Extreme; // tries to go last

	Task<bool> ISkipBuilds.Skip( Space space ) {  _hadRavageOrBuild = true; return Task.FromResult(false);}

	Task IConfigRavages.Config( Space space ) { _hadRavageOrBuild = true; return Task.CompletedTask; }

	#endregion detect build or ravage

	#region IRunAfterInvaderPhase imp

	async Task IRunAfterInvaderPhase.AfterInvaderPhase( GameState gameState ) {
		if(_hadRavageOrBuild) { _hadRavageOrBuild = false; return; }

		Space space = ActionScope.Current.Spaces.Single( ss => 0 < ss[this] );
		// Do action
		await using ActionScope actionScope = await ActionScope.StartSpiritAction( ActionCategory.Spirit_Power, _spirit );
		await TriggeredAction( _spirit.Target( space.SpaceSpec ) );

		// Remove
		_remove = true; // ??? what happens if we put 2 of these down?
		space.Init(this,0);
	}
	bool IRunAfterInvaderPhase.RemoveAfterRun => _remove;
	bool _remove = false;
	#endregion IRunAfterInvaderPhase imp

	async Task TriggeredAction( TargetSpaceCtx ctxx ) {
		// 3 Fear.
		await ctx.AddFear(3);

		// 12 Damage.
		await ctx.DamageInvaders( 12 );

		// Add 1 Beast. (either normal or the marked one)
		await ctx.Space.AddAsync( beastToken, 1 );

		// You may Push that Beast.
		await ctx.SourceSelector
			.UseQuota(new Quota().AddGroup( 1, Token.Beast ))
			.FilterSpaceToken( st => st.Token == beastToken )
			.ConfigDestination(d=>d.Track( async to => {
				// 1 Fear and 2 Damage in its land.
				Space toSpace = (Space)to;
				await toSpace.AddFear(1);
				await ctxx.Self.Target(toSpace).DamageInvaders(2);
			} ))
			.PushN( ctx.Self );
	}

}

/// <summary>
/// Beast created from Unearth-a-Beast-of-wrathful-stone
/// </summary>
public class MarkedBeast : IToken
	, IModifyRemovingToken
	, IHandleTokenAdded
{
	string IToken.Badge => "🦏";

	public MarkedBeast(Spirit controlSpirit) {
		// Each Slow phase: You may Push Marked Beast. 1 Fear and 2 Damage at Marked Beast
		controlSpirit.Mods.Add( new MarkedBeastMover( controlSpirit, this ) );
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

	#region Track Token Space
	Task IHandleTokenAdded.HandleTokenAddedAsync( Space to, ITokenAddedArgs args ) {
		if(args.Added == this) SpaceSpec = to.SpaceSpec;
		return Task.CompletedTask;
	}
	#endregion

	public SpaceSpec? SpaceSpec { get; private set; }

}

/// <summary>
/// Each Slow phase: pushes the Marked Beast. 1 Fear and 2 Damage at its (possibly new) land.
/// </summary>
class MarkedBeastMover( Spirit controlSpirit, MarkedBeast beast ) : IActionFactory, IModifyAvailableActions {

	void IModifyAvailableActions.Modify( List<IActionFactory> orig, Phase phase ) {
		if( phase == Phase.Slow
			&& beast.SpaceSpec is not null // if the beast has been placed on the board.
			&& !controlSpirit.UsedActions.Any( x => x == this ) // once per round
		)
			orig.Add( this );
	}

	string IActionFactory.Title => "Push Marked Beast";
	string IOption.Text => "Push Marked Beast";
	public bool CouldActivateDuring( Phase speed, Spirit spirit ) => speed == Phase.Slow;

	public async Task ActivateAsync( Spirit self ) {
		var scope = ActionScope.Current;
		await scope.AccessTokens( beast.SpaceSpec! ).SourceSelector
			.UseQuota(new Quota().AddGroup(1,Token.Beast))
			.FilterSpaceToken(st=>st.Token==beast)
			.PushUpToN(self);
		// 1 Fear and 2 Damage in its land.
		Space space = scope.AccessTokens( beast.SpaceSpec! ); // don't cache space-state, it might have moved
		await space.AddFear( 1 );
		await self.Target(space).DamageInvaders(2);
	}


}
