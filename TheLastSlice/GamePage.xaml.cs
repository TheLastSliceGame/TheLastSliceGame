using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TheLastSlice
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GamePage : Page
    {
        readonly TheLastSliceGame _game;

        public GamePage()
        {
            InitializeComponent();

            ApplicationView.PreferredLaunchViewSize = new Size(800, 600);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;

            SizeChanged += SizeChangedEventHandler;


            // Create the game.
            var launchArguments = string.Empty;
            _game = MonoGame.Framework.XamlGame<TheLastSliceGame>.Create(launchArguments, Window.Current.CoreWindow, swapChainPanel);
        }

        public void SizeChangedEventHandler(Object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width * .75 > e.NewSize.Height)
            {
                swapChainPanel.Height = e.NewSize.Height;
                swapChainPanel.Width = e.NewSize.Height * (800f / 600f);
            }
            else
            {
                swapChainPanel.Width = e.NewSize.Width;
                swapChainPanel.Height = e.NewSize.Width * .75;
            }
            

        }
    }

}

