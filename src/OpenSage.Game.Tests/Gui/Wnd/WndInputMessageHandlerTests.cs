using System.Collections.Generic;
using OpenSage.Gui.Wnd;
using OpenSage.Gui.Wnd.Controls;
using OpenSage.Input;
using OpenSage.Mathematics;
using Xunit;


namespace OpenSage.Tests.Gui.Wnd
{
    public class WndInputMessageHandlerTests
    {
        private MockPanel _panel;
        private MockWndWindowLoader _loader;
        private WndWindowManager _windowManager;
        private WndInputMessageHandler _messageHandler;
        private Window _window;

        private List<(Control, WndWindowMessageType)> _invoked;

        private const int W = 50;
        private const int H = 50;

        private bool InputCallback(Control control, WndWindowMessage message, ControlCallbackContext context)
        {
            _invoked.Add((control, message.MessageType));
            return true;
        }

        private Control CreateControl(Control parent, string name, int y, int height)
        {
            var control = new Control
            {
                Name = name,
                Bounds = new Rectangle(0, y, W, height),
                InputCallback = InputCallback,
                Window = _window
            };
            if (parent != null)
            {
                parent.Controls.Add(control);
            }
            return control;
        }

        public WndInputMessageHandlerTests()
        {
            var root = CreateControl(null, "Root", 0, H);
            _panel = new MockPanel(W, H);
            _loader = new MockWndWindowLoader();
            _windowManager = new WndWindowManager(_panel, _loader);
            _messageHandler = new WndInputMessageHandler(_windowManager, null);
            _window = new Window(new(W, H), root, _windowManager)
            {
                Name = "Window"
            };
            _windowManager.PushWindow(_window);
            _invoked = new List<(Control, WndWindowMessageType)>();
        }

        private void TestMessage(InputMessage message, InputMessageResult result, 
                                 bool mouseOverControl, List<(Control, WndWindowMessageType)> expected)
        {
            Assert.Equal(result, _messageHandler.HandleMessage(message));
            Assert.Equal(expected, _invoked);
            _invoked.Clear();
            Assert.Equal(mouseOverControl, _messageHandler.IsMouseOverControl);
        }

        private void TestNotHandled(InputMessage message, List<(Control, WndWindowMessageType)> expected = null)
        {
            if (expected == null)
            {
                expected = new List<(Control, WndWindowMessageType)>();
            }
            TestMessage(message, InputMessageResult.NotHandled, false, expected);
        }

        private void TestHandled(InputMessage message, List<(Control, WndWindowMessageType)> expected)
        {
            TestMessage(message, InputMessageResult.Handled, true, expected);
        }

        private void TestHandled(InputMessage message, (Control, WndWindowMessageType) expected)
        {
            var expectedList = new List<(Control, WndWindowMessageType)>{ expected };
            TestMessage(message, InputMessageResult.Handled, true, expectedList);
        }

        private void TestMouseAndKeyHandled(Control control, in Point2D point)
        {
            TestHandled(InputMessage.CreateMouseButton(InputMessageType.MouseLeftButtonDown, point),
                        (control, WndWindowMessageType.MouseDown));
            TestHandled(InputMessage.CreateMouseButton(InputMessageType.MouseLeftButtonUp, point),
                        (control, WndWindowMessageType.MouseUp));
            TestHandled(InputMessage.CreateMouseButton(InputMessageType.MouseRightButtonDown, point),
                        (control, WndWindowMessageType.MouseRightDown));
            TestHandled(InputMessage.CreateMouseButton(InputMessageType.MouseRightButtonUp, point),
                        (control, WndWindowMessageType.MouseRightUp));
            TestHandled(InputMessage.CreateKeyDown(Veldrid.Key.F, Veldrid.ModifierKeys.None),
                        (control, WndWindowMessageType.KeyDown));
        }

        private void TestMouseAndKeyNotHandled(in Point2D point)
        {
            TestNotHandled(InputMessage.CreateMouseButton(InputMessageType.MouseLeftButtonDown, point));
            TestNotHandled(InputMessage.CreateMouseButton(InputMessageType.MouseLeftButtonUp, point));
            TestNotHandled(InputMessage.CreateMouseButton(InputMessageType.MouseRightButtonDown, point));
            TestNotHandled(InputMessage.CreateMouseButton(InputMessageType.MouseRightButtonUp, point));
            TestNotHandled(InputMessage.CreateKeyDown(Veldrid.Key.F, Veldrid.ModifierKeys.None));
        }


        [Fact]
        public void TestDisabledNoInput()
        {
            var controlA = CreateControl(_window.Root, "ControlA", 0, H);
            var controlB = CreateControl(controlA, "ControlB", 0, H);
            Point2D point = new(W / 2, H / 2);
            controlA.Enabled = false;
            _windowManager.Focus(controlB);
            TestNotHandled(InputMessage.CreateMouseMove(new(-1, -1)));
            TestHandled(InputMessage.CreateMouseMove(point), new List<(Control, WndWindowMessageType)> {
                (controlB, WndWindowMessageType.MouseEnter),
                (controlA, WndWindowMessageType.MouseEnter),
                (_window.Root, WndWindowMessageType.MouseEnter),
                (controlB, WndWindowMessageType.MouseMove),
                (controlA, WndWindowMessageType.MouseMove),
                (_window.Root, WndWindowMessageType.MouseMove),
            });
            TestMouseAndKeyHandled(_window.Root, point);
            _window.Root.NoInput = true;
            TestNotHandled(InputMessage.CreateMouseMove(point));
            TestMouseAndKeyNotHandled(point);
            TestNotHandled(InputMessage.CreateMouseMove(new(-1, -1)), new List<(Control, WndWindowMessageType)> {
                (controlB, WndWindowMessageType.MouseExit),
                (controlA, WndWindowMessageType.MouseExit),
                (_window.Root, WndWindowMessageType.MouseExit)
            });

        }

        [Fact]
        public void TestTwoChildren()
        {
            var controlA = CreateControl(_window.Root, "ControlA", 0, H / 2);
            var controlB = CreateControl(_window.Root, "ControlB", H / 2, H / 2);
            Point2D point = new(W / 2, H / 4);
            _windowManager.Focus(controlA);
            TestNotHandled(InputMessage.CreateMouseMove(new(-1, -1)));
            TestHandled(InputMessage.CreateMouseMove(point), new List<(Control, WndWindowMessageType)> {
                (controlA, WndWindowMessageType.MouseEnter),
                (_window.Root, WndWindowMessageType.MouseEnter),
                (controlA, WndWindowMessageType.MouseMove),
                (_window.Root, WndWindowMessageType.MouseMove),
            });
            TestMouseAndKeyHandled(controlA, point);
            point = new(W / 2, 3 * H / 4);
            _windowManager.Focus(controlB);
            TestHandled(InputMessage.CreateMouseMove(point), new List<(Control, WndWindowMessageType)> {
                (controlA, WndWindowMessageType.MouseExit),
                (controlB, WndWindowMessageType.MouseEnter),
                (controlB, WndWindowMessageType.MouseMove),
                (_window.Root, WndWindowMessageType.MouseMove),
            });
            TestMouseAndKeyHandled(controlB, point);
            TestNotHandled(InputMessage.CreateMouseMove(new(-1, -1)), new List<(Control, WndWindowMessageType)> {
                (controlB, WndWindowMessageType.MouseExit),
                (_window.Root, WndWindowMessageType.MouseExit)
            });
        }
    }
}