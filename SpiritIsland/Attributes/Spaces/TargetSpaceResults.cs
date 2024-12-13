namespace SpiritIsland;

public class TargetSpaceResults{

	public TargetSpaceResults(Space space, Space[] sources) {
		Space = space;
		Sources = sources;
	}

	public Space Space { get; }
	public Space[]Sources { get; }
}

public record TargetRoute(Space source,Space target);

public class TargetRoutes {
	public TargetRoutes( IEnumerable<TargetRoute> routes ) {
		_routes = [..routes];
	}
	public Space[] Targets => _routes.Select(r=>r.target).Distinct().ToArray();
	public Space[] Sources => _routes.Select(r => r.source).Distinct().ToArray();
	public Space[] SourcesFor(Space target) => _routes.Where(x=>x.target == target).Select(x=>x.source).ToArray();
	public TargetRoute[] _routes;

	public void AddRoutes(IEnumerable<TargetRoute> routes) {
		_routes = _routes.Union(routes).Distinct().ToArray();
	}

	public TargetSpaceResults MakeResult(Space target) => new TargetSpaceResults(target,SourcesFor(target));
}