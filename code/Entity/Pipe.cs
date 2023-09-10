using Editor;
using System;
using Sandbox;
using System.Text.Json.Serialization;
using Sandbox.ModelEditor.Nodes;
using Sandbox.Utility;
using System.Collections.Generic;
using System.Threading.Tasks;
using SvM.Player;

namespace SvM.Ent
{
	/// <summary>
	/// Spy vs merc objective entity.
	/// A spy can interact with it and finish an objective, or trigger something.
	/// </summary>
	[Library( "svm_pipe" )]
	[HammerEntity, SupportsSolid]
	[Model( Archetypes = ModelArchetype.static_prop_model )]
	[RenderFields, VisGroup( VisGroup.Dynamic )]
	[Title( "Pipe" ), Category( "Gameplay" )]
	public partial class PipeEntity : ModelEntity
	{
		public enum Flags
		{
			Vertical,
			Horizontal
		}

		[Property( "direction", Title = "Directional Climb" )]
		public Flags Direction { get; set; } = Flags.Vertical;

		public override void Spawn()
		{
			base.Spawn();

			SetupPhysicsFromModel( PhysicsMotionType.Dynamic );
			PhysicsBody.MotionEnabled = false;
			PhysicsBody.EnableSolidCollisions = false;
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
		}
	}
}
