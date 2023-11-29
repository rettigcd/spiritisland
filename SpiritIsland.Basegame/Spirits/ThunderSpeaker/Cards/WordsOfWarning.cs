namespace SpiritIsland.Basegame;

public class WordsOfWarning {

	public const string Name = "Words of Warning";

	[SpiritCard( WordsOfWarning.Name, 1, Element.Air, Element.Sun, Element.Animal ),Fast,FromPresence(1,Target.Dahan)]
	[Instructions("Defend 3. During Ravage, Dahan in target land deal damage simultaneously with Invaders."), Artist( Artists.LoicBelliau )]
	static public Task Act( TargetSpaceCtx ctx ) {

		// defend 3
		ctx.Defend(3);

		// During Ravage, dahan in target land deal damage simultaneiously with invaders
		ctx.Tokens.Init(new SimultaneousDefend(),1);

		return Task.CompletedTask;
	}

}

public class SimultaneousDefend : BaseModEntity, IConfigRavages, IEndWhenTimePasses {

	void IConfigRavages.Config( SpaceState space ) {

		// Token Reduces Attack of invaders by 1
		foreach(HumanToken orig in space.HumanOfAnyTag( Human.Dahan ).ToArray())
			AdjustRavageOrder( space, orig, RavageOrder.InvaderTurn );

		// At end of Action, invaders are are restored to original attack time.
		ActionScope.Current.AtEndOfThisAction( scope => {
			foreach(HumanToken orig in space.HumanOfAnyTag( Human.Dahan ).ToArray())
				AdjustRavageOrder( space, orig, RavageOrder.DahanTurn );
		} );

	}

	static void AdjustRavageOrder( SpaceState space, HumanToken orig, RavageOrder order ) {
		space.Init( orig.SetRavageOrder( order ), space[orig] );
		space.Init( orig, 0 );
	}

}