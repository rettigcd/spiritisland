namespace SpiritIsland.NatureIncarnate;

public class BoonOfCorruptedBlood {
	const string Name = "Boon of Corrupted Blood";

	[SpiritCard( Name, 1, Element.Sun, Element.Fire, Element.Animal ),Fast, AnySpirit]
	[Instructions( "1 Damage in one of target Spirit's lands. If you target another Spirit, in that land also: Destroy 1 of their Presence. 1 Damage. Gather 1 Beast." ), Artist( Artists.NolanNasser )]
	static public Task ActAsync( TargetSpiritCtx ctx ) => SpiritAction(ctx.Other,ctx.Other==ctx.Self);


	static async Task SpiritAction( Spirit spirit, bool isSelf ) {

		// Get a spirit's land...

		// in one of target Spirit's lands:
		int damage = isSelf ? 1 : 2;
		Space space = await spirit.SelectSpaceAsync(
			$"{damage} Damage" + (isSelf ? "" : ", Destroy Presence, Gather 1 Beast"), 
			spirit.Presence.Lands, 
			Present.Done
		);
		if(space is null) return;

		// 1 Damage 
		// If you target another Spirit, in that land also:
		//		1 Damage.
		//		Destroy 1 of their Presence.
		//		Gather 1 Beast.
		var targetSpiritsLandCtx = spirit.Target(space);
		await targetSpiritsLandCtx.DamageInvaders( damage );
		if(!isSelf) {
			await space.Destroy(targetSpiritsLandCtx.Self.Presence.Token,1);
			await targetSpiritsLandCtx.Gather(1,Token.Beast);
		}

	}

}

