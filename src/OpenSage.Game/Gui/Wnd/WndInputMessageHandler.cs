using System.Collections.Generic;
using System.Linq;
using OpenSage.Gui.Wnd.Controls;
using OpenSage.Input;
using OpenSage.Mathematics;

namespace OpenSage.Gui.Wnd
{
    internal sealed class WndInputMessageHandler : InputMessageHandler
    {
        private readonly WndWindowManager _windowManager;
        private readonly Game _game;

        private readonly List<Control> _lastMouseOverControls = new List<Control>();

        private bool GetControlAtPoint(
            in Point2D mousePosition,
            out Control control,
            out Point2D controlRelativePosition)
        {
            control = _windowManager.GetControlAtPoint(mousePosition);

            if (control != null)
            {
                controlRelativePosition = control.PointToClient(mousePosition);
                return true;
            }

            controlRelativePosition = Point2D.Zero;;
            return false;
        }

        private static void InvokeCallback(
            Control control,
            ControlCallbackContext context,
            WndWindowMessage message)
        {
            control.InputCallback.Invoke(control, message, context);
        }

        private static void InvokeCallback(
            Control control,
            ControlCallbackContext context,
            WndWindowMessageType messageType)
        {
            var message = new WndWindowMessage(messageType, control);
            control.InputCallback.Invoke(control, message, context);
        }

        private static void InvokeCallback(
            Control control,
            ControlCallbackContext context,
            WndWindowMessageType messageType,
            Point2D mousePosition)
        {
            var message = new WndWindowMessage(messageType, control, mousePosition);
            control.InputCallback.Invoke(control, message, context);
        }

        public override HandlingPriority Priority => HandlingPriority.UIPriority;

        public WndInputMessageHandler(WndWindowManager windowManager, Game game)
        {
            _windowManager = windowManager;
            _game = game;
        }
        public override InputMessageResult HandleMessage(InputMessage message)
        {

            var context = new ControlCallbackContext(_windowManager, _game);

            switch (message.MessageType)
            {
                case InputMessageType.MouseMove:
                    {
                        var mouseOverControls = _windowManager.GetControlsAtPoint(message.Value.MousePosition).ToList();
                        foreach (var control in _lastMouseOverControls)
                        {
                            if (!mouseOverControls.Contains(control))
                            {
                                InvokeCallback(control, context, WndWindowMessageType.MouseExit);
                            }
                        }
                        foreach (var control in mouseOverControls)
                        {
                            if (!_lastMouseOverControls.Contains(control))
                            {
                                InvokeCallback(control, context, WndWindowMessageType.MouseEnter);
                            }
                        }

                        _lastMouseOverControls.Clear();
                        _lastMouseOverControls.AddRange(mouseOverControls);

                        foreach (var control in mouseOverControls)
                        {
                            var mousePosition = control.PointToClient(message.Value.MousePosition);
                            InvokeCallback(control, context, WndWindowMessageType.MouseMove, mousePosition);
                        }
                        return mouseOverControls.Count > 0
                            ? InputMessageResult.Handled
                            : InputMessageResult.NotHandled;
                    }

                case InputMessageType.MouseLeftButtonDown:
                    {
                        if (GetControlAtPoint(message.Value.MousePosition, out var element, out var mousePosition))
                        {
                            InvokeCallback(element, context, WndWindowMessageType.MouseDown, mousePosition);
                            return InputMessageResult.Handled;
                        }
                        break;
                    }

                case InputMessageType.MouseLeftButtonUp:
                    {
                        if (GetControlAtPoint(message.Value.MousePosition, out var element, out var mousePosition))
                        {
                            InvokeCallback(element, context, WndWindowMessageType.MouseUp, mousePosition);
                            return InputMessageResult.Handled;
                        }
                        break;
                    }

                case InputMessageType.MouseRightButtonDown:
                    {
                        if (GetControlAtPoint(message.Value.MousePosition, out var element, out var mousePosition))
                        {
                            InvokeCallback(element, context, WndWindowMessageType.MouseRightDown, mousePosition);
                            return InputMessageResult.Handled;
                        }
                        break;
                    }

                case InputMessageType.MouseRightButtonUp:
                    {
                        if (GetControlAtPoint(message.Value.MousePosition, out var element, out var mousePosition))
                        {
                            InvokeCallback(element, context, WndWindowMessageType.MouseRightUp, mousePosition);
                            return InputMessageResult.Handled;
                        }
                        break;
                    }

                // For the time being, just consume middle click events so that they don't go through controls:
                case InputMessageType.MouseMiddleButtonDown:
                case InputMessageType.MouseMiddleButtonUp:
                    {
                        return GetControlAtPoint(message.Value.MousePosition, out var _, out var _)
                            ? InputMessageResult.Handled
                            : InputMessageResult.NotHandled;
                    }

                case InputMessageType.KeyDown:
                    {
                        var control = _windowManager.FocussedControl;
                        if (control != null)
                        {
                            var wndMessage =  new WndWindowMessage(
                                WndWindowMessageType.KeyDown,
                                control,
                                null,
                                message.Value.Key,
                                message.Value.Modifiers);
                            InvokeCallback(control, context, wndMessage);
                            return InputMessageResult.Handled;
                        }


                        break;
                    }
            }

            return InputMessageResult.NotHandled;
        }
    }
}
