using System;
using System.Collections.Generic;
using OpenSage.Input;

namespace OpenSage.Gui.ControlBar
{
    public class ControlBarInputHandler : InputMessageHandler
    {
        private readonly Game _game;
        private readonly Dictionary<Veldrid.Key, CommandButton> _hotkeys;
        public override HandlingPriority Priority => HandlingPriority.UIPriority;

        public ControlBarInputHandler(Game game)
        {
            _hotkeys = new Dictionary<Veldrid.Key, CommandButton>();
            _game = game;
        }

        public override InputMessageResult HandleMessage(InputMessage message)
        {
            if (message.MessageType == InputMessageType.KeyUp)
            {
                if (message.Value.Modifiers != Veldrid.ModifierKeys.None)
                {
                    return InputMessageResult.NotHandled;
                }
                if (_hotkeys.TryGetValue(message.Value.Key, out var button))
                {
                    if (button.Enabled)
                    {
                        CommandButtonCallback.HandleCommand(_game, button, button.Object?.Value);
                        _game.PlayClick();
                    }
                    return InputMessageResult.Handled;
                }
            }
            return InputMessageResult.NotHandled;
        }

        public void Clear()
        {
            _hotkeys.Clear();
        }

        public void RegisterHotkey(Veldrid.Key key, CommandButton button)
        {
            _hotkeys[key] = button;
        }
    }
}