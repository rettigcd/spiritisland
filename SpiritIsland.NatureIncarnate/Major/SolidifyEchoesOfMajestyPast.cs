namespace SpiritIsland.NatureIncarnate;

public class SolidifyEchoesOfMajestyPast {

	public const string Name = "Solidify Echoes of Majesty Past";

	[MajorCard(Name,4,"sun,moon,air,earth"),Fast]
	[AnySpirit]
	[Instructions( "Choose one of target Spirit's lands.  In that land and each adjacent land, Defend 3. They Add 1 DestroyedPresence to each adjacent land. Skip up to 1 Invader Action at each added DestroyedPresence. -If you have- 2 sun,2 moon,2 earth: Target Spirit either Relaims 1 Power Card ore re-gains a Unique Power they previously forgot. They may play it by paying its cost." ), Artist( Artists.EmilyHancock )]
	static public Task ActAsync(TargetSpaceCtx ctx){
		return Task.CompletedTask;
	}

}

