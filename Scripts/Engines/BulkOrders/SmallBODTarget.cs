using System;
using RunUO;
using RunUO.Targeting;
using RunUO.Network;

namespace RunUO.Engines.BulkOrders
{
	public class SmallBODTarget : Target
	{
		private SmallBOD m_Deed;

		public SmallBODTarget( SmallBOD deed ) : base( 18, false, TargetFlags.None )
		{
			m_Deed = deed;
		}

		protected override void OnTarget( Mobile from, object targeted )
		{
			if ( m_Deed.Deleted || !m_Deed.IsChildOf( from.Backpack ) )
				return;

			m_Deed.EndCombine( from, targeted );
		}
	}
}