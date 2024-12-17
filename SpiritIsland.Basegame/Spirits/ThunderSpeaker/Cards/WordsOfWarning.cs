using SpiritIsland.Invaders.Ravage;

namespace SpiritIsland.Basegame;

public class WordsOfWarning {

	public const string Name = "Words of Warning";

	[SpiritCard( WordsOfWarning.Name, 1, Element.Air, Element.Sun, Element.Animal ),Fast,FromPresence(1,Filter.Dahan)]
	[Instructions("Defend 3. During Ravage, Dahan in target land deal damage simultaneously with Invaders."), Artist( Artists.LoicBelliau )]
	static public Task ActAsync( TargetSpaceCtx ctx ) {

		// defend 3
		ctx.Defend(3);

		// During Ravage, dahan in target land deal damage simultaneiously with invaders
		ctx.Space.Init(new SimultaneousDefend(),1);

		return Task.CompletedTask;
	}

}

public class SimultaneousDefend : BaseModEntity, IConfigRavages, IEndWhenTimePasses {

	Task IConfigRavages.Config( Space space ) {

		// Adjust dahan to go at same time as invaders
		foreach(HumanToken orig in space.HumanOfAnyTag( Human.Dahan ).ToArray())
			AdjustRavageOrder( space, orig, RavageOrder.InvaderTurn );

		// restore
		ActionScope.Current.AtEndOfThisAction( scope => {
			foreach(HumanToken orig in space.HumanOfAnyTag( Human.Dahan ).ToArray())
				AdjustRavageOrder( space, orig, RavageOrder.DahanTurn );
		} );
		return Task.CompletedTask;
	}

	static void AdjustRavageOrder( Space space, HumanToken orig, RavageOrder order ) {
		space.Init( orig.SetRavageOrder( order ), space[orig] );
		space.Init( orig, 0 );
	}

}