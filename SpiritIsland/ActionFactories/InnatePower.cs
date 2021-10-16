using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class InnatePower : IFlexibleSpeedActionFactory {

		readonly GeneratesContextAttribute targetAttr;
		readonly RepeatIfAttribute repeatAttr;

		public string TargetFilter => this.targetAttr.TargetFilter;

		public string RangeText => this.targetAttr.RangeText;

		#region Constructors and factories

		static public InnatePower For<T>(){ 
			Type actionType = typeof(T);
			var contextAttr = actionType.GetCustomAttributes<GeneratesContextAttribute>().Single();
			return new InnatePower( actionType, contextAttr );
		}

		internal InnatePower(Type actionType,GeneratesContextAttribute targetAttr){

			innatePowerAttr = actionType.GetCustomAttribute<InnatePowerAttribute>();
			speedAttr = actionType.GetCustomAttribute<SpeedAttribute>(false) 
				?? throw new InvalidOperationException("Missing Speed attribute for "+actionType.Name);
			this.targetAttr = targetAttr;
			this.repeatAttr = actionType.GetCustomAttribute<RepeatIfAttribute>();

			Name = innatePowerAttr.Name;

			// try static method (spirit / major / minor)
			this.elementListByMethod = actionType
				.GetMethods( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static )
				.Select( m => new MethodTuple(m) )
				.Where( x => x.Attr != null )
				.ToList();
		}

		#endregion

		readonly InnatePowerAttribute innatePowerAttr;
		readonly protected SpeedAttribute speedAttr;

		readonly List<MethodTuple> elementListByMethod;
		class MethodTuple {
			public MethodTuple(MethodInfo m ) {
				Method = m;
				Attr = m.GetCustomAttributes<InnateOptionAttribute>().FirstOrDefault();
			}
			public MethodInfo Method { get; }
			public InnateOptionAttribute Attr { get; }
			public Element[] Elements => Attr.Elements;
			public int Group => Attr.Group;
		}

		#region Speed

		public Speed Speed => speedAttr.DisplaySpeed;
		public SpeedOverride OverrideSpeed { get; set; }

		public virtual bool IsActiveDuring( Speed requestSpeed, CountDictionary<Element> elements ){
			this.IsTriggered = elementListByMethod
				.OrderByDescending( x => x.Elements.Length )
				.Any( x => elements.Contains( x.Elements ) );
			return IsTriggered && 
				(OverrideSpeed != null
					? OverrideSpeed.Speed.IsOneOf( requestSpeed, Speed.FastOrSlow)
					: speedAttr.IsActiveFor(requestSpeed,elements)
				);
		}

		#endregion

		public string Name {get;}

		public string Text => Name;

		public LandOrSpirit LandOrSpirit => targetAttr.LandOrSpirit;

		bool ShouldRepeat(CountDictionary<Element> elements) => repeatAttr != null && repeatAttr.Repeat( elements );

		public async Task ActivateAsync( Spirit self, GameState gameState ) {
			await ActivateInnerAsync(self,gameState);
			if( ShouldRepeat(self.Elements) )
				await ActivateInnerAsync(self,gameState);
		}

		async Task ActivateInnerAsync( Spirit self, GameState gameState ) {
			var ctx = await targetAttr.GetTargetCtx( self, gameState );
			if(ctx == null) return;
			var methods = HighestMethodOfEachGroup( self );
			foreach(var method in methods)
				await (Task)method.Invoke( null, new object[] { ctx } );
		}

		public Element[][] GetTriggerThresholds() => elementListByMethod.Select(a=>a.Attr.Elements).ToArray();

		protected MethodInfo[] HighestMethodOfEachGroup( Spirit spirit ) {
			var activatedElements = spirit.Elements;
			var bestMatch = elementListByMethod
				// filter first - so we only have groups that have matches
				.Where( pair => activatedElements.Contains( pair.Elements ) && pair.Attr.Purpose != AttributePurpose.DisplayOnly )
				.GroupBy(x=>x.Group)
				// from each group, select method with most elements
				.Select( grp => grp.OrderByDescending( pair => pair.Elements.Length ).First().Method )
				.ToArray();
			return bestMatch;
		}

		protected bool IsTriggered;

		public IEnumerable<InnateOptionAttribute> Options => elementListByMethod.Select(x=>x.Attr);

		static public string[] Tokenize( string s ) {

			var tokens = new Regex( "sacred site|presence|fast|slow"
				+ "|dahan|blight|fear|city|town|explorer"
				+ "|sun|moon|air|fire|water|plant|animal|earth"
				+ "|beast|disease|strife|wilds"
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

	}

	public enum LandOrSpirit { None, Land, Spirit }

}