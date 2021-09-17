using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class InnatePower_TargetSpace : InnatePower, IActionFactory {

		#region Constructors and factories

		public InnatePower_TargetSpace( Type type ) : base( type, LandOrSpirit.Land ) {
			this.targetSpaceAttribute = (TargetSpaceAttribute)type.GetCustomAttributes<TargetSpaceAttribute>().FirstOrDefault()
				?? throw new Exception("missing TargetSpace attribute");
		}

		#endregion

		public override async Task ActivateAsync( Spirit spirit, GameState gameState ) {
			var target = await targetSpaceAttribute.GetTarget( spirit, gameState );
			if(target == null) return;
			MethodInfo x = HighestMethod( spirit );
			var engine = new TargetSpaceCtx( spirit, gameState, target, Cause.Power );
			// !! Make sure we await this
			await (Task)x.Invoke( null, new object[] { engine } ); // Check Innate Powers that target spirits - what is first parameter?
		}

		readonly TargetSpaceAttribute targetSpaceAttribute;

		public override string TargetFilter => this.targetSpaceAttribute.TargetFilter;

		public override string RangeText => this.targetSpaceAttribute.RangeText;
	}

}