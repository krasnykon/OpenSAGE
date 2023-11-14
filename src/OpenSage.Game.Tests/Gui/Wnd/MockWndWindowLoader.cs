using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenSage.Gui.Wnd;
using OpenSage.Gui.Wnd.Controls;

namespace OpenSage.Tests.Gui.Wnd
{
    public class MockWndWindowLoader : IWndWindowLoader
    {
        public void LayoutInit(Window window) { }

        public void LayoutUpdate(Window window) { }

        public Window LoadWindow(string wndFileName)
        {
            throw new NotImplementedException();
        }
    }
}