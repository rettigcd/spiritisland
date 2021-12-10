using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SpiritIsland {

	public sealed class PowerCard : IFlexibleSpeedActionFactory, IOption {

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

		public async Task ActivateAsync(SpiritGameStateCtx ctx) {

			if(!await SpeedBehavior.IsActiveFor( ctx.GameState.Phase, ctx.Self ))
				throw new InvalidOperationException( "can't run PowerCard at this speeed" );

			await ActivateInnerAsync( ctx );
			if(repeatAttr != null) {
				var repeater = repeatAttr.GetRepeater();
				while(await repeater.ShouldRepeat( ctx.Self ))
					await ActivateInnerAsync( ctx );
			}

		}

		async Task ActivateInnerAsync( SpiritGameStateCtx spiritCtx ) {
			var targetCtx = await targetAttr.GetTargetCtx( Name, spiritCtx, TargettingFrom.PowerCard );
			if(targetCtx != null) // Can't find a tar
				await InvokeOnObjectCtx( targetCtx );
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

		readonly SpeedAttribute speedAttr;
		readonly CardAttribute cardAttr;
		readonly MethodBase methodBase;
		readonly RepeatAttribute repeatAttr;

		#endregion

		#region static

		static public PowerCard For<T>() => For(typeof(T));

		static PowerCard For( Type type ) => For( FindMethod( type ) );

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

		static public PowerCard[] GetMajors(Type assemblyRefType) {
			static bool HasMajorAttribute( MethodBase m ) => m.GetCustomAttributes<MajorCardAttribute>().Any();
			static bool HasMajorMethod( Type type ) => type.GetMethods().Any( HasMajorAttribute );
			return assemblyRefType.Assembly.GetTypes().Where( HasMajorMethod ).Select( For ).ToArray();
		}
		static public PowerCard[] GetMinors( Type assemblyRefType ) {
			static bool HasMinorAttribute( MethodBase m ) => m.GetCustomAttributes<MinorCardAttribute>().Any();
			static bool HasMinorMethod( Type type ) => type.GetMethods().Any( HasMinorAttribute );
			return assemblyRefType.Assembly.GetTypes().Where( HasMinorMethod ).Select( For ).ToArray();
		}

		#endregion

		readonly GeneratesContextAttribute targetAttr;

	}

}