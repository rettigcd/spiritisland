namespace SpiritIsland.NatureIncarnate;

public class BoonOfCorruptedBlood {
	const string Name = "Boon of Corrupted Blood";

	[SpiritCard( Name, 1, Element.Sun, Element.Fire, Element.Animal ),Fast, AnySpirit]
	[Instructions( "1 Damage in one of target Spirit's lands. If you target another Spirit, in that land also: Destroy 1 of their Presence. 1 Damage. Gather 1 Beast." ), Artist( Artists.NolanNasser )]
	static public Task ActAsync( TargetSpiritCtx ctx ) => Spirit(ctx.OtherCtx,ctx.Other==ctx.Self);


	static async Task Spirit( SelfCtx ctx,bool isSelf ) {

		// Get a spirit's land...

		// in one of target Spirit's lands:
		int damage = isSelf ? 1 : 2;
		TargetSpaceCtx spaceCtx = await ctx.SelectSpace(
			$"Name: {damage} Damage" + (isSelf ? "" : ", Destroy Presence, Gather 1 Beast"), 
			ctx.Self.Presence.Spaces, 
			Present.Done
		);
		if(spaceCtx==null) return;

		// 1 Damage 
		// If you target another Spirit, in that land also:
		//		1 Damage.
		//		Destroy 1 of their Presence.
		//		Gather 1 Beast.
		await spaceCtx.DamageInvaders( damage );
		if(!isSelf) {
			await spaceCtx.Self.Presence.DestroyPresenceOn( spaceCtx.Tokens );
			await spaceCtx.Gather(1,Token.Beast);
		}

	}

}

