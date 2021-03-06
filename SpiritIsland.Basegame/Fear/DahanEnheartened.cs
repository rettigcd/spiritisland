namespace SpiritIsland.Basegame;

public class DahanEnheartened : IFearOptions {

	public const string Name = "Dahan Enheartened";
	string IFearOptions.Name => Name;


	[FearLevel( 1, "Each player may Push 1 Dahan from a land with Invaders or Gather 1 Dahan into a land with Invaders." )]
	public async Task Level1( FearCtx ctx ) {
		var gs = ctx.GameState;
		foreach( var spiritCtx in ctx.Spirits ) {
			var spacesWithInvaders = gs.Island.AllSpaces.Where( s=>spiritCtx.Target(s).HasInvaders ).ToArray();
			var target = await spiritCtx.Decision( new Select.Space( "Select Space to Gather or push 1 dahan", spacesWithInvaders, Present.Always));

			var spaceCtx = spiritCtx.Target(target);
			await spaceCtx.SelectActionOption(
				new SpaceAction( "Push",    ctx => ctx.PushUpToNDahan( 1 ) ),
				new SpaceAction( "Gather", ctx => ctx.GatherUpToNDahan( 1 ) )
			);
		}
	}

	[FearLevel( 2, "Each player chooses a different land. In chosen lands: Gather up to 2 Dahan, then 1 Damage if Dahan are present." )]
	public async Task Level2( FearCtx ctx ) {
		HashSet<Space> used = new ();
		foreach(var spiritCtx in ctx.Spirits) {
			var options = spiritCtx.AllSpaces.Where( s=>spiritCtx.Target(s).Dahan.Any ).Except( used ).ToArray();
			var target = await spiritCtx.Decision( new Select.Space( "Fear:select land with dahan for 1 damage", options, Present.Always ));
			used.Add( target );
			var sCtx = spiritCtx.Target(target);

			await sCtx.GatherUpToNDahan( 2 );
			if( sCtx.Dahan.Any )
				await sCtx.DamageInvaders( 1 );
		}
	}

	[FearLevel( 3, "Each player chooses a different land. In chosen lands: Gather up to 2 Dahan, then 1 Damage per Dahan present." )]
	public async Task Level3( FearCtx ctx ) {
		HashSet<Space> used = new ();
		foreach(var spiritCtx in ctx.Spirits) {
			var options = spiritCtx.AllSpaces.Where( s => spiritCtx.Target(s).Dahan.Any ).Except( used ).ToArray();
			var target = await spiritCtx.Decision( new Select.Space( "Fear:select land with dahan for 1 damage", options, Present.Always ));
			used.Add( target );
			var sCtx = spiritCtx.Target(target);

			await sCtx.GatherUpToNDahan( 2 );
			await sCtx.DamageInvaders( sCtx.Dahan.Count );
		}
	}

}