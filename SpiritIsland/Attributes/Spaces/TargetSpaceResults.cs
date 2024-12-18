#nullable enable
namespace SpiritIsland;

public class TargetSpaceResults(Space space, Space[] sources) {
	public Space Space { get; } = space;
	public Space[] Sources { get; } = sources;
}

public record TargetRoute(Space source,Space target);

public class TargetRoutes(IEnumerable<TargetRoute> routes) {
	public Space[] Targets => _routes.Select(r=>r.target).Distinct().ToArray();
	public Space[] Sources => _routes.Select(r => r.source).Distinct().ToArray();
	public Space[] SourcesFor(Space target) => _routes.Where(x=>x.target == target).Select(x=>x.source).ToArray();
	public TargetRoute[] _routes = [.. routes];

	public void AddRoutes(IEnumerable<TargetRoute> routes) {
		_routes = _routes.Union(routes).Distinct().ToArray();
	}

	public TargetSpaceResults MakeResult(Space target) => new TargetSpaceResults(target, SourcesFor(target));
}