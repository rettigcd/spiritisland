namespace SpiritIsland.NatureIncarnate;

[InnatePower( "Land Creaks with Tension","Each tier targets any 1 of your lands." ), Fast, Yourself]
public class LandCreaksWithTension {

	[InnateTier( "1 earth", "If you have at least 1 Impending, Add 1 Quake.", 0 )]
	static public Task Option1( SelfCtx ctx ) => AddQuakeIf( ctx, 1 );

	[InnateTier( "1 moon,1 earth", "Defend 1 per Impending (max 3).", 1 )]
	static public Task Option2( SelfCtx ctx ) => Defend1PerImpending( ctx ); 

	[InnateTier( "1 moon,2 earth", "If you have at least 3 Impending, Add 1 Quake.", 2 )]
	static public Task Option3( SelfCtx ctx ) => AddQuakeIf( ctx, 3 );

	[InnateTier( "2 moon,3 earth", "Defend 1 per Impending (max 3).", 3 )]
	static public Task Option4( SelfCtx ctx ) => Defend1PerImpending( ctx ); 


	static Task AddQuakeIf( SelfCtx ctx, int minImpending )
		=> AddQuake
			.OnlyExecuteIf( x=> minImpending <= ImpendingCount(ctx ) )
			.In().SpiritPickedLand().Which( Has.YourPresence)
			.ActAsync( ctx);

	static SpaceCmd AddQuake => new SpaceCmd( "Add 1 Quake", ctx => ctx.Tokens.Add( Token.Quake, 1 ) );

	static Task Defend1PerImpending( SelfCtx ctx )
		=> Cmd.Defend( Math.Min( 3, ImpendingCount( ctx ) ) )
			.In().SpiritPickedLand().Which( Has.YourPresence )
			.ActAsync( ctx );

	static int ImpendingCount(SelfCtx ctx) => ((DancesUpEarthquakes)ctx.Self).Impending.Count;
}
