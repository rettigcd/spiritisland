namespace SpiritIsland.Basegame;

public class DahanRaid : IFearOptions {

	public const string Name = "Dahan Raid";
	string IFearOptions.Name => Name;

	[FearLevel(1, "Each player chooses a different land with Dahan. 1 Damage there.")]
	public Task Level1( FearCtx ctx ) {
		return ForEachPlayerChosenLandWithDahan( ctx, sCtx=>sCtx.DamageInvaders( 1 ) );
	}

	[FearLevel( 2, "Each player chooses a different land with Dahan. 1 Damage per Dahan there." )]
	public Task Level2( FearCtx ctx ) {
		return ForEachPlayerChosenLandWithDahan( ctx, sCtx => sCtx.DamageInvaders( sCtx.Dahan.Count ) );
	}

	[FearLevel( 3, "Each player chooses a different land with Dahan. 2 Damage per Dahan there." )]
	public Task Level3( FearCtx ctx ) {
		return ForEachPlayerChosenLandWithDahan( ctx, sCtx => sCtx.DamageInvaders( sCtx.Dahan.Count * 2 ) );
	}

	static async Task ForEachPlayerChosenLandWithDahan( FearCtx ctx, Func<TargetSpaceCtx,Task> action ) {

		const string prompt = "Fear:select land with dahan";

		HashSet<Space> used = new ();
		foreach(var spiritCtx in ctx.Spirits) {
			// Select un-used space
			var options = spiritCtx.AllSpaces.Where( s=>spiritCtx.Target(s).Dahan.Any ).Except( used ).ToArray();
			var target = await spiritCtx.Decision( new Select.Space( prompt, options, Present.Always ));
			used.Add( target );
			TargetSpaceCtx spactCtx = spiritCtx.Target(target);

			await action(spactCtx);
		}
	}

}