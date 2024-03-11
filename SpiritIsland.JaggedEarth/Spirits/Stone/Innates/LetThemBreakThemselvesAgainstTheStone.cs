namespace SpiritIsland.JaggedEarth;

[InnatePower("Let Them Break Themselves Against the Stone"), Fast, FromPresence(0)]
[RepeatIf( "7 earth, 2 sun")]
class LetThemBreakThemselvesAgainstTheStone {

	[InnateTier("3 earth","After Invaders deal 1 or more Damage to target land, 2 Damage")]
	static public Task Option0(TargetSpaceCtx ctx ) {
		ctx.Space.Adjust( new BreakThemselvesMod(ctx.Self,false), 1 );
		return Task.CompletedTask;
	}

	[InnateTier("5 earth","Also deal half of the Damage Invaders did to the land (rounding down)")]
	static public Task Option1(TargetSpaceCtx ctx ) {
		ctx.Space.Adjust( new BreakThemselvesMod( ctx.Self, true ), 1 );
		return Task.CompletedTask;
	}

}

class BreakThemselvesMod( Spirit _spirit, bool _shouldAddHalfInvaderDamage ) 
	: BaseModEntity
	, IEndWhenTimePasses
	, IReactToLandDamage
{
	async Task IReactToLandDamage.HandleDamageAddedAsync( Space space,int added ) {
		int damage = 2;
		if(_shouldAddHalfInvaderDamage) damage += space[LandDamage.Token] / 2;

		await space.UserSelected_DamageInvadersAsync( _spirit, damage );
	}
}