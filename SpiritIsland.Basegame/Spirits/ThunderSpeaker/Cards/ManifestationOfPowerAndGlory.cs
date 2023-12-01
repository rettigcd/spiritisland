using System.Xml;

namespace SpiritIsland.Basegame;

public class ManifestationOfPowerAndGlory {

	[SpiritCard( "Manifestation of Power and Glory", 3, Element.Sun, Element.Fire, Element.Air ),Slow,FromPresence(0,Filter.Dahan)]
	[Instructions( "1 Fear. Each Dahan deals damage equal to the number of your Presence in target land." ), Artist( Artists.LoicBelliau )]
	static public Task Act( TargetSpaceCtx ctx ) {

		// 1 fear
		ctx.AddFear(1);

		// each dahan deals damange equal to the number of your presense in the target land
		return ctx.DamageInvaders( ctx.Dahan.CountAll * ctx.PresenceCount );

	}

}