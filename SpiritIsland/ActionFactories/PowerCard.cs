namespace SpiritIsland;

public sealed class PowerCard : IFlexibleSpeedActionFactory, IRecordLastTarget {

	#region constructor

	PowerCard( MethodBase methodBase, GeneratesContextAttribute targetAttr ) {
		this.methodBase = methodBase;
		this.targetAttr = targetAttr;
		cardAttr = methodBase.GetCustomAttributes<CardAttribute>().VerboseSingle( "Couldn't find CardAttribute on PowerCard targeting a space" );
		speedAttr = methodBase.GetCustomAttribute<SpeedAttribute>(false) ?? throw new InvalidOperationException("Missing Speed attribute for "+methodBase.DeclaringType.Name);
		this.repeatAttr = methodBase.GetCustomAttribute<RepeatAttribute>();
	}

	#endregion

	public string Text => $"{Name} ${Cost} ({DisplaySpeed})";
	public string Name         => cardAttr.Name;
	public Phase DisplaySpeed         => speedAttr.DisplaySpeed;
	public ISpeedBehavior OverrideSpeedBehavior { get; set; }

	public int Cost            => cardAttr.Cost;
	public ElementCounts Elements  => cardAttr.Elements;
	public PowerType PowerType => cardAttr.PowerType;
	public Type MethodType     => methodBase.DeclaringType; // for determining card namespace and Basegame, BranchAndClaw, etc

	public LandOrSpirit LandOrSpirit => targetAttr.LandOrSpirit;

	public bool CouldActivateDuring( Phase requestSpeed, Spirit spirit ){
		return SpeedBehavior.CouldBeActiveFor(requestSpeed,spirit);
	}

	public async Task ActivateAsync( SelfCtx ctx ) {
		// Don't check speed here.  Slow card may have been made fast (Lightning's Swift Strike)

		await ActivateInnerAsync( ctx );
		if(repeatAttr != null) {
			var repeater = repeatAttr.GetRepeater();
			while(await repeater.ShouldRepeat( ctx.Self ))
				await ActivateInnerAsync( ctx );
		}

	}

	async Task ActivateInnerAsync( SelfCtx spiritCtx ) {
		LastTarget = await targetAttr.GetTargetCtx( Name, spiritCtx, TargetingPowerType.PowerCard );
		if(LastTarget != null) // Can't find a tar
			await InvokeOnObjectCtx( LastTarget );
	}

	/// <remarks>Called directly from Let's See What Happens with special ContextBehavior</remarks>
	public Task InvokeOn( TargetSpaceCtx ctx ) {
		return targetAttr.LandOrSpirit != LandOrSpirit.Land
			? throw new InvalidOperationException("Cannot invoke spirit-based PowerCard using TargetSpaceCtx")
			: InvokeOnObjectCtx(ctx);
	}

	Task InvokeOnObjectCtx(object ctx) => (Task)methodBase.Invoke( null, new object[] { ctx } );

	#region private

	ISpeedBehavior SpeedBehavior => OverrideSpeedBehavior ?? speedAttr;

	public object LastTarget { get; private set; }

	readonly SpeedAttribute speedAttr;
	readonly CardAttribute cardAttr;
	readonly MethodBase methodBase;
	readonly RepeatAttribute repeatAttr;

	#endregion

	#region static

	static public PowerCard For<T>() => For(typeof(T));

	static public PowerCard For( Type type ) => For( FindMethod( type ) );

	static MethodInfo FindMethod( Type type ) {
		// try static method (spirit / major / minor)
		return type.GetMethods( BindingFlags.Public | BindingFlags.Static )
			.Where( m => m.GetCustomAttributes<CardAttribute>().Count() == 1 )
			.VerboseSingle( $"PowerCard {type.Name} missing static method with SpiritCard, MinorCard or MajorCard attribute" );
	}

	static public PowerCard For( MethodInfo method ) {
		// check if targets spirit
		var targetSpiritAttribute = method.GetCustomAttributes<AnySpiritAttribute>().FirstOrDefault();
		if( targetSpiritAttribute != null )
			return new PowerCard( method, targetSpiritAttribute );

		//TargetSpaceAttribute targetSpace = (TargetSpaceAttribute)method.GetCustomAttributes<FromPresenceAttribute>().FirstOrDefault()
		//	?? (TargetSpaceAttribute)method.GetCustomAttributes<FromSacredSiteAttribute>().FirstOrDefault();
		TargetSpaceAttribute targetSpace = method.GetCustomAttributes<TargetSpaceAttribute>().FirstOrDefault();

		return new PowerCard( method, targetSpace );
	}

	#endregion

	readonly GeneratesContextAttribute targetAttr;

}
