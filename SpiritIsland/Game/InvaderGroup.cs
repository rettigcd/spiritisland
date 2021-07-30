using System;
using System.Collections.Generic;
using System.Linq;
using SpiritIsland.Core;

namespace SpiritIsland {

	public class InvaderGroup{

		#region constructor

		public InvaderGroup(Space space,Dictionary<Invader,int> dict){
			this.Space = space;
			this.innerDict = dict;
			countDict = new CountDictionary<Invader>(dict);
		}

		#endregion

		public void ApplyDamage( Invader invader, int damage ) {
			if(damage > invader.Health) damage = invader.Health;
			var damagedInvader = invader.Damage(damage);
			changed.Add( invader ); --countDict[invader];
			changed.Add( damagedInvader ); ++countDict[damagedInvader];
		}

		// returns as much damage as possible to invader
		// returns actual damage done
		public int ApplyDamageMax( Invader invader, int availableDamage ) {
			int damageToInvader = Math.Min( invader.Health, availableDamage );

			var damagedInvader = invader.Damage( damageToInvader );
			changed.Add( invader );				--countDict[invader];
			changed.Add( damagedInvader  );		++countDict[damagedInvader];

			return damageToInvader;
		}

		public int this[Invader i] {
			get{ return countDict[i]; }
			set{ 
				changed.Add(i); // so value gets persisted to gamestate
				countDict[i] = value;
			}
		}

		public int Destroy( Invader healthy, int max=1 ) => DestroyInner( healthy, max );
		public int DestroyAll( Invader healthy ) => DestroyInner( healthy, null );

		int DestroyInner( Invader healthy, int? max ) {
			var invadersToDestory = this.InvaderTypesPresent
				.Where(i=>i.Healthy==healthy)
				.OrderByDescending(x=>x.Health) // kill healthiest first
				.ToArray();
			if(max.HasValue)
				invadersToDestory = invadersToDestory.Take(max.Value).ToArray(); 
			int total = 0;
			foreach(var key in invadersToDestory){
				int count = this[key];
				total += count;
				this[key.Dead] += count;
				this[key] = 0;
			}
			return total;
		}

		#region public Read-Only

		public Invader[] FilterBy( params Invader[] allowedTypes ) => allowedTypes
			.SelectMany(x=>x.AliveVariations)
			.Intersect( InvaderTypesPresent )
			.ToArray();

		public Space Space { get; }

		public bool HasCity => countDict.HasAnyKey( Invader.City, Invader.City2, Invader.City1 );
		public bool HasTown => countDict.HasAnyKey( Invader.Town, Invader.Town1 );
		public bool HasExplorer => countDict.HasAnyKey( Invader.Explorer );

		public int DestroyedCities => countDict[Invader.City0];
		public int DestroyedTowns => countDict[Invader.Town0];
		public int DestroyedExplorers => countDict[Invader.Explorer0];

		public IEnumerable<Invader> Changed => changed.Where( i => i.Health != 0 );

		/// <summary> Includes damaged invaders.</summary>
		public IEnumerable<Invader> InvaderTypesPresent => innerDict.Keys.Where( invader => invader.Health > 0 );

		public int TotalCount => innerDict.Values.Sum();

		public int DamageInflictedByInvaders => innerDict.Select(p=>p.Value * p.Key.Healthy.Health).Sum();

		public override string ToString() {
			static int Order_CitiesTownsExplorers( Invader invader )
				=> -(invader.Healthy.Health * 10 + invader.Health);
			return innerDict
				.Where( pair => pair.Value > 0 && pair.Key.Health > 0 )
				.OrderBy( pair => Order_CitiesTownsExplorers( pair.Key ) )
				.Select( p => p.Value + p.Key.Summary )
				.Join( "," );
		}

		#endregion

		#region private
		readonly Dictionary<Invader, int> innerDict;
		readonly CountDictionary<Invader> countDict;
		readonly HashSet<Invader> changed = new HashSet<Invader>();
		#endregion

	}

}
