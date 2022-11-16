using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using GTA.Native;
using LemonUI;
using LemonUI.Menus;

namespace BrandNewTrainer
{
    public class Menu : Script
    {
        #region Variables

        private static readonly ObjectPool pool = new ObjectPool();
        private static readonly NativeMenu mainMenu = new NativeMenu(ModSettings.modName);

        private static readonly NativeMenu cheatsSubmenu = new NativeMenu(ModSettings.modName, "Cheats");
        private static readonly NativeItem cheatsClearWantedItem = new NativeItem("Clear Wanted");
        private static readonly NativeCheckboxItem cheatsNeverWantedItem = new NativeCheckboxItem("Never Wanted", false);
        private static readonly NativeCheckboxItem cheatsGodModeItem = new NativeCheckboxItem("God Mode", false);

        private static readonly NativeMenu weatherAndTimeSubmenu = new NativeMenu(ModSettings.modName, "Time & Weather");
        private static readonly NativeListItem<string> timeListItem = new NativeListItem<string>("Time", "Set the current time", "Morning", "Afternoon", "Evening", "Night");
        private static readonly NativeListItem<string> weatherListItem = new NativeListItem<string>("Weather", "Set the current weather", "Clear", "Clearing", "Neutral", "Extra Sunny", "Clouds", "Raining", "Foggy", "Smog", "Blizzard", "Thunder Storm", "Snowing", "Snowlight", "Christmas", "Halloween", "Overcast");

        private static readonly NativeMenu debugSubmenu = new NativeMenu(ModSettings.modName, "Debug");
        private static readonly NativeItem debugGetPosItem = new NativeItem("Get Position", "Get current position of the player");

        private bool isVisible = false;

        private bool isNeverWanted = false;

        #endregion

        #region SHVDN Methods
        public Menu()
        {
            pool.Add(mainMenu);

            InitAllSubmenus();

            Tick += OnTick;
            KeyUp += OnKeyUp;
        }

        private void OnTick(object sender, EventArgs e)
        {
            pool.Process();

            if (isNeverWanted)
            {
                if (Game.Player.WantedLevel > 0) Game.Player.WantedLevel = 0;
            }
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F6)
            {
                if (!pool.AreAnyVisible)
                {
                    mainMenu.Visible = true;
                }
            }
        }

        #endregion

        #region Init Submenus
        private void InitCheatsSubmenu()
        {
            pool.Add(cheatsSubmenu);

            mainMenu.AddSubMenu(cheatsSubmenu);

            cheatsSubmenu.Add(cheatsClearWantedItem);
            cheatsSubmenu.Add(cheatsNeverWantedItem);
            cheatsSubmenu.Add(cheatsGodModeItem);

            cheatsClearWantedItem.Activated += OnClearWanted;
            cheatsNeverWantedItem.Activated += OnNeverWanted;
            cheatsGodModeItem.Activated += OnGodMode;
        }

        private void InitTeleportSubmenu()
        {
            NativeMenu submenu = new NativeMenu(ModSettings.modName, "Teleportation");
            NativeItem waypointItem = new NativeItem("Waypoint", "Teleport the player to the current waypoint");

            NativeMenu exteriorsSubmenu = new NativeMenu(ModSettings.modName, "Exteriors", "Teleport the player to a specific location");
            NativeMenu interiorsSubmenu = new NativeMenu(ModSettings.modName, "Interiors", "Teleport the player to a specific interior");

            pool.Add(submenu);
            pool.Add(exteriorsSubmenu);
            pool.Add(interiorsSubmenu);

            mainMenu.AddSubMenu(submenu);
            submenu.Add(waypointItem);

            submenu.AddSubMenu(exteriorsSubmenu);
            submenu.AddSubMenu(interiorsSubmenu);

            waypointItem.Activated += OnTeleportToWaypoint;

            Dictionary<string, Vector3> exteriorsDict = new Dictionary<string, Vector3>()
            {
                {"Strip Club", new Vector3(131.80f, -1303.347f, 29.22822f)},
                {"Impoud Garage", new Vector3(434.80f, -1016.346f, 28f) }
            };

            foreach (KeyValuePair<string, Vector3> location in exteriorsDict)
            {
                NativeItem item = new NativeItem(location.Key);
                exteriorsSubmenu.Add(item);

                item.Activated += (sender, args) =>
                {
                    Game.Player.Character.Position = location.Value;
                };
            }

            Dictionary<string, Vector3> interiorsDict = new Dictionary<string, Vector3>()
            {
                {"Michael's House", new Vector3(-814.5281f, 178.1695f, 76.74077f)},
                {"Franklin's House", new Vector3(2.39f, 525.015f, 170.6172f)},
                {"Trevor's House", new Vector3(1969.68f, 3815.052f, 33.42872f)},
            };

            foreach (KeyValuePair<string, Vector3> location in interiorsDict)
            {
                NativeItem item = new NativeItem(location.Key);
                interiorsSubmenu.Add(item);

                item.Activated += (sender, args) =>
                {
                    Game.Player.Character.Position = location.Value;
                };
            }

        }

