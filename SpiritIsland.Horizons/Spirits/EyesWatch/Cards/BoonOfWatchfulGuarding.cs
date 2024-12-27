namespace SpiritIsland.Horizons;

public class BoonOfWatchfulGuarding {

	public const string Name = "Boon of Watchful Guarding";

	[SpiritCard(Name, 0, Element.Earth, Element.Plant, Element.Animal), Fast, AnySpirit]
	[Instructions("In one of Target Spirit's lands, Defend 4. Target Spirit may pay 1 Energy to instead Defend 8."), Artist(Artists.MoroRogers)]
	static public async Task ActAsync(TargetSpiritCtx ctx) {

		// In one of Target Spirit's lands, Defend 4.
		var space = await ctx.Self.SelectAlways("Defend 4 in", ctx.Other.Presence.Lands);
		var landCtx = ctx.Self.Target(space);
		landCtx.Defend(4);

		// Target Spirit may pay 1 Energy to instead Defend 8.
		if(0 < ctx.Other.Energy 
			&& await ctx.Other.UserSelectsFirstText("Pay 1 Energy to defend an additional 4?","Yes, defend 8 total.", "No thank you") 
		) {
			--ctx.Other.Energy;
			landCtx.Defend(4);
		}

	}

}
