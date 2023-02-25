namespace SpiritIsland.Basegame;

public class FlashFloods {

	public const string Name = "Flash Floods";
	[SpiritCard(FlashFloods.Name,2,Element.Sun,Element.Water),Fast]
	[FromPresence(1,Target.Invaders)]
	[Preselect("1 Damage (2 for coastal)","Explorer,Town,City")]
	static public async Task ActionAsync(TargetSpaceCtx ctx) {
		// +1 damage, if coastal +1 additional damage
		int damage = ctx.IsCoastal ? 2 : 1;
		await ctx.DamageInvaders( damage );
	}

}