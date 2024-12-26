namespace SpiritIsland.NatureIncarnate;

/// <summary> Causes any dahan in-or-adjacent-to the given Incarna, to not paritcipate in ravage. </summary>
/// <remarks> Added to entire board for Wandering Voice </remarks>
public class SpreadTumultAndDelusion( Spirit spirit ) : BaseModEntity, IConfigRavages {

	public const string Name = "Spread Tumult and Delusion";
	const string StrifeStopsRavage_Description = "In lands with or adjacent to Incarna: if Strife is present, Dahan do not participate in Ravage.";
	static public SpecialRule Rule => new SpecialRule( Name, AClarionVoiceGivenForm.IncarnaAddsStrife_Description + " " + StrifeStopsRavage_Description );

	Task IConfigRavages.Config( Space space ) {
		// In lands with or adjacent to Incarna
		if( spirit.Incarna.IsPlaced && space.InOrAdjacentTo.Contains(spirit.Incarna.Space)
			// if Strife is present
			&& space.HumanOfAnyTag(Human.Invader).Any(x=>x.StrifeCount>0)
		)
			// Dahan do not participate in Ravage.
			SitOutRavage.AllSitOutThisRavageAction(space,Human.Dahan);
		return Task.CompletedTask;
	}

}