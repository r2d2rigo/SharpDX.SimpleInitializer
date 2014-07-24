using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace SharpDX.SimpleInitializer.Silverlight
{
    internal class DrawingSurfaceBackgroundContentProvider : DrawingSurfaceBackgroundContentProviderNativeBase
    {
        private SharpDXContext sharpDXContext;
        private DrawingSurfaceRuntimeHost runtimeHost;
        private Size backBufferSize;

        public DrawingSurfaceBackgroundContentProvider(SharpDXContext context)
        {
            this.sharpDXContext = context;
        }

        public override void Connect(DrawingSurfaceRuntimeHost host, Device device)
        {
            this.runtimeHost = host;
        }

        public override void Disconnect()
        {
            this.runtimeHost = null;
        }

        public override void Draw(Device device, DeviceContext context, RenderTargetView renderTargetView)
        {
            bool deviceReset = false;

            if (device != this.sharpDXContext.D3DDevice)
            {
                this.sharpDXContext.OnDeviceReset(device, context);
                deviceReset = true;
            }

            this.sharpDXContext.D3DContext.ClearState();
            this.sharpDXContext.renderTargetView = renderTargetView;

            using (Texture2D backBufferTexture = new Texture2D(this.sharpDXContext.RenderTargetView.Resource.NativePointer))
            {
                int currentWidth = (int)this.backBufferSize.Width;
                int currentHeight = (int)this.backBufferSize.Height;

                if ((currentWidth != backBufferTexture.Description.Width && currentHeight != backBufferTexture.Description.Height)
                    || deviceReset)
                {
                    this.backBufferSize.Width = backBufferTexture.Description.Width;
                    this.backBufferSize.Height = backBufferTexture.Description.Height;

                    this.sharpDXContext.RecreateDepthStencilBuffer(this.backBufferSize);
                }
            }

            ViewportF viewport = new ViewportF(0, 0, (float)this.backBufferSize.Width, (float)this.backBufferSize.Height);
            sharpDXContext.D3DContext.Rasterizer.SetViewport(viewport);

            this.sharpDXContext.OnRender();

            this.runtimeHost.RequestAdditionalFrame();
        }

        public override void PrepareResources(DateTime presentTargetTime, ref Size2F desiredRenderTargetSize)
        {
        }
    }
}
