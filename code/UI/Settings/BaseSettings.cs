using System;
using Sandbox;
using System.Collections.Generic;
using Sandbox.UI;

namespace SvM.UI.Settings
{
	public class BaseSettings : Panel
	{
		public string Name { get; set; }
		public string ConVar { get; set; }
		public string Value { get; set; }
		public Panel Panel { get; set; }

		public BaseSettings( string conVar, string name )
		{
			Panel = new();
			Name = name;
			ConVar = conVar;
			Value = ConsoleSystem.GetValue( conVar );
		}

		public virtual void SetValue( string value )
		{
			Value = value;
			ConsoleSystem.SetValue( ConVar, value );
		}
	}
}
