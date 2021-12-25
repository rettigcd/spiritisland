namespace SpiritIsland {
	public static class SelfCmd {
		static public SelfAction DestoryPresence( ActionType actionType ) => new SelfAction("Destory presence", ctx => ctx.Presence.DestoryOne(actionType));
		static public SelfAction DestoryPresence( int count, ActionType actionType ) => new SelfAction("Destory presence", async ctx => {
				for(int i=0;i<count;++i)
					await ctx.Presence.DestoryOne(actionType);
			}
		);

	}

}
