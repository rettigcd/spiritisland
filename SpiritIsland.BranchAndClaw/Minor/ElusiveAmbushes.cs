namespace SpiritIsland.BranchAndClaw;

public class ElusiveAmbushes {

	[MinorCard("Elusive Ambushes",1,Element.Sun,Element.Fire,Element.Water),Fast,FromPresence(1,Filter.Dahan)]
	[Instructions( "1 Damage. -or- Defend 4." ), Artist( Artists.NolanNasser )]
	static public Task ActAsync(TargetSpaceCtx ctx ) {

		return ctx.SelectActionOption(
			new SpaceAction("1 damage", ctx => ctx.DamageInvaders(1)),
			new SpaceAction("Defend 4", ctx => ctx.Defend(4))
		);

	}

}