using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OpenSage.Data.Wnd;
using OpenSage.Gui.Wnd.Controls;

namespace OpenSage.Gui.Wnd
{
    internal class WndWindowLoader : IWndWindowLoader
    {
        private readonly WndCallbackResolver _wndCallbackResolver;
        private readonly Game _game;
        public WndWindowLoader(Game game)
        {
            _game = game;
            _wndCallbackResolver = new WndCallbackResolver();
        }

        Window IWndWindowLoader.LoadWindow(string wndFileName)
        {
            var entry = _game.ContentManager.FileSystem.GetFile(Path.Combine("Window", wndFileName));
            if (entry == null)
            {
                throw new Exception($"Window file {wndFileName} was not found.");
            }
            var wndFile = WndFile.FromFileSystemEntry(entry, _game.AssetStore);
            return new Window(wndFile, _game, _wndCallbackResolver);
        }

        void IWndWindowLoader.LayoutInit(Window window)
        {
            window.LayoutInit?.Invoke(window, _game);
        }

        void IWndWindowLoader.LayoutUpdate(Window window)
        {
            window.LayoutUpdate?.Invoke(window, _game);
        }
    }
}