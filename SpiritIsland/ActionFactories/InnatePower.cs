namespace SpiritIsland;

public class InnatePower : IPowerActionFactory {

	#region Constructors and factories

	// don't use <T> because it generates a unique method for each (63+) types.
	static public InnatePower For(Type actionType) => new InnatePower( actionType );

	static GeneratesContextAttribute GetContextFromAttribute( Type actionType ) => actionType.GetCustomAttributes<GeneratesContextAttribute>().VerboseSingle( actionType.Name + " must have Single Target space or Target spirit attribute" );

	protected InnatePower(Type actionType){

		_innatePowerAttr = actionType.GetCustomAttribute<InnatePowerAttribute>();
		_speedAttr = actionType.GetCustomAttribute<SpeedAttribute>(false) 
			?? throw new InvalidOperationException("Missing Speed attribute for "+actionType.Name);
		_targetAttr = GetContextFromAttribute( actionType );
		_repeatAttr = actionType.GetCustomAttribute<RepeatAttribute>();

		Title = _innatePowerAttr.Name;
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
			.Cast<IDrawableInnateTier>()
			.ToList();
		if(_repeatAttr is not null)
			drawableOptions.AddRange( _repeatAttr.ThresholdTiers );
		DrawableOptions = drawableOptions;

	}

	#endregion

	#region Speed
	public Phase Speed => _speedAttr.DisplaySpeed;
	/// <summary> When set, overrides the speed attribute for everything except Display Speed </summary>
	public ISpeedBehavior OverrideSpeedBehavior { get; set; }

	public bool CouldActivateDuring( Phase phase, Spirit spirit ) {
		return CouldBeTriggered( spirit )
			&& CouldMatchPhase( phase, spirit );
	}

	bool CouldBeTriggered( Spirit spirit ) {
		return DrawableOptions
			.Any(x=>spirit.Elements.CouldHave(x.Elements) != ECouldHaveElements.No);
	}

	bool CouldMatchPhase( Phase requestSpeed, Spirit spirit ) {
		return SpeedBehavior.CouldBeActiveFor( requestSpeed, spirit );
	}

	ISpeedBehavior SpeedBehavior => _speedAttr;

	#endregion

	#region Drawing Properties

	public string Title {get;}

	string IOption.Text => IOption_Text; // non-overridable, hiding IOption.Text so it isn't used for non IOption stuff
	virtual protected string IOption_Text => Title; // allow Dirived types to add additional info

	public string TargetFilter => _targetAttr.TargetFilterName;

	public string RangeText => _targetAttr.RangeText;

	public LandOrSpirit LandOrSpirit => _targetAttr.LandOrSpirit;

	public string GeneralInstructions { get; }

	public IEnumerable<IDrawableInnateTier> DrawableOptions { get; }

	#endregion

	public async Task ActivateAsync( Spirit spirit ) {

		await ActivateInnerAsync( spirit );
		if( _repeatAttr != null) {
			var repeater = _repeatAttr.GetRepeater();
			while( await repeater.ShouldRepeat( spirit ) )
				await ActivateInnerAsync( spirit );
		}
	}


	protected virtual async Task ActivateInnerAsync( Spirit self ) {

		// Do this 1st so Volcano can destroy its presence before we evaluate our options
		LastTarget = await _targetAttr.GetTargetCtx( Title, self );
		if(LastTarget == null) return;

		List<MethodInfo> lastMethods = await GetLastActivatedMethodsOfEachGroup( self );
		if( lastMethods.Count == 0 ) {
			LastTarget = null;
			return;
		}

		var objList = new object[] { LastTarget };
		foreach(var method in lastMethods)
			await (Task)method.Invoke( null, objList );

	}

	async Task<List<MethodInfo>> GetLastActivatedMethodsOfEachGroup( Spirit self ) {

		// Not using LINQ because of the AWAIT in the loop.

		var lastMethods = new List<MethodInfo>();
		foreach(MethodTuple[] grp in _executionGroups) {

			// Ask spirit which methods they can activate
			var match = await SelectInnateTierToActivate(self, grp.Select(g=>g.Attr) );

			// Find matching method and it to execute-list
			MethodInfo method = grp.FirstOrDefault(g=>g.Attr==match)?.Method;
			if(method != null)
				lastMethods.Add( method );
		}
		return lastMethods;
	}

	// !!! Instead of putting this on Elements, it seems like it should go on the InnatePower instead.
	// Overriden by:
	//	* Shifting Memories
	//	* Volcano
	protected virtual async Task<IDrawableInnateTier> SelectInnateTierToActivate(Spirit spirit, IEnumerable<IDrawableInnateTier> innateOptions) {

		// return await spirit.Elements.SelectInnateTierToActivate(innateOptions);

		IDrawableInnateTier match = null;
		foreach( var option in innateOptions.OrderBy(o => o.Elements.Total) )
			if( await HasMetTierThreshold(spirit, option) )
				match = option;
		return match;
	}

	protected virtual async Task<bool> HasMetTierThreshold(Spirit spirit, IDrawableInnateTier option) {
		return await spirit.Elements.MeetThreshold(option.Elements, "Innate Tier" );
	}

	public object LastTarget { get; private set; } // for use in a power-action event, would be better to have ActAsync just return it.

	readonly InnatePowerAttribute _innatePowerAttr;
	readonly protected SpeedAttribute _speedAttr;
	readonly GeneratesContextAttribute _targetAttr;
	readonly RepeatAttribute _repeatAttr;
	readonly MethodTuple[][] _executionGroups;

	class MethodTuple( MethodInfo _m ) {
		public MethodInfo Method { get; } = _m;
		public InnateTierAttribute Attr { get; } = _m.GetCustomAttributes<InnateTierAttribute>().FirstOrDefault();
	}

}

public enum LandOrSpirit { None, Land, Spirit }
