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

		public virtual bool UpdateAndISActivatedBy(CountDictionary<Element> elements){
			return elementListByMethod
				.OrderByDescending(pair=>pair.Value.Length)
				.Any(pair=>elements.Contains(pair.Value));
		}

		public Speed Speed {get;set;}

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

	}

	public class InnatePower_TargetSpirit : InnatePower, IActionFactory {

		#region Constructors and factories

		internal InnatePower_TargetSpirit( Type type ):base(type) {}

		#endregion

		public override async Task ActivateAsync( Spirit self, GameState gameState ) {
			Spirit target = await self.SelectSpirit(gameState.Spirits);
			await TargetSpirit_PowerCard.TargetSpirit( HighestMethod( self ), self, gameState, target );
		}

	}

	internal class InnatePower_TargetSpace : InnatePower, IActionFactory {

		#region Constructors and factories

		internal InnatePower_TargetSpace( Type type	) : base( type ) {
			this.targetSpace = (TargetSpaceAttribute)type.GetCustomAttributes<FromPresenceAttribute>().FirstOrDefault()
				?? (TargetSpaceAttribute)type.GetCustomAttributes<FromSacredSiteAttribute>().FirstOrDefault();
		}

		#endregion

		public override async Task ActivateAsync( Spirit spirit, GameState gameState ) {
			var target = await targetSpace.GetTarget( spirit.MakeDecisionsFor(gameState) );
			MethodInfo x = HighestMethod( spirit );
			var engine = new TargetSpaceCtx( spirit, gameState, target );
			// !! Make sure we await this
			await (Task)x.Invoke( null, new object[] { engine } ); // Check Innate Powers that target spirits - what is first parameter?
		}

		readonly TargetSpaceAttribute targetSpace;

	}



}