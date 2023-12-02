namespace SpiritIsland.NatureIncarnate;

[InnatePower( "Land Creaks with Tension","Each tier targets any 1 of your lands." ), Fast, Yourself]
public class LandCreaksWithTension {

	[InnateTier( "1 earth", "If you have at least 1 Impending, Add 1 Quake.", 0 )]
	static public Task Option1( Spirit self ) => AddQuakeIf( self, 1 );

	[InnateTier( "1 moon,1 earth", "Defend 1 per Impending (max 3).", 1 )]
	static public Task Option2( Spirit self ) => Defend1PerImpending( self ); 

	[InnateTier( "1 moon,2 earth", "If you have at least 3 Impending, Add 1 Quake.", 2 )]
	static public Task Option3( Spirit self ) => AddQuakeIf( self, 3 );

	[InnateTier( "2 moon,3 earth", "Defend 1 per Impending (max 3).", 3 )]
	static public Task Option4( Spirit self ) => Defend1PerImpending( self ); 


	static Task AddQuakeIf( Spirit self, int minImpending )
		=> AddQuake
			.OnlyExecuteIf( x=> minImpending <= ImpendingCount(self) )
			.In().SpiritPickedLand().Which( Has.YourPresence)
			.ActAsync(self);

	static SpaceAction AddQuake => new SpaceAction( "Add 1 Quake", ctx => ctx.Tokens.AddAsync( Token.Quake, 1 ) );

	static Task Defend1PerImpending( Spirit self )
		=> Cmd.Defend( Math.Min( 3, ImpendingCount( self ) ) )
			.In().SpiritPickedLand().Which( Has.YourPresence )
			.ActAsync( self );

	static int ImpendingCount(Spirit self) => ((DancesUpEarthquakes)self).Impending.Count;
}
