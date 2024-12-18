#nullable enable
namespace SpiritIsland;

public sealed class PowerCard : IPowerActionFactory {

	#region static Factories

	static public PowerCard For(Type type) {
		MethodInfo method = type.GetMethods(BindingFlags.Public | BindingFlags.Static)
			.Where(m => m.GetCustomAttributes<CardAttribute>().Count() == 1)
			.VerboseSingle($"PowerCard {type.Name} missing static method with SpiritCard, MinorCard or MajorCard attribute");
		return ForDecorated(method);
	}

	static public PowerCard ForDecorated(Func<TargetSpaceCtx,Task> asyncAction){ return ForDecorated(asyncAction.Method); }
	static public PowerCard ForDecorated(Func<TargetSpiritCtx,Task> asyncAction){ return ForDecorated(asyncAction.Method); }
	static public PowerCard ForDecorated(Func<Spirit, Task> asyncAction) { return ForDecorated(asyncAction.Method); }


	static PowerCard ForDecorated(MethodInfo method) {
		var name = method.DeclaringType!.Name;
		var ctxFactory = method.GetCustomAttributes<GeneratesContextAttribute>().First();
		if( ctxFactory is TargetSpaceAttribute tsa )
			tsa.Preselect = method.GetCustomAttribute<PreselectAttribute>();
		var cardDetails = method.GetCustomAttributes<CardAttribute>().VerboseSingle("Couldn't find CardAttribute on PowerCard targeting a space");
		SpeedAttribute speed = method.GetCustomAttribute<SpeedAttribute>(false) ?? throw new InvalidOperationException("Missing Speed attribute for " + name);
		var repeater    = method.GetCustomAttribute<RepeatAttribute>();
		var instructions = method.GetCustomAttribute<InstructionsAttribute>(false)?.Text ?? throw new InvalidOperationException("Missing Instructions attribute for " + name);
		var artist = method.GetCustomAttribute<ArtistAttribute>(false)?.Artist ?? throw new InvalidOperationException("Missing Artist attribute for " + name);
		return new PowerCard( method, ctxFactory, cardDetails, speed, repeater, instructions, artist );
	}

	#endregion static Factories

	#region constructor

	PowerCard( 
		MethodBase methodBase, 
		ITargetCtxFactory targetCtxFactory,  // Range, Filter, Land/Spirit
		IDisplayCardDetails cardDetails,     // Name, Cost, Elements, Card/Innate
		IDisplaySpeedBehaviour speed,        // Speed (and speed behavior)
		IHaveARepeater? repeater,
		string instructions,
		string artist
	) {
		_methodBase = methodBase;
		_targetCtxFactory = targetCtxFactory;
		_cardDetails = cardDetails;
		_speed = speed;
		_repeatHolder = repeater;
		Instructions = instructions;
		Artist = artist;
	}

	#endregion

	string IOption.Text   => $"{Title} ${Cost} ({Speed})";
	public string Title   => _cardDetails.Name;
	public Phase Speed    => _speed.DisplaySpeed;

	// These are only used for drawing the cards.
	public string Instructions { get; }
	public string Artist { get; }
	Task InvokeOnObjectCtx(object ctx) => (Task)_methodBase.Invoke(null, [ctx])!;

	public string TargetFilter => _targetCtxFactory.TargetFilterName;
	/// <summary> Used by PowerCardImageManager to draw the range-text on the card. </summary>
	public string RangeText => _targetCtxFactory.RangeText;

	public int Cost            => _cardDetails.Cost;
	public CountDictionary<Element> Elements  => _cardDetails.Elements;
	public PowerType PowerType => _cardDetails.PowerType;

	public LandOrSpirit LandOrSpirit => _targetCtxFactory.LandOrSpirit;

	public bool CouldActivateDuring( Phase requestSpeed, Spirit spirit )
		=> _speed.CouldBeActiveFor(requestSpeed,spirit);

	public async Task ActivateAsync( Spirit self ) {
		// Don't check speed here.  Slow card may have been made fast (Lightning's Swift Strike)

		await ActivateInnerAsync( self );
		if(_repeatHolder != null) {
			var repeater = _repeatHolder.GetRepeater();
			while(await repeater.ShouldRepeat( self )) {
				await using var anotherScope = await ActionScope.StartSpiritAction(ActionCategory.Spirit_Power,self);
				await ActivateInnerAsync( self );
			}
		}

	}


	async Task ActivateInnerAsync( Spirit self ) {
		object? target = await _targetCtxFactory.GetTargetCtx(Title, self);
		if( target is not null )
			await InvokeOnObjectCtx(target);
	}

	/// <remarks>Called directly from Let's See What Happens with special ContextBehavior</remarks>
	public Task InvokeOn( TargetSpaceCtx ctx ) {
		return _targetCtxFactory.LandOrSpirit == LandOrSpirit.Land
			? InvokeOnObjectCtx( ctx )
			: throw new InvalidOperationException( "Cannot invoke spirit-based PowerCard using TargetSpaceCtx" );
	}

	#region private

	readonly IDisplaySpeedBehaviour _speed;
	readonly IDisplayCardDetails _cardDetails;
	readonly MethodBase _methodBase;
	readonly IHaveARepeater? _repeatHolder;

	#endregion

	readonly ITargetCtxFactory _targetCtxFactory;

}
