namespace SpiritIsland.PromoPack1;

// fast, range 1, any
// Target each level of this as a separate Power
// 1 air, 2 water Isolate target land.
// 2 air, 2 earth Isolate target land.
// 3 air, 2 plant Isolate target land.

[InnatePower( CloseTheWays.Name )]
[Fast,FromPresence( 1 )]
public class CloseTheWays {

	public const string Name = "Close the Ways";

	[InnateOption( "1 air,2 water", "Isolate target land.", 0 )]
	static public Task Option1( TargetSpaceCtx ctx ) => DoIsolate( ctx );

	[InnateOption( "2 air,2 earth", "Isolate target land.", 1 )]
	static public Task Option2( TargetSpaceCtx ctx ) => DoIsolate( ctx );

	[InnateOption( "3 air,2 plant", "Isolate target land.", 2 )]
	static public Task Option3( TargetSpaceCtx ctx ) => DoIsolate( ctx );

	static async Task DoIsolate(TargetSpaceCtx ctx) {
		bool previouslyRun = ctx.ActionScope.ContainsKey( Name );
		if(previouslyRun) {
			// Target a New Space
			var space = await ctx.Self.TargetsSpace(ctx, "Target Additional Space To Close", 
				new TargetingSourceCriteria(From.Presence),
				ctx.TerrainMapper.Specify(1)
			);
			ctx = ctx.Target(space);
		}
		ctx.ActionScope[Name] = true; // mark as ran

		ctx.Isolate();
	}

}
