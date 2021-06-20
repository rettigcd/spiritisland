namespace SpiritIsland.PowerCards {
	class NullOption : IOption {
		static public NullOption Value = new NullOption();
		static public IOption[] SingleOption = new IOption[]{Value};
		NullOption(){ } // hide constructor
		public string Text => "-null-";
	}

}



