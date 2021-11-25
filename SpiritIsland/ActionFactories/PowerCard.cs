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

		public async Task ActivateAsync(SpiritGameStateCtx spiritCtx) {

			if( !await SpeedBehavior.IsActiveFor(spiritCtx.GameState.Phase,spiritCtx.Self) )
				return; 

			var targetCtx = await targetAttr.GetTargetCtx( Name, spiritCtx, TargettingFrom.PowerCard );
			if(targetCtx == null) 
				return;

			await InvokeOnObjectCtx(targetCtx);

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

	// Volcano targets differently for Innates vs cards
	public enum TargettingFrom { None, Innate, PowerCard } // Can't think up a good name for this

	public class PowerType : IOption {
		public static readonly PowerType Minor  = new PowerType("minor");
		public static readonly PowerType Major  = new PowerType("major");
		public static readonly PowerType Spirit = new PowerType("spirit");
		public static readonly PowerType Innate = new PowerType("innate");

		public string Text { get; }

		PowerType(string text) { Text = text; }
	}

	static public class PowerCardExtensions_ForWinForms {

		public static string GetImageFilename( this PowerCard card ) {
			string filename = card.Name
				.Replace( ',', '_' )
				.Replace( ' ', '_' )
				.Replace( "__", "_" )
				.Replace( "'", "" )
				.Replace( "-", "" )
				.ToLower();
			string cardType = card.PowerType.Text;
			string ns = card.MethodType.Namespace;
			string edition = ns.Contains( "Basegame" ) ? "basegame"
				: ns.Contains( "BranchAndClaw" ) ? "bac"
				: ns.Contains( "PromoPack1" ) ? "bac"  // !!! temporary
				: ns.Contains( "JaggedEarth" ) ? "je"
				: ns;
			return $".\\images\\{edition}\\{cardType}\\{filename}.jpg";
		}

	}

}