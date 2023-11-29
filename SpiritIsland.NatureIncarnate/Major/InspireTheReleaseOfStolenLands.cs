namespace SpiritIsland.NatureIncarnate;

public class InspireTheReleaseOfStolenLands {

	public const string Name = "Inspire the Release of Stolen Lands";
	const string Threshold = "3 sun,3 water,2 animal";

	[MajorCard(Name,4,"sun,water,plant,animal"),Slow]
	[FromPresenceThresholdAlternate(2,Target.NoBlight,Threshold,2,Target.Any)]
	[Instructions( "Gather up to 3 Dahan. Remove up to 3 Health worth of Invaders per Dahan. -If you have- 3 Sun,3 Water,2 Animal: This Power can target lands with Blight. If Dahan are present, Remove 1 Blight from target land, then Remove 1 Explorer, 1 Town, and 1 City from a land within Range 1." ), Artist( Artists.AgnieszkaDabrowiecka )]
	static public async Task ActAsync(TargetSpaceCtx ctx){

		// Gather up to 3 Dahan.
		await ctx.GatherUpToNDahan(3);

		// Remove up to 3 Health worth of Invaders per Dahan.
		await Cmd.RemoveUpToNHealthOfInvaders(3 * ctx.Dahan.CountAll).ActAsync(ctx);

		// --if you have:3 sun,3 water,2 animal--
		// If Dahan are present,
		if( 0<ctx.Dahan.CountAll && await ctx.YouHave(Threshold) ) {
			// Remove 1 Blight from target land,
			await ctx.RemoveBlight();
			// then Remove 1 Explorer, 1 Town, and 1 City from a land within Range 1.
			await new SourceSelector(ctx.Self.PowerRangeCalc.GetSpaceOptions(ctx.Tokens,new TargetCriteria(1)))
				.AddGroup(1,Human.Explorer)
				.AddGroup(1,Human.Town)
				.AddGroup(1,Human.City)
				.Config( SelectFrom.ASingleLand )
				.RemoveN(ctx.Self);
		}
		
	}

}