namespace SpiritIsland.FeatherAndFlame;

public class GiftOfFlowingPower {

	[SpiritCard("Gift of Flowing Power",1,Element.Fire,Element.Water),Fast,AnotherSpirit]
	[Instructions( "Target Spirit gains 1 Energy. Target Spirit chooses to either: Play another Power Card by paying its cost. -or- Gain 1 Fire and 1 Water." ), Artist( Artists.JorgeRamos )]
	public static Task ActAsync( TargetSpiritCtx ctx ) {
		return DoTargetSpiritAction( ctx.Other );
	}

	static Task DoTargetSpiritAction( Spirit other ) {
		// Target spirit gains 1 energy.
		other.Energy += 1;

		return Cmd.Pick1(
			// Play another Power Card by paying its cost
			new SpiritAction( "Play another Power Card by paying its cost", spirit => spirit.AddActionFactory( new PlayCardForCost() ) ),
			// OR Gains 1 fire and 1 water.
			new SpiritAction( "Gain 1 fire and 1 water", spirit => spirit.Elements.Add( Element.Fire, Element.Water ) ),
			// Or nothing (optional)
			SpiritAction.NoAction
		).ActAsync( other );
	}
}