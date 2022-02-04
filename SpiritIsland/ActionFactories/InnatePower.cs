namespace SpiritIsland;

public class InnatePower : IFlexibleSpeedActionFactory, IRecordLastTarget {

	#region Constructors and factories

	static public InnatePower For<T>(){ 
		Type actionType = typeof(T);
		var contextAttr = actionType.GetCustomAttributes<GeneratesContextAttribute>().VerboseSingle(actionType.Name+" must have Single Target space or Target spirit attribute");
		return new InnatePower( actionType, contextAttr );
	}

	internal InnatePower(Type actionType,GeneratesContextAttribute targetAttr){

		innatePowerAttr = actionType.GetCustomAttribute<InnatePowerAttribute>();
		speedAttr = actionType.GetCustomAttribute<SpeedAttribute>(false) 
			?? throw new InvalidOperationException("Missing Speed attribute for "+actionType.Name);
		this.targetAttr = targetAttr;
		this.repeatAttr = actionType.GetCustomAttribute<RepeatAttribute>();

		Name = innatePowerAttr.Name;
		GeneralInstructions = innatePowerAttr.GeneralInstructions;

		// try static method (spirit / major / minor)
		var elementListByMethod = actionType
			.GetMethods( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static )
			.Select( m => new MethodTuple(m) )
			.Where( x => x.Attr != null )
			.ToList();

		executionGroups = elementListByMethod
			// filter first - so we only have groups that have matches
			.Where( x => x.Attr.Group.HasValue )
			.GroupBy( x => x.Attr.Group.Value )
			.Select( x => x.ToArray() )
			.ToArray();

		var drawableOptions = elementListByMethod
			.Select(x=>x.Attr)
			.Cast<IDrawableInnateOption>()
			.ToList();
		if(this.repeatAttr!=null)
			drawableOptions.AddRange( repeatAttr.Thresholds );
		DrawableOptions = drawableOptions;

	}

	#endregion

	#region Speed
	public Phase DisplaySpeed => speedAttr.DisplaySpeed;
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

	ISpeedBehavior SpeedBehavior => OverrideSpeedBehavior ?? speedAttr;

	#endregion

	#region Drawing Properties

	public string Name {get;}

	public string Text => Name;

	public string TargetFilter => this.targetAttr.TargetFilter;

	public string RangeText => this.targetAttr.RangeText;

	public LandOrSpirit LandOrSpirit => targetAttr.LandOrSpirit;

	public string GeneralInstructions { get; }

	public IEnumerable<IDrawableInnateOption> DrawableOptions { get; }

	#endregion

	public async Task ActivateAsync( SelfCtx ctx ) {

		if(!await SpeedBehavior.IsActiveFor( ctx.GameState.Phase, ctx.Self )) {
			LastTarget = null;
			return; // this is here so Shifting Memory can decline to Activate Innates at this speed
		}

		await ActivateInnerAsync( ctx );
		if( repeatAttr != null) {
			var repeater = repeatAttr.GetRepeater();
			while( await repeater.ShouldRepeat(ctx.Self) )
				await ActivateInnerAsync( ctx );
		}
	}

	async Task ActivateInnerAsync( SelfCtx spiritCtx ) {

		List<MethodInfo> lastMethods = await GetLastActivatedMethodsOfEachGroup( spiritCtx );
		if( lastMethods.Count == 0 ) {
			LastTarget = null;
			return;
		}

		LastTarget = await targetAttr.GetTargetCtx( Name, spiritCtx, TargettingFrom.Innate );
		if(LastTarget == null) return;

		var objList = new object[] { LastTarget };
		foreach(var method in lastMethods)
			await (Task)method.Invoke( null, objList );

	}

	async Task<List<MethodInfo>> GetLastActivatedMethodsOfEachGroup( SelfCtx spiritCtx ) {

		// Not using LINQ because of the AWAIT in the loop.

		var lastMethods = new List<MethodInfo>();
		foreach(MethodTuple[] grp in executionGroups) {

			// Ask spirit which methods they can activate
			ElementCounts match = await spiritCtx.Self.SelectInnateToActivate( grp.Select(g=>g.Attr) );

			// Find matching method and it to execute-list
			MethodInfo method = grp.FirstOrDefault(g=>g.Elements==match)?.Method;
			if(method != null)
				lastMethods.Add( method );
		}
		return lastMethods;
	}

	public object LastTarget { get; private set; } // for use in a power-action event, would be better to have ActAsync just return it.

	readonly InnatePowerAttribute innatePowerAttr;
	readonly protected SpeedAttribute speedAttr;
	readonly GeneratesContextAttribute targetAttr;
	readonly RepeatAttribute repeatAttr;
	readonly MethodTuple[][] executionGroups;

	class MethodTuple {
		public MethodTuple(MethodInfo m ) {
			Method = m;
			Attr = m.GetCustomAttributes<InnateOptionAttribute>().FirstOrDefault();
		}
		public MethodInfo Method { get; }
		public InnateOptionAttribute Attr { get; }
		public ElementCounts  Elements => Attr.Elements;

		// Execution
		// Display Words
		// Elements
	}

	public static string[] Tokenize( string s ) => TokenParser.Tokenize( s );

}

public static class TokenParser {
	public static string[] Tokenize( string s ) {

		var tokens = new Regex( "sacred site|presence|fast|slow"
			+ "|dahan|blight|fear|city|town|explorer"
			+ "|sun|moon|air|fire|water|plant|animal|earth"
			+ "|beast|disease|strife|wilds|badlands"
			+ "|\\+1range" 
		).Matches( s ).Cast<Match>().ToList();

		var results = new List<string>();

		int cur = 0;
		while(cur < s.Length) {
			// no more tokens, go to the end
			if(tokens.Count == 0) {
				results.Add( s[cur..] );
				break;
			}
			var nextToken = tokens[0];
			if(nextToken.Index == cur) {
				results.Add( "{"+nextToken.Value+"}" );
				cur = nextToken.Index + nextToken.Length;
				tokens.RemoveAt( 0 );
			} else {
				results.Add( s[cur..nextToken.Index] );
				cur = nextToken.Index;
			}
		}
		return results.ToArray();
	}

}

public enum LandOrSpirit { None, Land, Spirit }


