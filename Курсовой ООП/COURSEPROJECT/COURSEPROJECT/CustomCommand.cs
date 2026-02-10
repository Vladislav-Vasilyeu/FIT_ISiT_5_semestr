using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace COURSEPROJECT
{
    public static class CustomCommand
    {
        public static readonly RoutedCommand OpenProfileWindow = new RoutedUICommand(
            "Open Profile Window",
            "OpenProfileWindow",
            typeof(CustomCommand),
            new InputGestureCollection {
                new KeyGesture(Key.P, ModifierKeys.Control)
            }
        );
        public static readonly RoutedCommand SwitchTheme = new RoutedUICommand(
            "Switch Theme",
            "SwitchTheme",
            typeof(CustomCommand),
            new InputGestureCollection {
                new KeyGesture(Key.T, ModifierKeys.Control)
            }
        ); 
    }
}
