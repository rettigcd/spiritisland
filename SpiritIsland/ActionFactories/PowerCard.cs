using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SpiritIsland {

	public abstract class PowerCard : IActionFactory, IOption {

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
			if(method.GetCustomAttributes<TargetSpiritAttribute>().Any())
				return new PowerCard_TargetSpirit( method );

			//TargetSpaceAttribute targetSpace = (TargetSpaceAttribute)method.GetCustomAttributes<FromPresenceAttribute>().FirstOrDefault()
			//	?? (TargetSpaceAttribute)method.GetCustomAttributes<FromSacredSiteAttribute>().FirstOrDefault();
			TargetSpaceAttribute targetSpace = method.GetCustomAttributes<TargetSpaceAttribute>().FirstOrDefault();

			return new PowerCard_TargetSpace( method, targetSpace );
		}

		#endregion

		protected PowerCard( MethodBase methodBase ) {
			this.methodBase = methodBase;
			cardAttr = methodBase.GetCustomAttributes<CardAttribute>().VerboseSingle( "Couldn't find CardAttribute on PowerCard targeting a space" );
		}

		public string Name { get; protected set; }

		public int Cost { get; protected set;  }

		public Speed Speed => OverrideSpeed != null ? OverrideSpeed.Speed : DefaultSpeed;
		public Speed DefaultSpeed { get; protected set; }
		public SpeedOverride OverrideSpeed { get; set; }

		public Element[] Elements { get; protected set; }
		public PowerType PowerType { get; protected set; }
		public Type MethodType => methodBase.DeclaringType; // for determining card namespace and Basegame, BranchAndClaw, etc

		readonly protected CardAttribute cardAttr;
		readonly protected MethodBase methodBase;

		public void UpdateFromSpiritState( CountDictionary<Element> elements ) {
			cardAttr.UpdateFromSpiritState( elements, this );
		}

//		IActionFactory IActionFactory.Original => this;

		public string Text => Name;

		abstract public Task ActivateAsync( Spirit spirit, GameState gameState );

		#region get cards
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

}