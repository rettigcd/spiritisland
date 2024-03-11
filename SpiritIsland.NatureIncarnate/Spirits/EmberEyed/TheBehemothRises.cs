namespace SpiritIsland.NatureIncarnate;

public class TheBehemothRises : IActionFactory, IHaveDynamicUseCounts {
	const string Name = "The Behemoth Rises";

	public int Used { get; set; }
	public int MaxUses { get; set; }

	#region IActionFactory (explicit)
	string IOption.Text => Name + (MaxUses == 1 ? "" : $"({Used+1} of {MaxUses})");
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
			Space? space = await self.SelectAsync( new A.SpaceDecision( "Select space to place Incarna.", eeb.Presence.SacredSites, Present.Done ) );
			if(space == null) return;
			await space.AddAsync( incarna, 1 );
		}
	}


}

public interface IHaveDynamicUseCounts {
	public int Used {get; set; }
	public int MaxUses { get; set; }
}