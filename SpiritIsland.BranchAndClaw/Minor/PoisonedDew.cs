namespace SpiritIsland.BranchAndClaw;

public class PoisonedDew {

	[MinorCard( "Poisoned Dew", 1, Element.Fire, Element.Water, Element.Plant ),Slow,FromPresence( 1 )]
	[Instructions( "Destroy 1 Explorer. If target land is Jungle / Wetland, destroy all Explorer." ), Artist( Artists.CariCorene )]
	static public Task ActAsync( TargetSpaceCtx ctx ) {

		int countToDestroy = ctx.IsOneOf(Terrain.Jungle,Terrain.Wetland)
			? int.MaxValue
			: 1;

		return ctx.Invaders.DestroyNOfClass(countToDestroy,Human.Explorer);
	}

}