namespace SpiritIsland.Basegame;

public class GnawingRootbiters {

	public const string Name = "Gnawing Rootbiters";

	[MinorCard(GnawingRootbiters.Name,0,"earth, animal"), Slow, FromPresence(1)]
	[Instructions( "Push up to 2 Town." ), Artist( Artists.MoroRogers )]
	static public Task ActAsync(TargetSpaceCtx ctx ) {

		// push up to 2 towns
		return ctx.PushUpTo(2,Human.Town);
	}

}