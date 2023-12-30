namespace SpiritIsland.JaggedEarth;

[InnatePower("Let Them Break Themselves Against the Stone"), Fast, FromPresence(0)]
[RepeatIf( "7 earth, 2 sun")]
class LetThemBreakThemselvesAgainstTheStone {

	[InnateTier("3 earth","After Invaders deal 1 or more Damage to target land, 2 Damage")]
	static public Task Option0(TargetSpaceCtx ctx ) {
		ctx.Tokens.Adjust( new BreakThemselvesMod(ctx.Self,false), 1 );
		return Task.CompletedTask;
	}

	[InnateTier("5 earth","Also deal half of the Damage Invaders did to the land (rounding down)")]
	static public Task Option1(TargetSpaceCtx ctx ) {
		ctx.Tokens.Adjust( new BreakThemselvesMod( ctx.Self, true ), 1 );
		return Task.CompletedTask;
	}

}

class BreakThemselvesMod : BaseModEntity, IEndWhenTimePasses, IReactToLandDamage {

	public BreakThemselvesMod( Spirit spirit, bool shouldAddHalfInvaderDamage ) {
		_spirit = spirit;
		_shouldAddHalfInvaderDamage = shouldAddHalfInvaderDamage;
	}

	async Task IReactToLandDamage.HandleDamageAddedAsync( SpaceState tokens,int added ) {
		int damage = 2;
		if(_shouldAddHalfInvaderDamage) damage += tokens[LandDamage.Token] / 2;

		await tokens.UserSelected_DamageInvaders( _spirit, damage );
	}

	readonly Spirit _spirit;
	readonly bool _shouldAddHalfInvaderDamage;
}