namespace SpiritIsland.NatureIncarnate;

/// <summary>
/// Causes any dahan in-or-adjacent-to the given Incarna, to not paritcipate in ravage.
/// Added to entire board for Wandering Voice
/// </summary>
public class DahanNearToIncarnaSitOutRavage : BaseModEntity, IConfigRavages {

	readonly IIncarnaToken _incarna;
	public DahanNearToIncarnaSitOutRavage( IIncarnaToken incarna ) {
		_incarna = incarna;
	}

	void IConfigRavages.Config( SpaceState space ) {
		if( _incarna.Space != null && space.InOrAdjacentTo.Contains(_incarna.Space)) {
			var dahan = space.HumanOfTag(Human.Dahan)
				.ToDictionary( x => x, x => space[x] )
				.ToCountDict();
			SitOutRavage.SitOutNextRavage(space, dahan);
		}
	}
}