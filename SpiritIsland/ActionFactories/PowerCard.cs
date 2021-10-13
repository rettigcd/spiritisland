using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SpiritIsland {

	public abstract class PowerCard : IFlexibleSpeedActionFactory, IOption {

		#region constructor

		protected PowerCard( MethodBase methodBase ) {
			this.methodBase = methodBase;
			cardAttr = methodBase.GetCustomAttributes<CardAttribute>().VerboseSingle( "Couldn't find CardAttribute on PowerCard targeting a space" );
			speedAttr = methodBase.GetCustomAttribute<SpeedAttribute>(false) ?? throw new InvalidOperationException("Missing Speed attribute for "+methodBase.DeclaringType.Name);
		}

		#endregion

		public string Text => Name;

		public string Name         => cardAttr.Name;
		public Speed Speed         => speedAttr.DisplaySpeed;
		public SpeedOverride OverrideSpeed { get; set; }

		public int Cost            => cardAttr.Cost;
		public Element[] Elements  => cardAttr.Elements;
		public PowerType PowerType => cardAttr.PowerType;
		public Type MethodType     => methodBase.DeclaringType; // for determining card namespace and Basegame, BranchAndClaw, etc

		public bool IsActiveDuring( Speed requestSpeed, CountDictionary<Element> elements ){
			return OverrideSpeed != null
				? OverrideSpeed.Speed.IsOneOf( requestSpeed, Speed.FastOrSlow)
				: speedAttr.IsActiveFor(requestSpeed,elements);
		}

		abstract public Task ActivateAsync( Spirit spirit, GameState gameState );

		#region private

		readonly protected SpeedAttribute speedAttr;
		readonly protected CardAttribute cardAttr;
		readonly protected MethodBase methodBase;

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
			if(method.GetCustomAttributes<AnySpiritAttribute>().Any())
				return new PowerCard_TargetSpirit( method );

			//TargetSpaceAttribute targetSpace = (TargetSpaceAttribute)method.GetCustomAttributes<FromPresenceAttribute>().FirstOrDefault()
			//	?? (TargetSpaceAttribute)method.GetCustomAttributes<FromSacredSiteAttribute>().FirstOrDefault();
			TargetSpaceAttribute targetSpace = method.GetCustomAttributes<TargetSpaceAttribute>().FirstOrDefault();

			return new PowerCard_TargetSpace( method, targetSpace );
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

	}

	public enum PowerType { Minor, Major, Spirit }

	static public class PowerCardExtensions_ForWinForms {

		public static string GetImageFilename( this PowerCard card ) {
			string filename = card.Name
				.Replace( ',', '_' )
				.Replace( ' ', '_' )
				.Replace( "__", "_" )
				.Replace( "'", "" )
				.Replace( "-", "" )
				.ToLower();
			string cardType = card.PowerType switch {
				PowerType.Minor => "minor",
				PowerType.Major => "major",
				PowerType.Spirit => "spirit",
				_ => throw new Exception()
			};
			string ns = card.MethodType.Namespace;
			string edition = ns.Contains( "Basegame" ) ? "basegame"
				: ns.Contains( "BranchAndClaw" ) ? "bac"
				: ns.Contains( "PromoPack1" ) ? "bac"  // !!! temporary
				: ns;
			return $".\\images\\{edition}\\{cardType}\\{filename}.jpg";
		}

	}

}