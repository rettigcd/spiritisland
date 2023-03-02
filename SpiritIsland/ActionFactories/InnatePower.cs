namespace SpiritIsland;

public class InnatePower : IFlexibleSpeedActionFactory {

	#region Constructors and factories

	static public InnatePower For<T>(){ 
		Type actionType = typeof(T);
		var contextAttr = actionType.GetCustomAttributes<GeneratesContextAttribute>().VerboseSingle(actionType.Name+" must have Single Target space or Target spirit attribute");
		return new InnatePower( actionType, contextAttr );
	}

	protected InnatePower(Type actionType, GeneratesContextAttribute targetAttr){

		_innatePowerAttr = actionType.GetCustomAttribute<InnatePowerAttribute>();
		_speedAttr = actionType.GetCustomAttribute<SpeedAttribute>(false) 
			?? throw new InvalidOperationException("Missing Speed attribute for "+actionType.Name);
		this._targetAttr = targetAttr;
		this._repeatAttr = actionType.GetCustomAttribute<RepeatAttribute>();

		Name = _innatePowerAttr.Name;
		GeneralInstructions = _innatePowerAttr.GeneralInstructions;

		// try static method (spirit / major / minor)
		var elementListByMethod = actionType
			.GetMethods( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static )
			.Select( m => new MethodTuple(m) )
			.Where( x => x.Attr != null )
			.ToList();

		_executionGroups = elementListByMethod
			// filter first - so we only have groups that have matches
			.Where( x => x.Attr.Group.HasValue )
			.GroupBy( x => x.Attr.Group.Value )
			.Select( x => x.ToArray() )
			.ToArray();

		var drawableOptions = elementListByMethod
			.Select(x=>x.Attr)
			.Cast<IDrawableInnateOption>()
			.ToList();
		if(this._repeatAttr!=null)
			drawableOptions.AddRange( _repeatAttr.Thresholds );
		DrawableOptions = drawableOptions;

	}

	#endregion

	#region Speed
	public Phase DisplaySpeed => _speedAttr.DisplaySpeed;
	/// <summary> When set, overrides the speed attribute for everything except Display Speed </summary>
	public ISpeedBehavior OverrideSpeedBehavior { get; set; }

	public bool CouldActivateDuring( Phase phase, Spirit spirit ) {
		return CouldBeTriggered( spirit )
			&& CouldMatchPhase( phase, spirit );
	}

	bool CouldBeTriggered( Spirit spirit ) {
		return DrawableOptions
			.Any(x=>spirit.CouldHaveElements(x.Elements));
	}

	bool CouldMatchPhase( Phase requestSpeed, Spirit spirit ) {
		return SpeedBehavior.CouldBeActiveFor( requestSpeed, spirit );
	}

	ISpeedBehavior SpeedBehavior => OverrideSpeedBehavior ?? _speedAttr;

	#endregion

	#region Drawing Properties

	public string Name {get;}

	public string Text => Name;

	public string TargetFilter => this._targetAttr.TargetFilter;

	public string RangeText => this._targetAttr.RangeText;

	public LandOrSpirit LandOrSpirit => _targetAttr.LandOrSpirit;

	public string GeneralInstructions { get; }

	public IEnumerable<IDrawableInnateOption> DrawableOptions { get; }

	#endregion

	public async Task ActivateAsync( SelfCtx ctx ) {
		await ActivateInnerAsync( ctx );
		if( _repeatAttr != null) {
			var repeater = _repeatAttr.GetRepeater();
			while( await repeater.ShouldRepeat(ctx.Self) )
				await ActivateInnerAsync( ctx );
		}
	}

	protected virtual async Task ActivateInnerAsync( SelfCtx spiritCtx ) {

		// Do this 1st so Volcano can destroy its presence before we evaluate our options
		LastTarget = await _targetAttr.GetTargetCtx( Name, spiritCtx );
		if(LastTarget == null) return;

		List<MethodInfo> lastMethods = await GetLastActivatedMethodsOfEachGroup( spiritCtx );
		if( lastMethods.Count == 0 ) {
			LastTarget = null;
			return;
		}

		var objList = new object[] { LastTarget };
		foreach(var method in lastMethods)
			await (Task)method.Invoke( null, objList );

	}

	async Task<List<MethodInfo>> GetLastActivatedMethodsOfEachGroup( SelfCtx spiritCtx ) {

		// Not using LINQ because of the AWAIT in the loop.

		var lastMethods = new List<MethodInfo>();
		foreach(MethodTuple[] grp in _executionGroups) {

			// Ask spirit which methods they can activate
			var match = await spiritCtx.Self.SelectInnateToActivate( grp.Select(g=>g.Attr) );

			// Find matching method and it to execute-list
			MethodInfo method = grp.FirstOrDefault(g=>g.Attr==match)?.Method;
			if(method != null)
				lastMethods.Add( method );
		}
		return lastMethods;
	}

	public object LastTarget { get; private set; } // for use in a power-action event, would be better to have ActAsync just return it.

	readonly InnatePowerAttribute _innatePowerAttr;
	readonly protected SpeedAttribute _speedAttr;
	readonly GeneratesContextAttribute _targetAttr;
	readonly RepeatAttribute _repeatAttr;
	readonly MethodTuple[][] _executionGroups;

	class MethodTuple {
		public MethodTuple(MethodInfo m ) {
			Method = m;
			Attr = m.GetCustomAttributes<InnateOptionAttribute>().FirstOrDefault();
		}
		public MethodInfo Method { get; }
		public InnateOptionAttribute Attr { get; }
//		public ElementCounts  Elements => Attr.Elements;

		// Execution
		// Display Words
		// Elements
	}

}

public enum LandOrSpirit { None, Land, Spirit }


