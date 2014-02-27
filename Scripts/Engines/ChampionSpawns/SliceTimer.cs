using System;
using System.Collections;
using RunUO;
using RunUO.Items;

namespace RunUO.Engines.ChampionSpawns
{
	public class SliceTimer : Timer
	{
		private ChampionSpawn m_Spawn;

		public SliceTimer( ChampionSpawn spawn ) : base( TimeSpan.FromSeconds( 1.0 ),  TimeSpan.FromSeconds( 1.0 ) )
		{
			m_Spawn = spawn;
			Priority = TimerPriority.OneSecond;
		}

		protected override void OnTick()
		{
			m_Spawn.OnSlice();
		}
	}
}