namespace SpiritIsland.BranchAndClaw;

public class SacrosanctWilderness {

	// 2 fast, sun, earth, plant, 
	// range 1, no blight

	[SpiritCard("Sacrosanct Wilderness",2,Element.Sun,Element.Earth,Element.Plant),Fast,FromPresence(1,Target.NoBlight)]
	[Instructions( "Push 2 Dahan. 2 Damage per Wilds in target land. -or- Add 1 Wilds." ), Artist( Artists.JoshuaWright )]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {

		// push 2 dahan
		await ctx.PushDahan( 2 );

		await ctx.SelectActionOption(
			new SpaceCmd("2 Damage per wilds", ctx => ctx.DamageInvaders( 2 * ctx.Wilds ) ).OnlyExecuteIf( x=>ctx.Tokens.Wilds.Any ),
			new SpaceCmd("Add 1 wilds", ctx => ctx.Wilds.Add(1))
		);

	}

}