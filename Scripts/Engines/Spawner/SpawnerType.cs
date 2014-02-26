using System;
using System.Collections;
using RunUO;

namespace RunUO.Mobiles
{
	public class SpawnerType
	{
		public static Type GetType( string name )
		{
			return ScriptCompiler.FindTypeByName( name );
		}
	}
}