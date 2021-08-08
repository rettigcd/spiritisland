using System;
using System.Collections.Generic;
using System.Linq;
using SpiritIsland.Core;

namespace SpiritIsland {

	public class InvaderGroup{

		readonly static Dictionary<Invader,int> idx;

		static InvaderGroup() {
			idx = new Dictionary<Invader, int>();
			int i=0;
			idx.Add( Invader.Explorer0,i++);
			idx.Add( Invader.Explorer, i++ );
			idx.Add( Invader.Town0, i++ );
			idx.Add( Invader.Town1, i++ );
			idx.Add( Invader.Town, i++ );
			idx.Add( Invader.City0, i++ );
			idx.Add( Invader.City1, i++ );
			idx.Add( Invader.City2, i++ );
			idx.Add( Invader.City, i++ );
		}

		#region constructor

		public InvaderGroup(Space space, int[] counts, Action<int> addFear ) {

			this.counts = counts; // for committing

			this.Space = space;
			this.innerDict = Invader.Lookup.Values
				.ToDictionary( invader => invader, invader => counts[idx[invader]] );

			countDict = new CountDictionary<Invader>(innerDict);
			this.addFear = addFear;
		}
		readonly int[] counts;


		#endregion

		Invader[] KillOrder => killOrder ??= "C@1 C@2 C@3 T@1 T@2 E@1".Split( ' ' ).Select( k => Invader.Lookup[k] ).ToArray();
		Invader[] killOrder;

		Invader[] LeftOverOrder => leftOverOrder ??= "C@2 T@2 C@3".Split( ' ' ).Select( k => Invader.Lookup[k] ).ToArray();
		Invader[] leftOverOrder;

		public void ApplyDamageToInvaders( int startingDamage, List<string> log = null ) {
			int damageToInvaders = startingDamage;

			// While damage remains    &&    we have invaders
			while(damageToInvaders > 0 && InvaderTypesPresent.Any()) {
				var invaderToDamage = KillOrder
					.FirstOrDefault( invader =>
						invader.Health <= damageToInvaders // prefer things we can kill
						&& this[invader] > 0
					)
					?? LeftOverOrder.First( invader => this[invader] > 0 ); // left-over damage

				damageToInvaders -= this.ApplyDamage( invaderToDamage, damageToInvaders );

			}
			if(log != null) log.Add( $"{startingDamage} damage to invaders leaving {this}." );
		}

		readonly Action<int> addFear;

		public int ApplyDamage( Invader invader, int availableDamage ) {
			int damageToInvader = Math.Min( invader.Health, availableDamage );

			var damagedInvader = invader.Damage( damageToInvader );
			--countDict[invader]; 
			--counts[idx[invader]];

			++countDict[damagedInvader]; 
			++counts[idx[damagedInvader]];

			if(damagedInvader == Invader.City0)
				addFear( 2 );
			if(damagedInvader == Invader.Town0)
				addFear( 1 );

			return damageToInvader;
		}


		public int DestroyType( Invader healthy, int max=1 ) => (max == 0) ? 0 : DestroyInner( healthy, max );

		public int DestroyTypeAll( Invader healthy ) => DestroyInner( healthy, int.MaxValue );

		int DestroyInner( Invader healthy, int countToDestory ) {
			Invader[] invaderTypesToDestory = this.InvaderTypesPresent
				.Where(i=>i.Healthy==healthy && i.Health!=0)
				.OrderByDescending(x=>x.Health) // kill healthiest first
				.ToArray();

			int totalDestoyed = 0;
			foreach(var invaderType in invaderTypesToDestory) {
				while(countToDestory>0 && countDict[invaderType] > 0) {
					ApplyDamage( invaderType, invaderType.Health );
					++totalDestoyed;
					--countToDestory;
				}
			}
			return totalDestoyed;
		}

		#region public Read-Only

		public int this[Invader i] => countDict[i];

		public Invader[] FilterBy( params Invader[] allowedTypes ) => allowedTypes
			.SelectMany(x=>x.AliveVariations)
			.Intersect( InvaderTypesPresent )
			.ToArray();

		public Space Space { get; }

		public bool HasCity => countDict.HasAnyKey( Invader.City, Invader.City2, Invader.City1 );
		public bool HasTown => countDict.HasAnyKey( Invader.Town, Invader.Town1 );
		public bool HasExplorer => countDict.HasAnyKey( Invader.Explorer );
		public bool Has(params Invader[] healthyInvaders) => countDict.Keys.Select(k=>k.Healthy).Intersect( healthyInvaders ).Any();

		/// <summary> Includes damaged invaders.</summary>
		public IEnumerable<Invader> InvaderTypesPresent => countDict.Keys.Where( invader => invader.Health > 0 );

		public int TypeCount(params Invader[] healthyInvaders) => innerDict
			.Where(pair=>healthyInvaders.Contains(pair.Key.Healthy))
			.Sum(p=>p.Value);

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

		readonly static Invader[] damagedInvaders = new[] { Invader.City1, Invader.City2, Invader.Town1 };

		static public void Heal(int[] counts ) {
			foreach(var damaged in damagedInvaders) {
				counts[idx[damaged.Healthy]] += counts[idx[damaged]];
				counts[idx[damaged]] = 0;
			}
		}

		static public void Adjust(int[] counts, Invader invader, int delta ) {
			int index = idx[invader];
			counts[index] = Math.Max( 0, counts[index] + delta );
		}

		#endregion

		#region private
		readonly Dictionary<Invader, int> innerDict;
		readonly CountDictionary<Invader> countDict;
		#endregion

	}

}
