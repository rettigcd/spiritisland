namespace SpiritIsland.NatureIncarnate;

/// <summary>
/// Causes any dahan in-or-adjacent-to the given Incarna, to not paritcipate in ravage.
/// Added to entire board for Wandering Voice
/// </summary>
public class DahanNearToIncarnaSitOutRavage : BaseModEntity, IConfigRavages {

	readonly Incarna _incarna;
	public DahanNearToIncarnaSitOutRavage( Incarna incarna ) {
		_incarna = incarna;
	}

	void IConfigRavages.Config( SpaceState space ) {
		if( _incarna.IsPlaced && space.InOrAdjacentTo.Contains(_incarna.Space)) {
			var dahan = space.HumanOfTag(Human.Dahan)
				.ToDictionary( x => x, x => space[x] )
				.ToCountDict();
			SitOutRavage.SitOutThisRavageAction(space, dahan);
		}
	}
}