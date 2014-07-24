using Microsoft.Phone.Controls;
using SharpDX;
using SharpDX.SimpleInitializer;
using System;
using Windows.Phone.Input.Interop;

namespace Samples.WP8.DrawingSurfaceBackgroundGrid
{
    public partial class MainPage : PhoneApplicationPage
    {
        private SharpDXContext context;

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();

            this.context = new SharpDXContext();
            this.context.Render += context_Render;
            this.context.PointerMoved += context_PointerMoved;

            this.context.BindToControl(this.LayoutRoot);
        }

        void context_Render(object sender, EventArgs e)
        {
            this.context.D3DContext.ClearRenderTargetView(this.context.RenderTargetView, Color.CornflowerBlue);
        }

        void context_PointerMoved(DrawingSurfaceManipulationHost sender, Windows.UI.Core.PointerEventArgs args)
        {
        }
    }
}