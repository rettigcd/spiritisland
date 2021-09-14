using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SpiritIsland {

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
			innatePowerAttr = actionType.GetCustomAttribute<InnatePowerAttribute>();
			DefaultSpeed = innatePowerAttr.Speed;
			Name = innatePowerAttr.Name;

			// try static method (spirit / major / minor)
			this.elementListByMethod = actionType
				.GetMethods( BindingFlags.Public | BindingFlags.Static )
				.ToDictionary( m => m, m => m.GetCustomAttributes<InnateOptionAttribute>().ToArray() )
				.Where( p => p.Value.Length == 1 )
				.ToDictionary( p => p.Key, p => p.Value[0].Elements );
		}

		#endregion

		readonly InnatePowerAttribute innatePowerAttr;

		readonly Dictionary<MethodInfo, Element[]> elementListByMethod;

		public Speed Speed => OverrideSpeed!=null ? OverrideSpeed.Speed : DefaultSpeed;
		public Speed DefaultSpeed { get; set; }
		public SpeedOverride OverrideSpeed { get; set; }

		public string Name {get;}

		public string Text => Name;

		public IActionFactory Original => this;

		public abstract Task ActivateAsync( Spirit spirit, GameState gameState );

		public Element[][] GetTriggerThresholds() => elementListByMethod.Values.ToArray();

		protected MethodInfo HighestMethod( Spirit spirit ) {
			var activatedElements = spirit.Elements;
			return elementListByMethod
				.OrderByDescending( pair => pair.Value.Length )
				.Where( pair => activatedElements.Contains( pair.Value ) )
				.First().Key;
		}

		public bool IsTriggered { get; private set; }

		public virtual void UpdateFromSpiritState( CountDictionary<Element> elements ) {
//			defaultSpeed = this.innatePowerAttr.

			this.IsTriggered = elementListByMethod
				.OrderByDescending( pair => pair.Value.Length )
				.Any( pair => elements.Contains( pair.Value ) );
		}

	}

	public class InnatePower_TargetSpirit : InnatePower, IActionFactory {

		#region Constructors and factories

		internal InnatePower_TargetSpirit( Type type ):base(type) {}

		#endregion

		public override async Task ActivateAsync( Spirit self, GameState gameState ) {
			Spirit target = await self.Action.Decision( new Decision.TargetSpirit( gameState.Spirits) );
			await TargetSpirit_PowerCard.TargetSpirit( HighestMethod( self ), self, gameState, target );
		}

	}

	public class InnatePower_TargetSpace : InnatePower, IActionFactory {

		#region Constructors and factories

		public InnatePower_TargetSpace( Type type	) : base( type ) {
			this.targetSpaceAttribute = (TargetSpaceAttribute)type.GetCustomAttributes<TargetSpaceAttribute>().FirstOrDefault()
				?? throw new Exception("missing TargetSpace attribute");
		}

		#endregion

		public override async Task ActivateAsync( Spirit spirit, GameState gameState ) {
			var target = await targetSpaceAttribute.GetTarget( spirit, gameState );
			MethodInfo x = HighestMethod( spirit );
			var engine = new TargetSpaceCtx( spirit, gameState, target, Cause.Power );
			// !! Make sure we await this
			await (Task)x.Invoke( null, new object[] { engine } ); // Check Innate Powers that target spirits - what is first parameter?
		}

		readonly TargetSpaceAttribute targetSpaceAttribute;

	}

}