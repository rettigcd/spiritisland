namespace SpiritIsland.Basegame;

public class WordsOfWarning {

	public const string Name = "Words of Warning";

	[SpiritCard( WordsOfWarning.Name, 1, Element.Air, Element.Sun, Element.Animal )]
	[Fast]
	[FromPresence(1,Target.Dahan)]
	static public Task Act( TargetSpaceCtx ctx ) {

		// defend 3
		ctx.Defend(3);

		// During Ravage, dahan in target land deal damage simultaneiously with invaders
		ctx.GameState.ModifyRavage( ctx.Space, cfg => cfg.RavageSequence = SimultaneousDamage );

		return Task.CompletedTask;
	}

	static async Task SimultaneousDamage( RavageAction eng ) {
		int damageFromInvaders = eng.GetDamageInflictedByAttackers();
		int damageFromDahan = eng.GetDamageInflictedByDefenders();

		await eng.DamageLand( damageFromInvaders );
		await eng.DamageDefenders( damageFromInvaders );
		await eng.DamageAttackers( damageFromDahan );
	}

}