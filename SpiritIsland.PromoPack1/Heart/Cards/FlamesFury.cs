namespace SpiritIsland.PromoPack1;

public class FlamesFury{

	[SpiritCard("Flame's Fury",0,Element.Sun,Element.Fire,Element.Plant),Fast,AnySpirit]
	static public Task ActAsync( TargetSpiritCtx ctx ) {

		// Target Spirit gains 1 energy.
		++ctx.Other.Energy;

		// Target Spirit does +1 damage for each damage-dealing power
		++ctx.Other.BonusDamage;

		ctx.GameState.TimePasses_ThisRound.Push( ( gs ) => {
			--ctx.Other.BonusDamage;
			return Task.CompletedTask;
		} );

		return Task.CompletedTask;
	}

}