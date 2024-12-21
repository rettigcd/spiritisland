namespace SpiritIsland.Basegame;

public class ManifestationOfPowerAndGlory {

	public const string Name = "Manifestation of Power and Glory";

	[SpiritCard( Name, 3, Element.Sun, Element.Fire, Element.Air ),Slow,FromPresence(0,Filter.Dahan)]
	[Instructions( "1 Fear. Each Dahan deals damage equal to the number of your Presence in target land." ), Artist( Artists.LoicBelliau )]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {

		// 1 fear
		await ctx.AddFear(1);

		// each dahan deals damange equal to the number of your presense in the target land
		await ctx.DamageInvaders( ctx.Dahan.CountAll * ctx.PresenceCount );

	}

}