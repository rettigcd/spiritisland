﻿using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SpiritIsland {

	public abstract class PowerCard : IActionFactory, IOption {

		static public PowerCard For<T>() => For(typeof(T));

		static PowerCard For(Type type ) {
			// try static method (spirit / major / minor)
			var method = type.GetMethods( BindingFlags.Public | BindingFlags.Static )
				.Where( m => m.GetCustomAttributes<BaseCardAttribute>().Count() == 1 )
				.VerboseSingle( $"PowerCard {type.Name} missing static method with SpiritCard, MinorCard or MajorCard attribute" );

			// check if targets spirit
			if(method.GetCustomAttributes<TargetSpiritAttribute>().Any())
				return new TargetSpirit_PowerCard( method );

			TargetSpaceAttribute targetSpace = (TargetSpaceAttribute)method.GetCustomAttributes<FromPresenceAttribute>().FirstOrDefault()
				?? (TargetSpaceAttribute)method.GetCustomAttributes<FromSacredSiteAttribute>().FirstOrDefault();

			return new TargetSpace_PowerCard( method, targetSpace );
		}

		public string Name { get; protected set; }
		public int Cost { get; protected set;  }
		public Speed Speed { get; protected set;  }
		public PowerCard Original => this;
		public Element[] Elements { get; protected set;  }
		public PowerType PowerType { get; protected set;  }
		IActionFactory IActionFactory.Original => this;

		public string Text => Name;

		abstract public Task Activate(ActionEngine engine);


		static public PowerCard[] GetMajors() {
			static bool HasMajorAttribute( MethodBase m ) => m.GetCustomAttributes<MajorCardAttribute>().Any();
			static bool HasMajorMethod( Type type ) => type.GetMethods().Any( HasMajorAttribute );
			return typeof(PowerCard).Assembly.GetTypes().Where( HasMajorMethod ).Select(For).ToArray();
		}
		static public PowerCard[] GetMinors() {
			static bool HasMinorAttribute( MethodBase m ) => m.GetCustomAttributes<MinorCardAttribute>().Any();
			static bool HasMinorMethod( Type type ) => type.GetMethods().Any( HasMinorAttribute );
			return typeof( PowerCard ).Assembly.GetTypes().Where( HasMinorMethod ).Select( For ).ToArray();
		}

	}

}