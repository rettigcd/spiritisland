namespace SpiritIsland.Basegame;

public class TidalBoon {

	public const string Name = "Tidal Boon";

	[SpiritCard(Name,1,Element.Moon,Element.Water,Element.Earth),Slow,AnotherSpirit]
	[Instructions( "Target Spirit gains 2 Energy and may Push 1 Town and up to 2 Dahan from one of their lands. If Dahan are pushed to your ocean, you may move them to any Coastal land instead of Drowning them." ), Artist( Artists.JoshuaWright )]
	static public async Task Act( TargetSpiritCtx ctx ) {

		// If dahan are pushed to your ocean, you may move them to any coastal land instead of drowning them.
		Ocean.EnableSavingDahan();

		// target spirit gains 2 energy 
		ctx.Other.Energy += 2;

		// and may push 1 town and up to 2 dahan from one of their lands.
		var pushLand = await ctx.OtherCtx.TargetLandWithPresence( "Select land to push town and 2 dahan" );

		await pushLand
			.Pusher
			.AddGroup(1,Human.Town)
			.AddGroup(2,Human.Dahan)
			.DoUpToN();
	}

}