using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenSage.Data.Wnd;
using OpenSage.Gui.Wnd.Controls;
using OpenSage.Gui.Wnd.Transitions;
using OpenSage.Mathematics;

namespace OpenSage.Gui.Wnd
{
    public sealed class WndWindowManager
    {
        private readonly IPanel _panel;
        private readonly IWndWindowLoader _loader;
        private readonly WndInputMessageHandler _messageHandler;

        public int OpenWindowCount => WindowStack.Count;

        internal Stack<Window> WindowStack { get; }

        public WindowTransitionManager TransitionManager { get; }
        public Control FocussedControl { get; private set; }
        public bool IsMouseOverControl { get => _messageHandler.IsMouseOverControl; }

        public Window TopWindow => WindowStack.Count > 0 ? WindowStack.Peek() : null;

        private WndWindowManager()
        {
            WindowStack = new Stack<Window>();
            FocussedControl = null;
        }

        public WndWindowManager(Game game): this()
        {
            _panel = game.Panel;
            _messageHandler = new WndInputMessageHandler(this, game);
            game.InputMessageBuffer.Handlers.Add(_messageHandler);
            TransitionManager = new WindowTransitionManager(game.AssetStore.WindowTransitions);
            _loader = new WndWindowLoader(game);
        }

        internal WndWindowManager(IPanel panel, IWndWindowLoader loader): this()
        {
            _panel = panel;
            _loader = loader;
        }

        public Window LoadWindow(string wndFileName)
        {
            return _loader.LoadWindow(wndFileName);
        }

        public Window PushWindow(Window window)
        {
            window.Size = _panel.ClientBounds.Size;
            WindowStack.Push(window);
            _loader.LayoutInit(window);
            FocussedControl = null;
            return window;
        }

        public Window PushWindow(string wndFileName, object tag = null)
        {
            var window = _loader.LoadWindow(wndFileName);
            window.Tag = tag;
            return PushWindow(window);
        }

        public Window SetWindow(string wndFileName, object tag = null)
        {
            // TODO: Handle transitions between windows.

            while (WindowStack.Count > 0)
            {
                PopWindow();
            }

            return PushWindow(wndFileName, tag);
        }

        internal void EnabledChanged(Control control)
        {
            if (FocussedControl != null)
            {
                if (FocussedControl == control && !control.Enabled)
                {
                    FocussedControl = null;
                }
            }
        }

        internal void OnViewportSizeChanged(in Size newSize)
        {
            foreach (var window in WindowStack)
            {
                window.Size = newSize;
            }
        }

        public void PopWindow()
        {
            var popped = WindowStack.Pop();
            popped.Dispose();
        }

        public void PopWindow(string windowName)
        {
            var window = WindowStack.Peek();

            if (window.Name == windowName)
            {
                WindowStack.Pop();
                window.Dispose();
            }
        }

        private Window PrepareMessageBox(string title, string text)
        {
            var messageBox = PushWindow(@"Menus\MessageBox.wnd");
            messageBox.Controls.FindControl("MessageBox.wnd:StaticTextTitle").Text = title;
            var staticTextTitle = messageBox.Controls.FindControl("MessageBox.wnd:StaticTextTitle") as Label;
            staticTextTitle.TextAlignment = TextAlignment.Leading;

            messageBox.Controls.FindControl("MessageBox.wnd:StaticTextMessage").Text = text;

            return messageBox;
        }

        public void ShowMessageBox(string title, string text)
        {
            var messageBox = this.PrepareMessageBox(title, text);

            messageBox.Controls.FindControl("MessageBox.wnd:ButtonOk").Show();
        }

        public void ShowDialogBox(string title, string text, out Control yesButton, out Control noButton)
        {
            var messageBox = this.PrepareMessageBox(title, text);
            yesButton = messageBox.Controls.FindControl("MessageBox.wnd:ButtonYes");
            yesButton.Show();
            noButton = messageBox.Controls.FindControl("MessageBox.wnd:ButtonNo");
            noButton.Show();
        }

        public Control GetControlAtPoint(in Point2D mousePosition)
        {
            if (WindowStack.Count == 0)
            {
                return null;
            }

            var windowArray = WindowStack.ToArray();
            foreach(var window in windowArray)
            {
                var control = window.GetSelfOrDescendantAtPoint(mousePosition);
                if(control != null && control != window)
                {
                    return control;
                }
            }

            return null;
        }

        internal void Focus(Control control)
        {
            FocussedControl = control;
        }

        public List<Control> GetControlsAtPoint(in Point2D mousePosition)
        {
            if (WindowStack.Count == 0)
            {
                return new List<Control>();
            }
            else
            {
                var window = WindowStack.Peek();
                var result = window.GetSelfOrDescendantsAtPoint(mousePosition);
                return result;
            }
        }

        internal void Update(in TimeInterval gameTime)
        {
            foreach (var window in WindowStack)
            {
                _loader.LayoutUpdate(window);
            }

            TransitionManager.Update(gameTime);

            foreach (var window in WindowStack)
            {
                window.Update();
            }
        }

        internal void Render(DrawingContext2D drawingContext)
        {
            // TODO: Try to avoid using LINQ here.
            foreach (var window in WindowStack.Reverse())
            {
                window.Render(drawingContext);
            }
        }
    }
}
