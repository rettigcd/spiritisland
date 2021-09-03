using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SpiritIsland {

	public abstract class PowerCard : IActionFactory, IOption {

		#region static

		static public PowerCard For<T>() => For(typeof(T));

		static PowerCard For(Type type ) {

			// try static method (spirit / major / minor)
			var method = type.GetMethods( BindingFlags.Public | BindingFlags.Static )
				.Where( m => m.GetCustomAttributes<CardAttribute>().Count() == 1 )
				.VerboseSingle( $"PowerCard {type.Name} missing static method with SpiritCard, MinorCard or MajorCard attribute" );

			// check if targets spirit
			if(method.GetCustomAttributes<TargetSpiritAttribute>().Any())
				return new TargetSpirit_PowerCard( method );

			//TargetSpaceAttribute targetSpace = (TargetSpaceAttribute)method.GetCustomAttributes<FromPresenceAttribute>().FirstOrDefault()
			//	?? (TargetSpaceAttribute)method.GetCustomAttributes<FromSacredSiteAttribute>().FirstOrDefault();
			TargetSpaceAttribute targetSpace = (TargetSpaceAttribute)method.GetCustomAttributes<TargetSpaceAttribute>().FirstOrDefault();

			return new TargetSpace_PowerCard( method, targetSpace );
		}

		#endregion

		public string Name { get; protected set; }
		public int Cost { get; protected set;  }
		public Speed Speed { get; set;  }
		public PowerCard Original => this;
		public Element[] Elements { get; protected set; }
		public PowerType PowerType { get; protected set; }
		public Type MethodType { get; protected set;}

		IActionFactory IActionFactory.Original => this;

		public string Text => Name;

		abstract public Task ActivateAsync( Spirit spirit, GameState gameState );

		#region get cards
		static public PowerCard[] GetMajors() {
			static bool HasMajorAttribute( MethodBase m ) => m.GetCustomAttributes<MajorCardAttribute>().Any();
			static bool HasMajorMethod( Type type ) => type.GetMethods().Any( HasMajorAttribute );
			return typeof( PowerCard ).Assembly.GetTypes().Where( HasMajorMethod ).Select( For ).ToArray();
		}
		static public PowerCard[] GetMinors() {
			static bool HasMinorAttribute( MethodBase m ) => m.GetCustomAttributes<MinorCardAttribute>().Any();
			static bool HasMinorMethod( Type type ) => type.GetMethods().Any( HasMinorAttribute );
			return typeof( PowerCard ).Assembly.GetTypes().Where( HasMinorMethod ).Select( For ).ToArray();
		}

		#endregion

	}

	public enum PowerType { Minor, Major, Spirit }

}