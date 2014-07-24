using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Phone.Input.Interop;
using Windows.UI.Core;

namespace SharpDX.SimpleInitializer.Silverlight
{
    internal class DrawingSurfaceManipulationHandler : IDrawingSurfaceManipulationHandler
    {
        public event EventHandler<DrawingSurfaceManipulationHost> ManipulationHostChanged;

        public DrawingSurfaceManipulationHandler()
        {
        }

        public void SetManipulationHost(DrawingSurfaceManipulationHost manipulationHost)
        {
            if (this.ManipulationHostChanged != null)
            {
                this.ManipulationHostChanged(this, manipulationHost);
            }
        }
    }
}
