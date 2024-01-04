using System.Linq;

namespace SpiritIsland;

/// <summary>
/// Provides a filters to some context (Ctx) along with a description.
/// </summary>
public class CtxFilter<Ctx> {

	static public CtxFilter<Ctx> NullFilter => new CtxFilter<Ctx>( "", _ => true );

	/// <summary>
	/// Build a filter that evaluates 1 context at a time.
	/// </summary>
	public CtxFilter( string description, Func<Ctx, bool> singleFilter ) {
		Description = description;
		_groupFilter = x => x.Where( singleFilter );
	}

	/// <summary>
	/// Build a filter that evaluates all contexts as in a group.
	/// </summary>
	public CtxFilter( string description, Func<IEnumerable<Ctx>, IEnumerable<Ctx>> filter ) {
		Description = description;
		_groupFilter = filter;
	}

	public readonly string Description;

	public IEnumerable<Ctx> Filter( IEnumerable<Ctx> src ) => _groupFilter(src);

	Func<IEnumerable<Ctx>, IEnumerable<Ctx>> _groupFilter;

}


// !! Deprecate this and just use CtxFilter<TargetSpaceCtx>
public class TargetSpaceCtxFilter : CtxFilter<TargetSpaceCtx> {
	public TargetSpaceCtxFilter( string description, Func<TargetSpaceCtx, bool> filter ):base( description, filter) {
		MyFilter = filter;
	}
	public readonly Func<TargetSpaceCtx, bool> MyFilter;

}

// !! Deprecate this annd just use CtxFilter<Spirit>
public class SpiritFilter : CtxFilter<Spirit> {
	public SpiritFilter( string description, Func<Spirit, bool> filter ):base( description, filter ) {}
}