        private void InitWeatherAndTimeSubmenu()
        {
            pool.Add(weatherAndTimeSubmenu);

            mainMenu.AddSubMenu(weatherAndTimeSubmenu);

            weatherAndTimeSubmenu.Add(timeListItem);
            weatherAndTimeSubmenu.Add(weatherListItem);

            timeListItem.Activated += OnTime;
            weatherListItem.Activated += OnWeather;

        }

        private void InitDebugSubmenu()
        {
            pool.Add(debugSubmenu);

            mainMenu.AddSubMenu(debugSubmenu);
            debugSubmenu.Add(debugGetPosItem);

            debugGetPosItem.Activated += OnDebugGetPos;
        }

        private void InitAllSubmenus()
        {
            InitCheatsSubmenu();
            InitTeleportSubmenu();
            InitWeatherAndTimeSubmenu();
            InitDebugSubmenu();
        }

        #endregion

        #region Items Methods
        private void OnClearWanted(object sender, EventArgs e)
        {
            Game.Player.WantedLevel = 0;
        }

        private void OnNeverWanted(object sender, EventArgs e)
        {
            isNeverWanted = cheatsNeverWantedItem.Checked;
        }

        private void OnGodMode(object sender, EventArgs e)
        {
            Game.Player.Character.IsInvincible = cheatsGodModeItem.Checked;
        }

        private void OnTeleportToWaypoint(object sender, EventArgs e)
        {
            if (Game.IsWaypointActive)
            {
                Vector3 markerPos = World.WaypointPosition;
                float groundZ = World.GetGroundHeight(markerPos);
                markerPos = new Vector3(markerPos.X, markerPos.Y, groundZ);

                if (markerPos.Z == 0 || markerPos.Z == 1)
                {
                    markerPos = World.GetNextPositionOnStreet(markerPos);
                }

                Game.Player.Character.Position = markerPos;

                GTA.UI.Notification.Show("You has been ~b~teleported ~w~at the coords : \n~q~X : " + markerPos.X + "\n~p~Y : " + markerPos.Y + "\n~b~Z : " + markerPos.Z);

            }
            else
            {
                GTA.UI.Notification.Show("~r~You should add a ~b~waypoint ~r~first!");
            }
        }

        private void OnTime(object sender, EventArgs e)
        {
            switch (timeListItem.SelectedItem)
            {
                case "Morning":
                    Function.Call(Hash.SET_CLOCK_TIME, 06, 00, 00);
                    break;
                case "Afternoon":
                    Function.Call(Hash.SET_CLOCK_TIME, 12, 00, 00);
                    break;
                case "Evening":
                    Function.Call(Hash.SET_CLOCK_TIME, 18, 00, 00);
                    break;
                case "Night":
                    Function.Call(Hash.SET_CLOCK_TIME, 00, 00, 00);
                    break;
            }

            GTA.UI.Notification.Show("Time set to ~b~" + timeListItem.SelectedItem);
        }

        private void OnWeather(object sender, EventArgs e)
        {
            switch (weatherListItem.SelectedItem)
            {
                case "Clear":
                    World.Weather = Weather.Clear;
                    break;
                case "Clearing":
                    World.Weather = Weather.Clearing;
                    break;
                case "Neutral":
                    World.Weather = Weather.Neutral;
                    break;
                case "Extra Sunny":
                    World.Weather = Weather.ExtraSunny;
                    break;
                case "Clouds":
                    World.Weather = Weather.Clouds;
                    break;
                case "Raining":
                    World.Weather = Weather.Raining;
                    break;
                case "Foggy":
                    World.Weather = Weather.Foggy;
                    break;
                case "Smog":
                    World.Weather = Weather.Smog;
                    break;
                case "Blizzard":
                    World.Weather = Weather.Blizzard;
                    break;
                case "Thunder Storm":
                    World.Weather = Weather.ThunderStorm;
                    break;
                case "Snowing":
                    World.Weather = Weather.Snowing;
                    break;
                case "Snowlight":
                    World.Weather = Weather.Snowlight;
                    break;
                case "Christmas":
                    World.Weather = Weather.Christmas;
                    break;
                case "Halloween":
                    World.Weather = Weather.Halloween;
                    break;
                case "Overcast":
                    World.Weather = Weather.Overcast;
                    break;
            }

            GTA.UI.Notification.Show("Weather set to ~b~" + weatherListItem.SelectedItem);
        }

        private void OnDebugGetPos(object sender, EventArgs e)
        {
            Vector3 pos = Game.Player.Character.Position;
            GTA.UI.Notification.Show("~q~X: " + pos.X + "\n~p~Y: " + pos.Y + "\n~b~Z: " + pos.Z);
        }

        #endregion
    }
}
