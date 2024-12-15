namespace SpiritIsland;

public sealed class PowerCard : IPowerActionFactory {

	#region static Factories

	static public PowerCard For(Type type) => For(FindMethod(type));

	static MethodInfo FindMethod(Type type) {
		// try static method (spirit / major / minor)
		return type.GetMethods(BindingFlags.Public | BindingFlags.Static)
			.Where(m => m.GetCustomAttributes<CardAttribute>().Count() == 1)
			.VerboseSingle($"PowerCard {type.Name} missing static method with SpiritCard, MinorCard or MajorCard attribute");
	}

	static public PowerCard For(MethodInfo method) {
		GeneratesContextAttribute contextGenerator = method.GetCustomAttributes<AnySpiritAttribute>().Cast<GeneratesContextAttribute>().FirstOrDefault()
			?? method.GetCustomAttributes<TargetSpaceAttribute>().First();
		var cardDetails = method.GetCustomAttributes<CardAttribute>().VerboseSingle("Couldn't find CardAttribute on PowerCard targeting a space");
		return new PowerCard(method, contextGenerator, cardDetails);
	}

	#endregion static Factories

	#region constructor

	PowerCard( 
		MethodBase methodBase, 
		GeneratesContextAttribute targetAttr,
		IHaveCardDetails cardDetails
	) {
		_methodBase = methodBase;
		_targetAttr = targetAttr;
		_cardDetails = cardDetails;
		_speedAttr = methodBase.GetCustomAttribute<SpeedAttribute>(false) ?? throw new InvalidOperationException("Missing Speed attribute for "+methodBase.DeclaringType.Name);
		_repeatAttr = methodBase.GetCustomAttribute<RepeatAttribute>();

		if(_targetAttr is TargetSpaceAttribute tsa )
			tsa.Preselect = methodBase.GetCustomAttribute<PreselectAttribute>();
	}

	#endregion

	string IOption.Text        => $"{Title} ${Cost} ({Speed})";
	public string Title         => _cardDetails.Name;
	public Phase Speed  => _speedAttr.DisplaySpeed;
	public ISpeedBehavior OverrideSpeedBehavior { get; set; }

	// These are only used for drawing the cards.
	public string Instructions => _methodBase.GetCustomAttribute<InstructionsAttribute>(false)?.Text ?? throw new InvalidOperationException( "Missing Instructions attribute for " + _methodBase.DeclaringType.Name );
	public string Artist => _methodBase.GetCustomAttribute<ArtistAttribute>(false)?.Artist ?? throw new InvalidOperationException( "Missing Artist attribute for " + _methodBase.DeclaringType.Name );

	public string TargetFilter => _targetAttr.TargetFilterName;
	/// <summary> Used by PowerCardImageManager to draw the range-text on the card. </summary>
	public string RangeText => _targetAttr.RangeText;

	public int Cost            => _cardDetails.Cost;
	public CountDictionary<Element> Elements  => _cardDetails.Elements;
	public PowerType PowerType => _cardDetails.PowerType;
	public Type MethodType     => _methodBase.DeclaringType; // for determining card namespace and Basegame, BranchAndClaw, etc

	public LandOrSpirit LandOrSpirit => _targetAttr.LandOrSpirit;

	public bool CouldActivateDuring( Phase requestSpeed, Spirit spirit ){
		return SpeedBehavior.CouldBeActiveFor(requestSpeed,spirit);
	}

	public async Task ActivateAsync( Spirit self ) {
		// Don't check speed here.  Slow card may have been made fast (Lightning's Swift Strike)

		await ActivateInnerAsync( self );
		if(_repeatAttr != null) {
			var repeater = _repeatAttr.GetRepeater();
			while(await repeater.ShouldRepeat( self )) {
				await using var anotherScope = await ActionScope.StartSpiritAction(ActionCategory.Spirit_Power,self);
				await ActivateInnerAsync( self );
			}
		}

	}


	async Task ActivateInnerAsync( Spirit self ) {
		object target = await _targetAttr.GetTargetCtx(Title, self);
		if( target is not null )
			await InvokeOnObjectCtx(target);
	}

	/// <remarks>Called directly from Let's See What Happens with special ContextBehavior</remarks>
	public Task InvokeOn( TargetSpaceCtx ctx ) {
		return _targetAttr.LandOrSpirit == LandOrSpirit.Land
			? InvokeOnObjectCtx( ctx )
			: throw new InvalidOperationException( "Cannot invoke spirit-based PowerCard using TargetSpaceCtx" );
	}

	Task InvokeOnObjectCtx(object ctx) => (Task)_methodBase.Invoke( null, [ctx] );

	#region private

	ISpeedBehavior SpeedBehavior => _speedAttr;

	readonly SpeedAttribute _speedAttr;
	readonly IHaveCardDetails _cardDetails;
	readonly MethodBase _methodBase;
	readonly RepeatAttribute _repeatAttr;

	#endregion

	readonly GeneratesContextAttribute _targetAttr;

}
