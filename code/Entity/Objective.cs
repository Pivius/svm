using Editor;
using System;
using Sandbox;
using System.Text.Json.Serialization;
using Sandbox.ModelEditor.Nodes;
using Sandbox.Utility;
using System.Collections.Generic;
using System.Threading.Tasks;
using SvM.Player;
using SvM.UI;
using Sandbox.UI;

namespace SvM.Ent
{
	public static partial class ObjectiveRPCs
	{
		[ClientRpc]
		public static void Spawn(ObjectiveEntity ent)
		{
			Event.Run( "objective.spawn", ent );
		}

		[ClientRpc]
		public static void Interact( ObjectiveEntity ent )
		{
			Event.Run( "objective.interact", ent );
		}

		[ClientRpc]
		public static void Stop( ObjectiveEntity ent )
		{
			Event.Run( "objective.stop", ent );
		}

		[ClientRpc]
		public static void Finish( ObjectiveEntity ent )
		{
			Event.Run( "objective.finish", ent );
		}
	}

	/// <summary>
	/// Spy vs merc objective entity.
	/// A spy can interact with it and finish an objective, or trigger something.
	/// </summary>
	[Library( "svm_objective" )]
	[HammerEntity, SupportsSolid]
	[Model( Archetypes = ModelArchetype.static_prop_model )]
	[RenderFields, VisGroup( VisGroup.Dynamic )]
	[Title( "Objective" ), Category( "Gameplay" )]
	public partial class ObjectiveEntity : ModelEntity, IUse
	{
		public static List<ObjectiveEntity> AllObjectives { get; set; } = new();
		public enum Flags
		{
			Hack,
			Collect,
			Trigger
		}

		/// <summary>
		/// Settings that are only applicable when the entity spawns
		/// </summary>
		[Property( "objective_type", Title = "Objective type" )]
		public Flags ObjectiveType { get; set; } = Flags.Hack;

		[Property( "interact_sound", Title = "Start Interact Sound" ), FGDType( "sound" ), Category( "Sounds" )]
		public string InteractSound { get; set; } = "";

		[Property( "finish_sound", Title = "End Interact Sound" ), FGDType( "sound" ), Category( "Sounds" )]
		public string FinishSound { get; set; } = "";

		[Net]
		[Property( "interact_time", Title = "Time To Finish" )]
		public float InteractTime { get; set; } = 5f;

		[Property( "reset_on_end", Title = "Reset Timer On End" )]
		public bool ResetOnEnd { get; set; } = false;

		[Net, Predicted]
		public float Timer { get; set; } = 0;
		[Net, Predicted]
		public TimeSince TimeSince { get; set; }
		[Net]
		[Property( "interactable", Title = "Interactable" )]
		public bool Interactable { get; set; } = true;

		[Net]
		public bool Finished { get; set; } = false;

		[Net]
		public Entity IsUsing { get; set; }

		public override void Spawn()
		{
			base.Spawn();

			SetupPhysicsFromModel( PhysicsMotionType.Dynamic );
			PhysicsBody.MotionEnabled = false;
			PhysicsBody.EnableSolidCollisions = false;
			AllObjectives.Add( this );
		}

		public override void ClientSpawn()
		{
			base.ClientSpawn();
			AllObjectives.Add( this );
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
		}

		/// <summary>
		/// Set if the objective should be interactable
		/// </summary>
		[Input]
		void SetInteractable( bool interact )
		{
			Interactable = interact;

			if ( interact )
				ObjectiveRPCs.Spawn( this );
		}

		public virtual bool IsUsable( Entity user ) => Interactable;

		public virtual bool OnUse( Entity user )
		{
			if ( Game.IsServer )
				ObjectiveRPCs.Interact( this );

			if ( user is SpyPlayer player )
			{
				if ( IsUsing != null || Finished )
				{
					return false;
				}

				if ( Interactable )
				{
					IsUsing = user;
					player.SetAnimParameter( "b_hacking", true );
					player.StartInteract( this );
					_ = OnStart.Fire( user );
					TimeSince = Timer;

					return true;
				}
			}
			
			return false;
		}

		public virtual void StopUse()
		{
			if ( IsUsing != null )
			{
				_ = OnEnd.Fire( IsUsing );
				((SpyPlayer)IsUsing).StopInteracting();
				((SpyPlayer)IsUsing).SetAnimParameter( "b_hacking", false );
				IsUsing = null;
				Timer = TimeSince;

				if ( Game.IsServer )
					ObjectiveRPCs.Stop( this );
			}
		}

		public virtual void FinishObjective()
		{
			if ( TimeSince >= InteractTime && IsUsing != null )
			{
				_ = OnFinish.Fire( IsUsing );
				Finished = true;
				StopUse();

				if (Game.IsServer)
					ObjectiveRPCs.Finish( this );
			}
		}


		/// <summary>
		/// Fired when the entity gets damaged, even if it is unbreakable.
		/// </summary>
		protected Output OnDamaged { get; set; }

		public override void TakeDamage( DamageInfo info )
		{
			// The door was damaged, even if its unbreakable, we still want to fire it
			// TODO: Add damage type as argument? Or should it be the new health value?
			_ = OnDamaged.Fire( this );
		}

		protected Output OnFinish { get; set; }

		protected Output OnEnd { get; set; }

		protected Output OnStart { get; set; }

		[Event.Tick.Server]
		public void Think()
		{
			FinishObjective();
		}

		[Event.Tick.Client]
		public void ClientThink()
		{
	
			//ObjectivePanel.ScreenPositionToPanelPosition( Position.ToScreen() );
		}


		/// <summary>
		/// Sounds to be used by ent_door if it does not override sounds.
		/// </summary>
		[Sandbox.ModelEditor.GameData( "objective_sounds" )]
		public class ModelDoorSounds
		{

			[JsonPropertyName( "interact_sound" )]
			public string InteractSound { get; set; }

			[JsonPropertyName( "finish_sound" )]
			public string FinishSound { get; set; }
		}
	}
}
