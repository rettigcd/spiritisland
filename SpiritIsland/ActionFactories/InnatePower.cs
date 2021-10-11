﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SpiritIsland {

	public abstract class InnatePower : IFlexibleSpeedActionFactory {

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
			speedAttr = actionType.GetCustomAttribute<SpeedAttribute>(false) 
				?? throw new InvalidOperationException("Missing Speed attribute for "+actionType.Name);

			Speed = speedAttr.Speed;
			Name = innatePowerAttr.Name;
			LandOrSpirit = landOrSpirit;

			// try static method (spirit / major / minor)
			this.elementListByMethod = actionType
				.GetMethods( BindingFlags.Public | BindingFlags.Static )
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

		Speed EffectiveSpeed => OverrideSpeed!=null ? OverrideSpeed.Speed : Speed;
		public Speed Speed { get; set; }
		public SpeedOverride OverrideSpeed { get; set; }

		public bool IsActiveDuring( Speed speed, CountDictionary<Element> _ ) => IsTriggered
			&& (speed == EffectiveSpeed || EffectiveSpeed == Speed.FastOrSlow);
		public bool IsInactiveAfter( Speed speed ) => speed == EffectiveSpeed || speed == Speed.Slow;

		#endregion

		public string Name {get;}

		public string Text => Name;

		public LandOrSpirit LandOrSpirit { get; }

		public abstract Task ActivateAsync( Spirit spirit, GameState gameState );

		public abstract string RangeText { get; }

		public abstract string TargetFilter { get; }

		public Element[][] GetTriggerThresholds() => elementListByMethod.Select(a=>a.Attr.Elements).ToArray();

		protected MethodInfo[] HighestMethod( Spirit spirit ) {
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

		bool IsTriggered;

		public virtual void UpdateFromSpiritState( CountDictionary<Element> elements ) {
			this.IsTriggered = elementListByMethod
				.OrderByDescending( x => x.Elements.Length )
				.Any( x => elements.Contains( x.Elements ) );
		}

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