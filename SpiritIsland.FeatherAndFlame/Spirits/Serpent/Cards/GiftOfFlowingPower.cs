namespace SpiritIsland.FeatherAndFlame;

public class GiftOfFlowingPower {

	[SpiritCard("Gift of Flowing Power",1,Element.Fire,Element.Water),Fast,AnotherSpirit]
	[Instructions( "Target Spirit gains 1 Energy. Target Spirit chooses to either: Play another Power Card by paying its cost. -or- Gain 1 Fire and 1 Water." ), Artist( Artists.JorgeRamos )]
	public static Task ActAsync( TargetSpiritCtx ctx ) {
		// Target spirit gains 1 energy.
		ctx.Other.Energy += 1;

		// Target spirit chooses to either:
		return ctx.OtherCtx.SelectActionOption(
			// Play another Power Card by paying its cost
			new SpiritAction("Play another Power Card by paying its cost", _ => { ctx.Other.AddActionFactory( new PlayCardForCost() ); } ),
			// OR Gains 1 fire and 1 water.
			new SpiritAction("Gain 1 fire and 1 water", _ => ctx.Other.Elements.Add(Element.Fire,Element.Water) )
		);
	}

}