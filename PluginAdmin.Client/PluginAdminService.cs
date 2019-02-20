using JetBrains.Annotations;
using NFive.SDK.Client.Commands;
using NFive.SDK.Client.Events;
using NFive.SDK.Client.Interface;
using NFive.SDK.Client.Rpc;
using NFive.SDK.Client.Services;
using NFive.SDK.Core.Diagnostics;
using NFive.SDK.Core.Models.Player;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using System;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BattleRatttt.PluginAdmin.Client
{
	[PublicAPI]
	public class PluginAdminService : Service
	{

		public PluginAdminService(ILogger logger, ITickManager ticks, IEventManager events, IRpcHandler rpc, ICommandManager commands, OverlayManager overlay, User user) : base(logger, ticks, events, rpc, commands, overlay, user)
		{
			this.Commands.Register("car", new Action<string>(Car));
			this.Commands.Register("dv", new Action(Dv));
			this.Commands.Register("noclip", new Action(ToggleNoclip));
		}

		private async void Car(string model)
		{
			// create the vehicle
			Vehicle vehicle = await World.CreateVehicle(new Model(model), Game.PlayerPed.Position, Game.PlayerPed.Heading);

			// set the player ped into the vehicle and driver seat
			Game.PlayerPed.SetIntoVehicle(vehicle, VehicleSeat.Driver);
		}

		private void Dv()
		{
			// delete the vehicle that the ped is in
			Vehicle v = Game.Player.Character.CurrentVehicle;
			v.Delete();
		}

		private bool NoclipEnabled = false;
		private Vector3 NoclipPosition;
		private float NoclipRotation;
		private Vector3 MoveForwardOffset = new Vector3(0f, -1f, -1f);
		private Vector3 MoveBackwardOffset = new Vector3(0f, 1f, -1f);
		private Vector3 MoveUpOffset = new Vector3(0f, 0f, 1f);
		private Vector3 MoveDownOffset = new Vector3(0f, 0f, -2f);
		private float TurnOffset = 1f;

		private void ToggleNoclip()
		{
			NoclipEnabled = !NoclipEnabled;
			if (NoclipEnabled)
			{
				NoclipPosition = Game.Player.Character.Position;
				NoclipRotation = Game.Player.Character.Heading;
				this.Ticks.Attach(NoclipTick);
			}
			else
			{
				this.Ticks.Detach(NoclipTick);
			}
		}

		private async Task NoclipTick()
		{
			Game.Player.Character.Position = NoclipPosition;
			Game.Player.Character.Heading = NoclipRotation;

			if (Game.IsControlPressed(1, Control.MoveUpOnly))
			{
				NoclipPosition = Game.Player.Character.GetOffsetPosition(MoveForwardOffset);
			}
			else if (Game.IsControlPressed(1, Control.MoveDownOnly))
			{
				NoclipPosition = Game.Player.Character.GetOffsetPosition(MoveBackwardOffset);
			}

			if (Game.IsControlPressed(1, Control.MoveLeftOnly))
			{
				NoclipRotation = NoclipRotation + TurnOffset > 360f ? 0f : NoclipRotation + TurnOffset;
			}
			else if (Game.IsControlPressed(1, Control.MoveRightOnly))
			{
				NoclipRotation = NoclipRotation - TurnOffset < 0f ? 360f : NoclipRotation - TurnOffset;
			}

			if (Game.IsControlPressed(1, Control.Cover))
			{
				NoclipPosition = Game.Player.Character.GetOffsetPosition(MoveUpOffset);
			}
			else if (Game.IsControlPressed(1, Control.Pickup))
			{
				NoclipPosition = Game.Player.Character.GetOffsetPosition(MoveDownOffset);
			}
		}
	}
}
