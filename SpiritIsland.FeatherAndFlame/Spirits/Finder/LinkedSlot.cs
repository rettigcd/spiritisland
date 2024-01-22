namespace SpiritIsland.FeatherAndFlame;

public partial class FinderTrack
{

    public class LinkedSlot {
		public SlotState State { get; set; }

		public void Hide() {
			// Assert: is hidable
			State = SlotState.Hidden_But_Revealable;
			TellNextToRecheckForPreviousRevealed();
			TellPreviousTheyHaveANextHidden();
		}

		public async Task RevealAsync() {
			await Task.Delay(0); // $$$$
			// Assert: is revealable (except for setup)
			State = SlotState.Revealed_But_Hidable;
			TellPreviousToRecheckForNextHidden();
			TellNextTheyHaveAPreviousRevealed();
		}

		#region tell prev/next that you changed

		void TellNextToRecheckForPreviousRevealed() {
			foreach(LinkedSlot item in Next)
				item.RecheckForPreviousRevealed(); // it was hidden!
		}

		void TellPreviousToRecheckForNextHidden() {
			foreach(LinkedSlot item in Previous)
				item.RecheckForNextHiddenSlot(); // it was revealed!
		}
		void TellNextTheyHaveAPreviousRevealed() {
			foreach(LinkedSlot next in Next)
				next.HasRevealedPreviousSlot();
		}

		void TellPreviousTheyHaveANextHidden() {
			foreach(LinkedSlot prev in Previous)
				prev.HasHiddenNextSlot();
		}

		#endregion

		#region change self state

		void RecheckForPreviousRevealed() {
			if( State == SlotState.Hidden_But_Revealable	// was Revealable
				&& !Previous.Any( s => s.State == SlotState.Revealed_But_Hidable ) // but no longer has an upstream revealed slot
			) 
				State = SlotState.Hidden_Not_Revealable;
		}

		void RecheckForNextHiddenSlot() {
			
			if( State == SlotState.Revealed_But_Hidable    // was Hideable
				&& !Next.Any( s => s.State == SlotState.Hidden_But_Revealable )  // no no longer  has any downstream hidden slot
			)
				State = SlotState.Revealed_Not_Hideable;
		}

		void HasHiddenNextSlot() {
			if(State == SlotState.Revealed_Not_Hideable)
				State = SlotState.Revealed_But_Hidable;
		}

		void HasRevealedPreviousSlot() {
			if(State == SlotState.Hidden_Not_Revealable)
				State = SlotState.Hidden_But_Revealable;
		}

		#endregion

		public void TwoWay(params LinkedSlot[] others ) {
			foreach(var other in others) {
				this.FlowsTo( other );
				other.FlowsTo( this );
			}
		}

		public void FlowsTo( LinkedSlot next ) {
			Next.AddLast( next );
			next.Previous.AddLast( this );
		}

		public LinkedList<LinkedSlot> Next { get; set; } = new LinkedList<LinkedSlot>();
		public LinkedList<LinkedSlot> Previous { get; set; } = new LinkedList<LinkedSlot>();
	}

}