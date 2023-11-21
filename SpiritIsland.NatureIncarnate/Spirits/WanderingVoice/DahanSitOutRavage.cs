namespace SpiritIsland.NatureIncarnate;

public class DahanSitOutRavage : BaseModEntity, ISkipRavages {

	readonly IIncarnaToken _incarna;
	public DahanSitOutRavage( IIncarnaToken incarna ) {
		_incarna = incarna;
	}

	public UsageCost Cost => UsageCost.Free;
	public Task<bool> Skip( SpaceState space ) {
		if( _incarna.Space != null && space.InOrAdjacentTo.Contains(_incarna.Space)) {
			var dahan = space.HumanOfTag(Human.Dahan)
				.ToDictionary( x => x, x => space[x] )
				.ToCountDict();
			SitOutRavage.SitOutNextRavage(space, dahan);
		}
		return Task.FromResult(false);
	}
}