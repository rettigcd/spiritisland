namespace SpiritIsland;

public sealed class PowerCard : IFlexibleSpeedActionFactory {

	#region constructor

	PowerCard( MethodBase methodBase, GeneratesContextAttribute targetAttr ) {
		_methodBase = methodBase;
		_targetAttr = targetAttr;
		_cardAttr = methodBase.GetCustomAttributes<CardAttribute>().VerboseSingle( "Couldn't find CardAttribute on PowerCard targeting a space" );
		_speedAttr = methodBase.GetCustomAttribute<SpeedAttribute>(false) ?? throw new InvalidOperationException("Missing Speed attribute for "+methodBase.DeclaringType.Name);
		_repeatAttr = methodBase.GetCustomAttribute<RepeatAttribute>();

		if(_targetAttr is TargetSpaceAttribute tsa )
			tsa.Preselect = methodBase.GetCustomAttribute<PreselectAttribute>();
	}

	#endregion

	public string Text => $"{Name} ${Cost} ({DisplaySpeed})";
	public string Name         => _cardAttr.Name;
	public Phase DisplaySpeed         => _speedAttr.DisplaySpeed;
	public ISpeedBehavior OverrideSpeedBehavior { get; set; }

	// These are only used for drawing the cards.
	public string Instructions => _methodBase.GetCustomAttribute<InstructionsAttribute>(false)?.Text ?? throw new InvalidOperationException( "Missing Instructions attribute for " + _methodBase.DeclaringType.Name );
	public string Artist => _methodBase.GetCustomAttribute<ArtistAttribute>(false)?.Artist ?? throw new InvalidOperationException( "Missing Artist attribute for " + _methodBase.DeclaringType.Name );

	public string TargetFilter => _targetAttr.TargetFilterName;
	/// <summary> Used by PowerCardImageManager to draw the range-text on the card. </summary>
	public string RangeText => _targetAttr.RangeText;

	public int Cost            => _cardAttr.Cost;
	public CountDictionary<Element> Elements  => _cardAttr.Elements;
	public PowerType PowerType => _cardAttr.PowerType;
	public Type MethodType     => _methodBase.DeclaringType; // for determining card namespace and Basegame, BranchAndClaw, etc

	public LandOrSpirit LandOrSpirit => _targetAttr.LandOrSpirit;

	public bool CouldActivateDuring( Phase requestSpeed, Spirit spirit ){
		return SpeedBehavior.CouldBeActiveFor(requestSpeed,spirit);
	}

	public async Task ActivateAsync( SelfCtx ctx ) {
		// Don't check speed here.  Slow card may have been made fast (Lightning's Swift Strike)

		await ActivateInnerAsync( ctx );
		if(_repeatAttr != null) {
			var repeater = _repeatAttr.GetRepeater();
			while(await repeater.ShouldRepeat( ctx.Self )) {
				await using var anotherScope = await ActionScope.StartSpiritAction(ActionCategory.Spirit_Power,ctx.Self);
				await ActivateInnerAsync( ctx );
			}
		}

	}

	async Task ActivateInnerAsync( SelfCtx spiritCtx ) {
		LastTarget = await _targetAttr.GetTargetCtx( Name, spiritCtx );
		if(LastTarget != null) // Can't find a tar
			await InvokeOnObjectCtx( LastTarget );
	}

	/// <remarks>Called directly from Let's See What Happens with special ContextBehavior</remarks>
	public Task InvokeOn( TargetSpaceCtx ctx ) {
		return _targetAttr.LandOrSpirit == LandOrSpirit.Land
			? InvokeOnObjectCtx( ctx )
			: throw new InvalidOperationException( "Cannot invoke spirit-based PowerCard using TargetSpaceCtx" );
	}

	Task InvokeOnObjectCtx(object ctx) => (Task)_methodBase.Invoke( null, new object[] { ctx } );

	#region private

	ISpeedBehavior SpeedBehavior => OverrideSpeedBehavior ?? _speedAttr;

	public object LastTarget { get; private set; }

	readonly SpeedAttribute _speedAttr;
	readonly CardAttribute _cardAttr;
	readonly MethodBase _methodBase;
	readonly RepeatAttribute _repeatAttr;

	#endregion

	#region static

	static public PowerCard For( Type type ) => For( FindMethod( type ) );

	static MethodInfo FindMethod( Type type ) {
		// try static method (spirit / major / minor)
		return type.GetMethods( BindingFlags.Public | BindingFlags.Static )
			.Where( m => m.GetCustomAttributes<CardAttribute>().Count() == 1 )
			.VerboseSingle( $"PowerCard {type.Name} missing static method with SpiritCard, MinorCard or MajorCard attribute" );
	}

	static public PowerCard For( MethodInfo method ) {

		// check if targets spirit
		AnySpiritAttribute targetSpiritAttribute = method.GetCustomAttributes<AnySpiritAttribute>().FirstOrDefault();
		if( targetSpiritAttribute != null )
			return new PowerCard( method, targetSpiritAttribute );

		// Must be target-land
		TargetSpaceAttribute targetSpace = method.GetCustomAttributes<TargetSpaceAttribute>().FirstOrDefault();
		return new PowerCard( method, targetSpace );
	}

	#endregion

	readonly GeneratesContextAttribute _targetAttr;

}
