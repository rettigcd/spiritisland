namespace SpiritIsland.JaggedEarth;

public class FieryVengeance {

	[SpiritCard("Fiery Vengeance",0,Element.Sun,Element.Fire), Fast, AnySpirit]
	[Instructions( "Cost to Use: Target Spirit Removes 1 of their DestroyedPresence from the game. 1 Fear and 1 Damage in one of target Spirit's lands. (This is your Power, so Blight counts as Badlands, even if target is another Spirit.)" ), Artist( Artists.DamonWestenhofer )]
	static public async Task ActAsync(TargetSpiritCtx ctx ) {

		// Cost to User: Target Spirit Removes 1 of their Destroyed presence from the game.
		ITokenRemovedArgs destroyed = await ctx.Other.Presence.Destroyed.RemoveAsync();
		if(destroyed.Count == 0) return;

		// 1 fear and 1 damage in one of target Spirit's lands.
		var space = await ctx.Other.SelectAlways("1 fear + 1 damage", ctx.Other.Presence.Lands);
		var spaceCtx = ctx.Other.Target(space);
		await spaceCtx.AddFear(1);
		await spaceCtx.DamageInvaders(1);
		//  (This is your Power, so blight counts as badland, even if target is another Spirit.)
			
	}

}