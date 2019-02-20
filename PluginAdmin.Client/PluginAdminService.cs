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
using BattleRatttt.PluginAdmin.Client.Overlays;

namespace BattleRatttt.PluginAdmin.Client
{
	[PublicAPI]
	public class PluginAdminService : Service
	{
		private bool enabled;

		public PluginAdminService(ILogger logger, ITickManager ticks, IEventManager events, IRpcHandler rpc, ICommandManager commands, OverlayManager overlay, User user) : base(logger, ticks, events, rpc, commands, overlay, user)
		{
			this.Commands.Register("car", new Action<string>(Car));
			this.Commands.Register("dv", new Action(Dv));
			this.Commands.Register("noclip", new Action(NoClip));
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

		private Vector3 NoclipPosition = new Vector3();
		private int Heading;

		private void NoClip()
		{
			this.enabled = !this.enabled;
			Screen.ShowNotification($"NoClip is now {(this.enabled ? "~g~enabled" : "~r~disabled")}~s~.");

			if (this.enabled)
			{
				NoclipPosition = Game.Player.Character.Position;
				this.Ticks.Attach(Tick);
			}
			else
			{
				NoclipPosition = Game.Player.Character.Position;
				this.Ticks.Detach(Tick);
			}
		}

		private async Task Tick()
		{
			if (this.enabled)
			{
				Game.Player.Character.Position = NoclipPosition;

				if (Game.IsControlPressed(1, Control.Phone))
				{
					NoclipPosition = Game.Player.Character.GetOffsetPosition(new Vector3(0f, 0f, 1f));
				}

				if (Game.IsControlPressed(1, Control.PhoneDown))
				{
					NoclipPosition = Game.Player.Character.GetOffsetPosition(new Vector3(0f, 0f, -1f));
				}

				if (Game.IsControlPressed(1, Control.FlyUpDown))
				{
					NoclipPosition = Game.Player.Character.GetOffsetPosition(new Vector3(0f, 1f, 0f));
				}

				if (Game.IsControlPressed(1, Control.MoveUpOnly))
				{
					NoclipPosition = Game.Player.Character.GetOffsetPosition(new Vector3(0f, -1f, 0f));
				}

				if (Game.IsControlPressed(1, Control.MoveLeftOnly))
				{
					Heading += 1;
					if (Heading > 360)
					{
						Heading = 0;
					}
					Game.Player.Character.Heading = Heading;
				}

				if (Game.IsControlPressed(1, Control.FlyLeftRight))
				{
					Heading -= 1;
					if (Heading < 0)
					{
						Heading = 360;
					}
					Game.Player.Character.Heading = Heading;
				}

				await Task.FromResult(0);
			}
			else
			{
				this.Ticks.Detach(Tick);
			}
		}
	}
}
