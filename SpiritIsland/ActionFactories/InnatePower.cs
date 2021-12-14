using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class InnatePower : IFlexibleSpeedActionFactory, IRecordLastTarget {

		#region Constructors and factories

		static public InnatePower For<T>(){ 
			Type actionType = typeof(T);
			var contextAttr = actionType.GetCustomAttributes<GeneratesContextAttribute>().VerboseSingle(actionType.Name+" must have Single Target space or Target spirit attribute");
			return new InnatePower( actionType, contextAttr );
		}

		internal InnatePower(Type actionType,GeneratesContextAttribute targetAttr){

			innatePowerAttr = actionType.GetCustomAttribute<InnatePowerAttribute>();
			speedAttr = actionType.GetCustomAttribute<SpeedAttribute>(false) 
				?? throw new InvalidOperationException("Missing Speed attribute for "+actionType.Name);
			this.targetAttr = targetAttr;
			this.repeatAttr = actionType.GetCustomAttribute<RepeatAttribute>();

			Name = innatePowerAttr.Name;

			// try static method (spirit / major / minor)
			this.elementListByMethod = actionType
				.GetMethods( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static )
				.Select( m => new MethodTuple(m) )
				.Where( x => x.Attr != null )
				.ToList();

			this._drawableOptions = elementListByMethod
				.Select(x=>x.Attr)
				.Where( o => o.Purpose != AttributePurpose.ExecuteOnly )
				.Cast<IDrawableInnateOption>()
				.ToList();
			if(this.repeatAttr!=null)
				_drawableOptions.AddRange( repeatAttr.Thresholds );
		}

		#endregion

		#region Speed

		public Phase DisplaySpeed => speedAttr.DisplaySpeed;
		/// <summary> When set, overrides the speed attribute for everything except Display Speed </summary>
		public ISpeedBehavior OverrideSpeedBehavior { get; set; }

		public bool CouldActivateDuring( Phase phase, Spirit spirit ) {	// !!! Can we do this somehow without asking them every time if they want to use an element?
			return CouldBeTriggered( spirit )
				&& CouldMatchPhase( phase, spirit );
		}

		bool CouldBeTriggered( Spirit spirit ) {
			return elementListByMethod
				.Any(x=>spirit.CouldHaveElements(x.Elements));
		}

		bool CouldMatchPhase( Phase requestSpeed, Spirit spirit ) {
			return SpeedBehavior.CouldBeActiveFor( requestSpeed, spirit );
		}

		ISpeedBehavior SpeedBehavior => OverrideSpeedBehavior ?? speedAttr;

		#endregion

		public string Name {get;}

		public string Text => Name;

		public string TargetFilter => this.targetAttr.TargetFilter;

		public string RangeText => this.targetAttr.RangeText;

		public LandOrSpirit LandOrSpirit => targetAttr.LandOrSpirit;

		public async Task ActivateAsync( SpiritGameStateCtx ctx ) {
			if(!await SpeedBehavior.IsActiveFor( ctx.GameState.Phase, ctx.Self )) {
				LastTarget = null;
				return; // this is here for Shifting Memory to Activate Innates
			}

			await ActivateInnerAsync( ctx );
			if( repeatAttr != null) {
				var repeater = repeatAttr.GetRepeater();
				while( await repeater.ShouldRepeat(ctx.Self) )
					await ActivateInnerAsync( ctx );
			}
		}

		public ElementCounts[] GetTriggerThresholds() => elementListByMethod.Select(a=>a.Attr.Elements).ToArray();

		public IEnumerable<InnateOptionAttribute> Options => elementListByMethod.Select(x=>x.Attr);

		public IEnumerable<IDrawableInnateOption> DrawableOptions => _drawableOptions;

		readonly List<IDrawableInnateOption> _drawableOptions;

		async Task ActivateInnerAsync( SpiritGameStateCtx spiritCtx ) {

			List<MethodInfo> lastMethods = await GetLastActivatedMethodsOfEachGroup( spiritCtx );
			if( lastMethods.Count == 0 ) {
				LastTarget = null;
				return;
			}

			LastTarget = await targetAttr.GetTargetCtx( Name, spiritCtx, TargettingFrom.Innate );
			if(LastTarget == null) return;

			var objList = new object[] { LastTarget };
			foreach(var method in lastMethods)
				await (Task)method.Invoke( null, objList );

		}

		async Task<List<MethodInfo>> GetLastActivatedMethodsOfEachGroup( SpiritGameStateCtx spiritCtx ) {
			IEnumerable<MethodTuple[]> groups = elementListByMethod
				// filter first - so we only have groups that have matches
				.Where( pair => pair.Attr.Purpose != AttributePurpose.DisplayOnly )
				.GroupBy( x => x.Group )
				.Select( x => x.ToArray() );
			List<MethodInfo> lastMethods = new List<MethodInfo>();
			foreach(MethodTuple[] grp in groups) {
				MethodInfo method = await GetLastMethodThatHasElements( spiritCtx.Self, grp );
				if(method != null)
					lastMethods.Add( method );
			}
			return lastMethods;
		}

		static async Task<MethodInfo> GetLastMethodThatHasElements( Spirit self, MethodTuple[] grp ) {
			ElementCounts match = await self.GetHighestMatchingElements( grp.Select(g=>g.Elements) );
			return grp.FirstOrDefault(g=>g.Elements==match)?.Method;
		}

		public static string[] Tokenize( string s ) {

			var tokens = new Regex( "sacred site|presence|fast|slow"
				+ "|dahan|blight|fear|city|town|explorer"
				+ "|sun|moon|air|fire|water|plant|animal|earth"
				+ "|beast|disease|strife|wilds|badlands"
				+ "|\\+1range" 
			).Matches( s ).Cast<Match>().ToList();

			var results = new List<string>();

			int cur = 0;
			while(cur < s.Length) {
				// no more tokens, go to the end
				if(tokens.Count == 0) {
					results.Add( s[cur..] );
					break;
				}
				var nextToken = tokens[0];
				if(nextToken.Index == cur) {
					results.Add( "{"+nextToken.Value+"}" );
					cur = nextToken.Index + nextToken.Length;
					tokens.RemoveAt( 0 );
				} else {
					results.Add( s[cur..nextToken.Index] );
					cur = nextToken.Index;
				}
			}
			return results.ToArray();
		}

		public object LastTarget { get; private set; }

		readonly InnatePowerAttribute innatePowerAttr;
		readonly protected SpeedAttribute speedAttr;
		readonly GeneratesContextAttribute targetAttr;
		readonly RepeatAttribute repeatAttr;
		readonly List<MethodTuple> elementListByMethod;
		readonly List<object> targets;


		class MethodTuple {
			public MethodTuple(MethodInfo m ) {
				Method = m;
				Attr = m.GetCustomAttributes<InnateOptionAttribute>().FirstOrDefault();
			}
			public MethodInfo Method { get; }
			public InnateOptionAttribute Attr { get; }
			public ElementCounts  Elements => Attr.Elements;
			public int Group => Attr.Group;
		}

	}

	public enum LandOrSpirit { None, Land, Spirit }

}