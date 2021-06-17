namespace SpiritIsland {
	public class Invader : IOption {

		static readonly Invader[] Cities = new Invader[4]; // 0..3
		static readonly Invader[] Towns = new Invader[3];  // 0..2
		static readonly Invader[] Explorers = new Invader[3];  // 0..1

		// Healthy
		static readonly public Invader City      = new Invader("City",Cities,3,0); 
		static readonly public Invader City2     = new Invader("City",Cities,2,1); // damaged
		static readonly public Invader City1     = new Invader("City",Cities,1,2); // damaged
		static readonly public Invader City0     = new Invader("City",Cities,0,12); // DESTROYED
		static readonly public Invader Town      = new Invader("Town",Towns,2,3); 
		static readonly public Invader Town1     = new Invader("Town",Towns,1,4); // damaged
		static readonly public Invader Town0     = new Invader("Town",Towns,0,14); // DESTROYED
		static readonly public Invader Explorer  = new Invader("Explorer",Explorers,1,5);
		static readonly public Invader Explorer0 = new Invader("Explorer",Explorers,0,15);// DESTROYED

		Invader(string label, Invader[] typeArr, int health, int order){
			Label = label;
			this.typeArr = typeArr;
			Health = health;
			typeArr[Health] = this;
			Order = order;
		}

		readonly Invader[] typeArr;

		public string Summary => Initial+"@"+Health; // C@3, T@2

		public Invader Damage(int level){
			if(level>Health) throw new System.ArgumentOutOfRangeException();
			return typeArr[Health-level];
		}

		public int Health {get;}
		public int Order {get;}

		public char Initial => Label[0];

		public string Label { get; }

		string IOption.Text =>  Label; // + health ?
	}

}
