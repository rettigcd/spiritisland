﻿using SpiritIsland.Basegame.Spirits.VitalStrength;
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
			this.elementListByMethod = actionType
				.GetMethods( BindingFlags.Public | BindingFlags.Static )
				.ToDictionary( m => m, m => m.GetCustomAttributes<InnateOptionAttribute>().ToArray() )
				.Where( p => p.Value.Length == 1 )
				.ToDictionary( p => p.Key, p => p.Value[0].Elements );
		}

		#endregion

		readonly Dictionary<MethodInfo, Element[]> elementListByMethod;

		public int PowersActivated(Spirit spirit){
			return elementListByMethod
				.OrderByDescending(pair=>pair.Value.Length)
				.Where(pair=>spirit.Elements.Has(pair.Value))
				.Count();
		}

		public Speed Speed {get;}

		public string Name {get;}

		public string Text => Name;

		public IActionFactory Original => this;

		public abstract void Activate( ActionEngine engine );

		public Element[][] GetTriggerThresholds() => elementListByMethod.Values.ToArray();

        protected MethodInfo HighestMethod( Spirit spirit ) {
			var activatedElements = spirit.Elements;
			return elementListByMethod
                .OrderByDescending( pair => pair.Value.Length )
                .Where( pair => activatedElements.Has( pair.Value ) )
                .First().Key;
        }

    }

	public class InnatePower_TargetSpirit : InnatePower, IActionFactory {

		#region Constructors and factories

		internal InnatePower_TargetSpirit( Type type ):base(type) {}

		#endregion

		public override void Activate( ActionEngine engine ) {
			TargetSpirit_Action.FindSpiritAndInvoke( engine, HighestMethod(engine.Self) );
		}

	}

	internal class InnatePower_TargetSpace : InnatePower, IActionFactory {

		#region Constructors and factories

		internal InnatePower_TargetSpace( Type type	) : base( type ) {
			this.targetSpace = (TargetSpaceAttribute)type.GetCustomAttributes<FromPresenceAttribute>().FirstOrDefault()
				?? (TargetSpaceAttribute)type.GetCustomAttributes<FromSacredSiteAttribute>().FirstOrDefault();
		}

		#endregion

		public override void Activate( ActionEngine engine ) {
			TargetSpace_Action.DoIt( engine, HighestMethod(engine.Self), targetSpace );
		}
		readonly TargetSpaceAttribute targetSpace;

	}



}