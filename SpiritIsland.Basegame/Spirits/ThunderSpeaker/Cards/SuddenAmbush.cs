using System.Xml;

namespace SpiritIsland.Basegame;

public class SuddenAmbush {

	public const string Name = "Sudden Ambush";

	[SpiritCard( SuddenAmbush.Name, 2, Element.Fire, Element.Air, Element.Animal ),Fast,FromPresence(1)]
	[Instructions("You may Gather 1 Dahan. Each Dahan destroys 1 Explorer." ), Artist( Artists.LoicBelliau )]
	static public async Task Act( TargetSpaceCtx ctx ) {

		// you may gather 1 dahan
		await ctx.GatherUpToNDahan( 1 );

		// Each dahan destroys 1 explorer
		await ctx.Invaders.DestroyNOfClass( ctx.Dahan.CountAll, Human.Explorer );
	}

}