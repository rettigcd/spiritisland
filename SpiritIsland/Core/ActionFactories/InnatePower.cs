﻿using SpiritIsland.Base.Spirits.VitalStrength;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SpiritIsland.Core {

	public abstract class InnatePower : IActionFactory {

		#region Constructors and factories

		static public InnatePower For<T>(){ 
			Type actionType = typeof(T);

			bool targetSpirit = actionType.GetCustomAttributes<TargetSpiritAttribute>().Any();
            return targetSpirit		
				? new InnatePower_TargetSpirit( actionType ) 
				: new InnatePower_TargetSpace( actionType );
        }

        internal InnatePower(Type actionType){
			InnatePowerAttribute innatePowerAttr = actionType.GetCustomAttribute<InnatePowerAttribute>();
			Speed = innatePowerAttr.Speed;
			Name = innatePowerAttr.Name;

			// try static method (spirit / major / minor)
			this.methods = actionType
				.GetMethods( BindingFlags.Public | BindingFlags.Static )
				.ToDictionary( m => m, m => m.GetCustomAttributes<InnateOptionAttribute>().ToArray() )
				.Where( p => p.Value.Length == 1 )
				.ToDictionary( p => p.Key, p => p.Value[0].Elements );
		}

		#endregion

		readonly Dictionary<MethodInfo, Element[]> methods;

		public int PowersActivated(Spirit spirit){
			return methods
				.OrderByDescending(pair=>pair.Value.Length)
				.Where(pair=>spirit.HasElements(pair.Value))
				.Count();
		}

		public Speed Speed {get;}

		public string Name {get;}

		public string Text => Name;

		public IActionFactory Original => this;

		public abstract IAction Bind( Spirit spirit, GameState gameState );

        protected MethodInfo HighestMethod( Spirit spirit ) {
            return methods
                .OrderByDescending( pair => pair.Value.Length )
                .Where( pair => spirit.HasElements( pair.Value ) )
                .First().Key;
        }

    }

	public class InnatePower_TargetSpirit : InnatePower, IActionFactory {

		#region Constructors and factories

		internal InnatePower_TargetSpirit( Type type ):base(type) {}

		#endregion

		public override IAction Bind( Spirit spirit, GameState gameState ) {
			return new TargetSpirit_Action( spirit, gameState, HighestMethod(spirit) );
		}

	}

	internal class InnatePower_TargetSpace : InnatePower, IActionFactory {

		#region Constructors and factories

		internal InnatePower_TargetSpace( Type type	) : base( type ) {
			this.targetSpace = (TargetSpaceAttribute)type.GetCustomAttributes<FromPresenceAttribute>().FirstOrDefault()
				?? (TargetSpaceAttribute)type.GetCustomAttributes<FromSacredSiteAttribute>().FirstOrDefault();
		}

		#endregion

		public override IAction Bind( Spirit spirit, GameState gameState ) {
			return new TargetSpace_Action( spirit, gameState, HighestMethod(spirit), targetSpace );
		}
		readonly TargetSpaceAttribute targetSpace;

	}



}