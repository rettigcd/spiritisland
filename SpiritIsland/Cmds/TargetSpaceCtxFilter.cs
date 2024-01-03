namespace SpiritIsland;

/// <summary>
/// Provides a filters to some context (Ctx) along with a description.
/// </summary>
public class CtxFilter<Ctx> {

	public CtxFilter( string description, Func<Ctx, bool> filter ) {
		Description = description;
		Filter = filter;
	}
	public readonly Func<Ctx, bool> Filter;
	public readonly string Description;
}


// !! Deprecate this and just use CtxFilter<TargetSpaceCtx>
public class TargetSpaceCtxFilter : CtxFilter<TargetSpaceCtx> {
	public TargetSpaceCtxFilter( string description, Func<TargetSpaceCtx, bool> filter ):base( description, filter) {}
}

// !! Deprecate this annd just use CtxFilter<Spirit>
public class SpiritFilter : CtxFilter<Spirit> {
	public SpiritFilter( string description, Func<Spirit, bool> filter ):base( description, filter ) {}
}
