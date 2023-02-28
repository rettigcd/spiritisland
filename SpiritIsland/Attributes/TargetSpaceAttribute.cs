namespace SpiritIsland;

public abstract class TargetSpaceAttribute : GeneratesContextAttribute {

	public static SpaceState TargettedSpace => _targettedSpace.Value;
	static ActionScopeValue<SpaceState> _targettedSpace = new ActionScopeValue<SpaceState>("Targetted Space");

	readonly protected TargetingSourceCriteria _sourceCriteria;

	protected readonly string[] _targetFilters;
	protected readonly int _range;
	public override string TargetFilter { get; }

	public TargetSpaceAttribute(TargetingSourceCriteria sourceCriteria, int range, params string[] targetFilter ){
		_sourceCriteria = sourceCriteria;
		_range = range;
		TargetFilter = targetFilter.Length>0 ? string.Join("Or",targetFilter) : "Any";
		_targetFilters = targetFilter;
	}

	public override async Task<object> GetTargetCtx( string powerName, SelfCtx ctx ){

		var space = await ctx.Self.TargetsSpace( ctx, powerName+": Target Space", Preselect,
			_sourceCriteria,
			await GetCriteria( ctx )
		);
		if(space == null) return null;
		var target = ctx.Target( space );
		_targettedSpace.Value = target.Tokens;
		return target;
	}

	protected virtual async Task<TargetCriteria> GetCriteria( SelfCtx ctx ) 
		=> new TargetCriteria( await CalcRange( ctx ), ctx.Self, _targetFilters );

	/// <remarks>Hook so ExtendableRangeAttribute can increase range.</remarks>
	protected virtual Task<int> CalcRange( SelfCtx ctx ) => Task.FromResult( _range );

	public override LandOrSpirit LandOrSpirit => LandOrSpirit.Land;

	public IPreselect Preselect {get; set; }

}

[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method )]
public class FromPresenceAttribute : TargetSpaceAttribute {
	public FromPresenceAttribute( int range, params string[] filters )
		: base( new TargetingSourceCriteria( From.Presence ), range, filters ) {}
	public override string RangeText => _range.ToString();
}

[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method )]
public class FromPresenceInAttribute : TargetSpaceAttribute {
	public FromPresenceInAttribute( int range, Terrain sourceTerrain, string filter = Target.Any )
		: base( new TargetingSourceCriteria( From.Presence, sourceTerrain), range, filter ) {}
	public override string RangeText => $"{_range}({_sourceCriteria.Terrain})";
}

[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method )]
public class FromSacredSiteAttribute : TargetSpaceAttribute {
	public FromSacredSiteAttribute( int range, params string[] filters )
		: base( new TargetingSourceCriteria( From.SacredSite ), range, filters ) { }
	public override string RangeText => $"ss:{_range}";
}

[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method )]
public class PreselectAttribute : Attribute, IPreselect {
	public PreselectAttribute( string prompt, string classString ) {
		Prompt = prompt;
		TokenClasses = classString.Split( ',' )
			.Select( x => x switch {
				"Explorer" => (IEntityClass)Human.Explorer,
				"Town" => (IEntityClass)Human.Town,
				"City" => (IEntityClass)Human.City,
				"Beast" => (IEntityClass)Token.Beast,
				"Disease" => (IEntityClass)Token.Disease,
				"Wilds" => (IEntityClass)Token.Wilds,
				"Dahan" => (IEntityClass)Human.Dahan,
				_ => throw new Exception( $"{x} not known" )
			} ).ToArray();
	}

	public string Prompt { get; }

	public IEntityClass[] TokenClasses { get; }

}

/// <summary> Provides token/prompt info to enable selecing token and space at the same time (when appropriate). </summary>
public interface IPreselect {
	string Prompt { get; }
	IEntityClass[] TokenClasses { get; }
}

