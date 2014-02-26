using System;
using RunUO;
using RunUO.Spells.First;
using RunUO.Targeting;

namespace RunUO.Items
{
	public class FeebleWand : BaseWand
	{
		[Constructable]
		public FeebleWand() : base( WandEffect.Feeblemindedness, 5, 30 )
		{
		}

		public FeebleWand( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}

		public override void OnWandUse( Mobile from )
		{
			Cast( new FeeblemindSpell( from, this ) );
		}
	}
}