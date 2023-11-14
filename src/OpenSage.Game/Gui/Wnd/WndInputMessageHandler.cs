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

        public bool IsMouseOverControl { get; private set; }

        private bool GetControlAtPoint(
            in Point2D mousePosition,
            out Control control,
            out Point2D controlRelativePosition)
        {
            var mouseOverControls = GetControlsAtPoint(mousePosition);
            if (mouseOverControls.Any())
            {
                control = mouseOverControls.First();
                controlRelativePosition = control.PointToClient(mousePosition);
                return true;
            }
            else
            {
                control = null;
                controlRelativePosition = Point2D.Zero;
                return false;
            }
        }

        private List<Control> GetControlsAtPoint(in Point2D mousePosition)
        {
            return _windowManager.GetControlsAtPoint(mousePosition);
        }

        private static Control FindEnabledControl(Control element)
        {
            Control disabled = null, control = element;
            while (control != null)
            {
                if (!control.Enabled)
                {
                    disabled = control;
                }
                control = control.Parent;
            }
            control = disabled == null ? element : disabled.Parent;
            return control != null && control.NoInput ? null : control;
        }

        private static bool InvokeCallback(
            Control element, bool checkEnabled,
            ControlCallbackContext context,
            WndWindowMessage message)
        {
            Control control = element;
            if (checkEnabled)
            {
                control = FindEnabledControl(element);
            }
            while (control != null)
            {
                if (control.InputCallback.Invoke(control, message, context))
                {
                    return true;
                }
                control = control.Parent;
            }
            return false;
        }

        private static bool InvokeCallback(
            Control element, bool checkEnabled,
            ControlCallbackContext context,
            WndWindowMessageType messageType)
        {
            var message = new WndWindowMessage(messageType, element);
            return InvokeCallback(element, checkEnabled, context, message);
        }

        private static bool InvokeCallback(
            Control element, bool checkEnabled,
            ControlCallbackContext context,
            WndWindowMessageType messageType,
            Point2D mousePosition)
        {
            var message = new WndWindowMessage(messageType, element, mousePosition);
            return InvokeCallback(element, checkEnabled, context, message);
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
            var handled = false;

            switch (message.MessageType)
            {
                case InputMessageType.MouseMove:
                    {
                        var mouseOverControls = GetControlsAtPoint(message.Value.MousePosition);
                        IsMouseOverControl = false;
                        /* 
                         * We ignore ENABLED and NOINPUT flags and InvokeCallback's result 
                         * for the MouseExit and MouseEnter events
                         */
                        foreach (var control in _lastMouseOverControls)
                        {
                            if (!mouseOverControls.Contains(control))
                            {
                                InvokeCallback(control, false, context, WndWindowMessageType.MouseExit);
                            }
                        }
                        foreach (var control in mouseOverControls)
                        {
                            if (!_lastMouseOverControls.Contains(control))
                            {
                                var wndMessage = new WndWindowMessage(WndWindowMessageType.MouseEnter, control);
                                InvokeCallback(control, false, context, wndMessage);
                            }
                        }
                        _lastMouseOverControls.Clear();
                        _lastMouseOverControls.AddRange(mouseOverControls);

                        if (mouseOverControls.Count > 0 && FindEnabledControl(mouseOverControls.First()) != null)
                        {
                            foreach (var control in mouseOverControls)
                            {
                                var mousePosition = control.PointToClient(message.Value.MousePosition);
                                var wndMessage = new WndWindowMessage(WndWindowMessageType.MouseMove, control, mousePosition);
                                InvokeCallback(control, false, context, wndMessage);
                            }
                            IsMouseOverControl = true;
                            handled = true;
                        }
                        break;
                    }

                case InputMessageType.MouseLeftButtonDown:
                    {
                        if (GetControlAtPoint(message.Value.MousePosition, out var element, out var mousePosition))
                        {
                            handled = InvokeCallback(element, true, context, WndWindowMessageType.MouseDown, mousePosition);
                        }
                        break;
                    }

                case InputMessageType.MouseLeftButtonUp:
                    {
                        if (GetControlAtPoint(message.Value.MousePosition, out var element, out var mousePosition))
                        {
                            handled = InvokeCallback(element, true, context, WndWindowMessageType.MouseUp, mousePosition);
                        }
                        break;
                    }

                case InputMessageType.MouseRightButtonDown:
                    {
                        if (GetControlAtPoint(message.Value.MousePosition, out var element, out var mousePosition))
                        {
                            handled = InvokeCallback(element, true, context, WndWindowMessageType.MouseRightDown, mousePosition);
                        }
                        break;
                    }

                case InputMessageType.MouseRightButtonUp:
                    {
                        if (GetControlAtPoint(message.Value.MousePosition, out var element, out var mousePosition))
                        {
                            handled = InvokeCallback(element, true, context, WndWindowMessageType.MouseRightUp, mousePosition);
                        }
                        break;
                    }

                // For the time being, just consume middle click events so that they don't go through controls:
                case InputMessageType.MouseMiddleButtonDown:
                case InputMessageType.MouseMiddleButtonUp:
                    {
                        if (GetControlAtPoint(message.Value.MousePosition, out var _, out var _))
                        {
                            handled = true;
                        }
                        break;
                    }

                case InputMessageType.KeyDown:
                    {
                        var control = _windowManager.FocussedControl;
                        if (control != null && control.Enabled && !control.NoInput)
                        {
                            var wndMessage = new WndWindowMessage(
                                WndWindowMessageType.KeyDown,
                                control,
                                null,
                                message.Value.Key,
                                message.Value.Modifiers);
                            handled = InvokeCallback(control, true, context, wndMessage);
                        }
                        break;
                    }
            }

            return handled ? InputMessageResult.Handled : InputMessageResult.NotHandled;
        }
    }
}
