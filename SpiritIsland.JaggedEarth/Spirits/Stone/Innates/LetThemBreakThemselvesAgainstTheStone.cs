namespace SpiritIsland.JaggedEarth;

[InnatePower("Let Them Break Themselves Against the Stone"), Fast, FromPresence(0)]
[RepeatIf( "7 earth, 2 sun")]
class LetThemBreakThemselvesAgainstTheStone {

	[InnateOption("3 earth","After Invaders deal 1 or more Damage to target land, 2 Damage")]
	static public Task Option0(TargetSpaceCtx ctx ) {
		ctx.Tokens.Adjust( new BreakThemselvesMod(ctx.Self,false), 1 );
		return Task.CompletedTask;
	}

	[InnateOption("5 earth","Also deal half of the Damage Invaders did to the land (rounding down)")]
	static public Task Option1(TargetSpaceCtx ctx ) {
		ctx.Tokens.Adjust( new BreakThemselvesMod( ctx.Self, true ), 1 );
		return Task.CompletedTask;
	}

}

class BreakThemselvesMod : BaseModEntity, IHandleTokenAddedAsync, IEndWhenTimePasses {

	public BreakThemselvesMod( Spirit spirit, bool shouldAddHalfInvaderDamage ) {
		_spirit = spirit;
		_shouldAddHalfInvaderDamage = shouldAddHalfInvaderDamage;
	}

	public async Task HandleTokenAddedAsync( ITokenAddedArgs args ) {
		if(args.Added != LandDamage.Token) return;

		int damage = 2;
		if(_shouldAddHalfInvaderDamage) damage += args.To[args.Added] / 2;

		await args.To.Invaders.UserSelectedDamage( _spirit, damage );
	}

	readonly Spirit _spirit;
	readonly bool _shouldAddHalfInvaderDamage;
}