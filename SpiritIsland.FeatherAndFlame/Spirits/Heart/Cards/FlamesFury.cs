namespace SpiritIsland.FeatherAndFlame;

public class FlamesFury{

	[SpiritCard("Flame's Fury",0,Element.Sun,Element.Fire,Element.Plant),Fast,AnySpirit]
	[Instructions( "Target Spirit gains 1 Energy. Target Spirit does +1 Damage with each Damage dealing Power they use this turn. (Powers which damage multiple lands or each Invader only get 1 extra damage total. Repeated Powers keep the +1 boost. Destroy effects don't get any bonus.)" ), Artist( Artists.NolanNasser )]
	static public Task ActAsync( TargetSpiritCtx ctx ) {

		// Target Spirit gains 1 energy.
		++ctx.Other.Energy;

		// Target Spirit does +1 damage for each damage-dealing power
		++ctx.Other.BonusDamage;

		GameState.Current.AddTimePassesAction( TimePassesAction.Once( gs => --ctx.Other.BonusDamage) );

		return Task.CompletedTask;
	}

}