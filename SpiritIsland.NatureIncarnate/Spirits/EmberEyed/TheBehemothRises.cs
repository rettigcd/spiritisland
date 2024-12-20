namespace SpiritIsland.NatureIncarnate;

public class TheBehemothRises : IActionFactory, IHaveDynamicUseCounts {

	static public readonly SpecialRule Rule = new SpecialRule(
		"The Behemoth Rises", 
		"You have an Incarna.  Once/turn, during any phase, you may push it or Add/Move it to any of your Sacred Sites."
	);

	const string Name = "The Behemoth Rises";

	public int Used { get; set; }
	public int MaxUses { get; set; }

	public void Inc() { Used++; }

	#region IActionFactory (explicit)
	string IOption.Text => Name; // + (MaxUses == 1 ? "" : $"({Used+1})");
	string IActionFactory.Title => Name;
	bool IActionFactory.CouldActivateDuring( Phase speed, Spirit spirit ) => true;
	# endregion IActionFactory (explicit)

	public async Task ActivateAsync( Spirit self ){
		// Either: Push incarna OR Add or Move Incarna to any SS on island.

		// if on board
		EmberEyedBehemoth eeb = (EmberEyedBehemoth)self;
		Incarna incarna = eeb.Incarna;
		if(incarna.IsPlaced) {
			Space from = incarna.Space;
			// Move/Push
			await new TokenMover(eeb,"Move/Push", from,
					// to SS or adjacents
					eeb.Presence.SacredSites.Union(from.Adjacent_Existing).Distinct().ToArray()
				)
				.AddAll(incarna)
				.DoN();
		} else {
			Space? space = await self.Select( "Select space to place Incarna.", eeb.Presence.SacredSites, Present.Done );
			if(space is null) return;
			await space.AddAsync( incarna, 1 );
		}
	}


}

public interface IHaveDynamicUseCounts {
	public int Used {get; set; }
	public void Inc() { Used++; }
}