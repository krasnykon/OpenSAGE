﻿using System.IO;
using System.Linq;
using OpenSage.Gui;
using OpenSage.Gui.Wnd;
using OpenSage.Gui.Wnd.Controls;
using OpenSage.Logic.Orders;

namespace OpenSage.Mods.Generals.Gui
{
    [WndCallbacks]
    public static class ControlBarCallbacks
    {
        private const string GeneralsExpPointsWnd = "GeneralsExpPoints.wnd";

        public static void W3DCommandBarBackgroundDraw(Control control, DrawingContext2D drawingContext)
        {

        }

        public static void ControlBarSystem(Control control, WndWindowMessage message, ControlCallbackContext context)
        {
            switch (message.MessageType)
            {
                case WndWindowMessageType.SelectedButton:
                    switch (message.Element.Name)
                    {
                        case "ControlBar.wnd:ButtonLarge":
                            ((GeneralsControlBar) context.Game.Scene2D.ControlBar).ToggleSize();
                            break;
                        case "ControlBar.wnd:ButtonOptions":
                            context.WindowManager.PushWindow(Path.Combine("Menus", "QuitMenu.wnd"));
                            break;
                        case "ControlBar.wnd:ButtonGeneral":
                            if (context.WindowManager.TopWindow?.Name == GeneralsExpPointsWnd) {
                                GeneralsExpPointsCallbacks.SetWindow(null);
                                context.WindowManager.PopWindow();
                            } else {
                                var window = context.WindowManager.PushWindow(GeneralsExpPointsWnd);
                                window.Name = GeneralsExpPointsWnd;
                                GeneralsExpPointsCallbacks.SetWindow(window);
                            }

                            break;
                    }
                    break;
            }
        }

        public static bool LeftHUDInput(Control control, WndWindowMessage message, ControlCallbackContext context)
        {
            if (message.MessageType != WndWindowMessageType.MouseDown
                && message.MessageType != WndWindowMessageType.MouseRightDown)
            {
                return false;
            }

            var terrainPosition = context.Game.Scene3D.Radar.RadarToWorldSpace(
                message.MousePosition,
                control.ClientRectangle);

            switch (message.MessageType)
            {
                case WndWindowMessageType.MouseDown: // Left mouse moves selected units
                    var unit = context.Game.Scene3D.LocalPlayer.SelectedUnits.LastOrDefault();
                    if (unit != null)
                    {
                        // TODO: Duplicated in OrderGeneratorSystem
                        unit.OnLocalMove(context.Game.Audio);
                        var order = Order.CreateMoveOrder(context.Game.Scene3D.GetPlayerIndex(context.Game.Scene3D.LocalPlayer), terrainPosition);
                        context.Game.NetworkMessageBuffer.AddLocalOrder(order);
                    }
                    return true;

                case WndWindowMessageType.MouseRightDown: // Right mouse moves camera.
                    context.Game.Scene3D.CameraController.TerrainPosition = terrainPosition;
                    return true;
            }
            
            return false;
        }

        public static void W3DLeftHUDDraw(Control control, DrawingContext2D drawingContext)
        {
            control.Window.Game.Scene3D.Radar.Draw(
                drawingContext,
                control.ClientRectangle);
        }
    }
}
