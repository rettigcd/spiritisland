namespace SpiritIsland.NatureIncarnate;

/// <summary>
/// Causes any dahan in-or-adjacent-to the given Incarna, to not paritcipate in ravage.
/// Added to entire board for Wandering Voice
/// </summary>
public class DahanNearToIncarnaSitOutRavage( Incarna incarna ) : BaseModEntity, IConfigRavages {

	readonly Incarna _incarna = incarna;

	Task IConfigRavages.Config( Space space ) {
		if( _incarna.IsPlaced && space.InOrAdjacentTo.Contains(_incarna.Space)) {
			var dahan = space.HumanOfTag(Human.Dahan)
				.ToDictionary( x => x, x => space[x] )
				.ToCountDict();
			SitOutRavage.SitOutThisRavageAction(space, dahan);
		}
		return Task.CompletedTask;
	}
}