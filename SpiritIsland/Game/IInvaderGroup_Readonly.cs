//using System.Collections.Generic;

//namespace SpiritIsland {
//	public interface IInvaderGroup_Readonly {
//		int this[Invader generic] { get; }
//		int this[InvaderSpecific specific] { get; }

//		int DamageInflictedByInvaders { get; }
//		bool HasCity { get; }
//		bool HasExplorer { get; }
//		bool HasTown { get; }
//		IEnumerable<Invader> InvaderTypesPresent_Generic { get; }
//		IEnumerable<InvaderSpecific> InvaderTypesPresent_Specific { get; }
//		Space Space { get; }
//		int TotalCount { get; }
//		int TownsAndCitiesCount { get; }

//		InvaderSpecific[] FilterBy( params Invader[] healthyTypes );
//		bool Has( Invader inv );
//		bool HasAny( params Invader[] healthyInvaders );
//		InvaderSpecific PickBestInvaderToRemove( params Invader[] removable );
//		string ToString();
//	}
//}