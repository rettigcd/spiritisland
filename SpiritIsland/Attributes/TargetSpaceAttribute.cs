namespace SpiritIsland;

public abstract class TargetSpaceAttribute : GeneratesContextAttribute {

	//readonly protected From fromSourceEnum;
	//readonly protected Terrain? sourceTerrain;
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

		//TokenClass[] preselects = powerName switch {
		//	"Massive Flooding" => Invader.Explorer_Town,
		//	"Flash Floods" => Invader.Any,
		//	"Wash Away" => Invader.Explorer_Town,
		//	_ => null
		//};

		var space = await ctx.Self.TargetsSpace( ctx, powerName+": Target Space", Preselect,
			_sourceCriteria,
			await GetCriteria( ctx )
		);
		return space == null ? null : ctx.Target(space);
	}

	protected virtual async Task<TargetCriteria> GetCriteria( SelfCtx ctx ) 
		=> ctx.TerrainMapper.Specify( await CalcRange( ctx ), _targetFilters );

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
				"Explorer" => (TokenClass)Human.Explorer,
				"Town" => (TokenClass)Human.Town,
				"City" => (TokenClass)Human.City,
				"Beast" => (TokenClass)Token.Beast,
				"Disease" => (TokenClass)Token.Disease,
				"Wilds" => (TokenClass)Token.Wilds,
				"Dahan" => (TokenClass)Human.Dahan,
				_ => throw new Exception( $"{x} not known" )
			} ).ToArray();
	}

	public string Prompt { get; }

	public TokenClass[] TokenClasses { get; }

}

/// <summary> Provides token/prompt info to enable selecing token and space at the same time (when appropriate). </summary>
public interface IPreselect {
	string Prompt { get; }
	TokenClass[] TokenClasses { get; }
}

