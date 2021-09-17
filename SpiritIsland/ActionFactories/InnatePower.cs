using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
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

		internal InnatePower(Type actionType,LandOrSpirit landOrSpirit){
			innatePowerAttr = actionType.GetCustomAttribute<InnatePowerAttribute>();
			DefaultSpeed = innatePowerAttr.Speed;
			Name = innatePowerAttr.Name;
			LandOrSpirit = landOrSpirit;

			// try static method (spirit / major / minor)
			this.elementListByMethod = actionType
				.GetMethods( BindingFlags.Public | BindingFlags.Static )
				.ToDictionary( m => m, m => m.GetCustomAttributes<InnateOptionAttribute>().FirstOrDefault() )
				.Where( p => p.Value != null )
				.ToDictionary( p => p.Key, p => p.Value );
		}

		#endregion

		readonly InnatePowerAttribute innatePowerAttr;

		readonly Dictionary<MethodInfo, InnateOptionAttribute> elementListByMethod;

		#region Speed

		public Speed Speed => OverrideSpeed!=null ? OverrideSpeed.Speed : DefaultSpeed;
		public Speed DefaultSpeed { get; set; }
		public SpeedOverride OverrideSpeed { get; set; }

		#endregion

		public string Name {get;}

		public string Text => Name;

		public LandOrSpirit LandOrSpirit { get; }

		public abstract Task ActivateAsync( Spirit spirit, GameState gameState );

		public abstract string RangeText { get; }

		public abstract string TargetFilter { get; }

		public Element[][] GetTriggerThresholds() => elementListByMethod.Values.Select(a=>a.Elements).ToArray();

		protected MethodInfo HighestMethod( Spirit spirit ) {
			var activatedElements = spirit.Elements;
			return elementListByMethod
				.Where( pair => activatedElements.Contains( pair.Value.Elements ) && pair.Value.Purpose != AttributePurpose.DisplayOnly )
				.OrderByDescending( pair => pair.Value.Elements.Length )
				.First().Key;
		}

		public bool IsTriggered { get; private set; }

		public virtual void UpdateFromSpiritState( CountDictionary<Element> elements ) {
			this.IsTriggered = elementListByMethod
				.OrderByDescending( pair => pair.Value.Elements.Length )
				.Any( pair => elements.Contains( pair.Value.Elements ) );
		}

		public IEnumerable<InnateOptionAttribute> Options => elementListByMethod.Values;

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