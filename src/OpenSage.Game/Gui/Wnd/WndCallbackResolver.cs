using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OpenSage.Gui.Wnd.Controls;

namespace OpenSage.Gui.Wnd
{
    public class WndCallbackResolver
    {
        private readonly Game _game;
        private readonly Dictionary<string, MethodInfo> _callbackCache;
        private readonly Dictionary<string, MethodInfo> _callbackFactoryCache;

        public delegate TDelegate FactoryDelegate<TDelegate>(Game game);

        internal WndCallbackResolver(Game game)
        {
            _game = game;
            _callbackCache = new Dictionary<string, MethodInfo>();
            _callbackFactoryCache = new Dictionary<string, MethodInfo>();

            // TODO: Filter by mod, perhaps using parameter to [WndCallbacks]?
            // At the moment callbacks from all mods are lumped together.
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!assembly.FullName.StartsWith("OpenSage"))
                    continue;

                foreach (var type in assembly.GetTypes())
                {
                    if (type.GetCustomAttributes(typeof(WndCallbacksAttribute), false).Length > 0)
                    {
                        foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public).Where(m => !m.IsSpecialName))
                        {
                            _callbackCache.Add(method.Name, method);
                        }
                    }
                    else if (type.GetCustomAttributes(typeof(WndCallbacksFactoryAttribute), false).Length > 0)
                    {
                        foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public).Where(m => !m.IsSpecialName))
                        {
                            _callbackFactoryCache.Add(method.Name, method);
                        }
                    }
                }
            }
        }

        internal WindowCallback GetWindowCallback(string name)
        {
            return GetCallback<WindowCallback>(name);
        }

        internal ControlCallback GetControlCallback(string name)
        {
            return GetCallback<ControlCallback>(name);
        }

        internal InputCallback GetInputCallback(string name)
        {
            return GetCallback<InputCallback>(name);
        }

        internal ControlDrawCallback GetControlDrawCallback(string name)
        {
            return GetCallback<ControlDrawCallback>(name);
        }

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private TDelegate GetCallback<TDelegate>(string name)
            where TDelegate : class
        {
            MethodInfo method;

            if (string.Equals(name, "[None]", StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            if (_callbackCache.TryGetValue(name, out method))
            {
                return (TDelegate) (object) Delegate.CreateDelegate(typeof(TDelegate), method);
                
            }
            else if (_callbackFactoryCache.TryGetValue(name, out method))
            {
                Type type = typeof(FactoryDelegate<TDelegate>);
                var factory = (FactoryDelegate<TDelegate>)Delegate.CreateDelegate(type, method);
                return factory(_game);
            }
            else
            {
                logger.Warn($"Failed to resolve callback '{name}'");
                return null;
            }
        }
    }
}
