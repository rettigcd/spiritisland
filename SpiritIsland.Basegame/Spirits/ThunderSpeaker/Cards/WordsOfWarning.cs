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

	static async Task SimultaneousDamage( RavageBehavior behavior, RavageData data ) {
		int damageFromInvaders = await RavageBehavior.GetDamageInflictedByAttackers(behavior,data);
		int damageFromDahan = RavageBehavior.GetDamageInflictedByDefenders( behavior,data );

		await RavageBehavior.DamageLand( data, damageFromInvaders );
		await behavior.DamageDefenders( behavior, data, damageFromInvaders );
		await RavageBehavior.DamageAttackers( data, damageFromDahan );
	}

}