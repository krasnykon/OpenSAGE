using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenSage.Gui.Wnd.Controls;

namespace OpenSage.Gui.Wnd
{
    internal interface IWndWindowLoader
    {
        Window LoadWindow(string wndFileName);
        void LayoutInit(Window window);
        void LayoutUpdate(Window window);

    }
}