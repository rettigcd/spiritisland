namespace SpiritIsland.Basegame;

public class VigorOfTheBreakingDawn {

	[MajorCard("Vigor of the Breaking Dawn",3,Element.Sun,Element.Animal),Fast,FromPresence(2,Filter.Dahan)]
	[Instructions( "2 Damage per Dahan in target land. -If you have- 3 Sun, 2 Animal: You may Push up to 2 Dahan. In lands you Pushed Dahan to, 2 Damage per Dahan." ), Artist( Artists.LoicBelliau )]
	public static async Task ActAsync(TargetSpaceCtx ctx){

		// 2 damage per dahan in target land
		await ctx.DamageInvaders(2*ctx.Dahan.CountAll);

		if( await ctx.YouHave("3 sun,2 animal") ){

			HashSet<Space> pushedToLands = [];

			// you may push up to 2 dahan.
			await ctx.SourceSelector
				.UseQuota(new Quota().AddGroup(2,Human.Dahan))
				.ConfigDestination( Distribute.OnEachDestinationLand( async to => {
					pushedToLands.Add(to);
					await DahanDeal2DamageEach( ctx.Target( to ) );
				}) )
				.PushUpToN(ctx.Self );

			// Wait until the end so that we don't do this damage until ALL the dahan have been moved to this destination.
			foreach( var pushedToLand in pushedToLands )
				await DahanDeal2DamageEach( ctx.Target(pushedToLand) );
		}
	}

	static Task DahanDeal2DamageEach( TargetSpaceCtx ctx ) => ctx.DamageInvaders(ctx.Dahan.CountAll * 2);

}