using System;
using OpenSage.Gui.Wnd.Controls;

namespace OpenSage.Gui.Wnd
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class WndCallbacksAttribute : Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class WndCallbacksFactoryAttribute : Attribute
    {

    }
}
