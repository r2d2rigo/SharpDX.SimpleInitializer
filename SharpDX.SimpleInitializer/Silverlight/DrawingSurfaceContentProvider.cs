using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace SharpDX.SimpleInitializer.Silverlight
{
    internal class DrawingSurfaceContentProvider : DrawingSurfaceContentProviderNativeBase
    {
        private SharpDXContext sharpDXContext;
        private DrawingSurfaceRuntimeHost runtimeHost;
        private DrawingSurfaceSynchronizedTexture synchronizedTexture;

        public DrawingSurfaceContentProvider(SharpDXContext context)
        {
            this.sharpDXContext = context;

#if DEBUG
            DeviceCreationFlags creationFlags = DeviceCreationFlags.Debug;
#else
            DeviceCreationFlags creationFlags = DeviceCreationFlags.None;
#endif

            FeatureLevel[] featureLevels = 
	        {
                FeatureLevel.Level_11_1,
		        FeatureLevel.Level_11_0,
		        FeatureLevel.Level_10_1,
		        FeatureLevel.Level_10_0,
		        FeatureLevel.Level_9_3
	        };

            using (Device defaultDevice = new Device(DriverType.Hardware, creationFlags, featureLevels))
            {
                Device newDevice = defaultDevice.QueryInterface<Device1>();
                DeviceContext newContext = newDevice.ImmediateContext.QueryInterface<DeviceContext1>();

                this.sharpDXContext.OnDeviceReset(newDevice, newContext);
            }
        }

        public override void Connect(DrawingSurfaceRuntimeHost host)
        {
            this.runtimeHost = host;
        }

        public override void Disconnect()
        {
            this.runtimeHost = null;
            Utilities.Dispose(ref this.synchronizedTexture);
        }

        public override void PrepareResources(DateTime presentTargetTime, out Bool isContentDirty)
        {
            isContentDirty = true;
        }

        public override void GetTexture(Size2F surfaceSize, out DrawingSurfaceSynchronizedTexture synchronizedTexture, out RectangleF textureSubRectangle)
        {
            if (this.synchronizedTexture == null)
            {
                this.sharpDXContext.RecreateBackBuffer(new Size(surfaceSize.Width, surfaceSize.Height));
                this.sharpDXContext.RecreateDepthStencilBuffer(new Size(surfaceSize.Width, surfaceSize.Height));

                ViewportF viewport = new ViewportF(0, 0, surfaceSize.Width, surfaceSize.Height);
                this.sharpDXContext.D3DContext.Rasterizer.SetViewport(viewport);

                this.synchronizedTexture = this.runtimeHost.CreateSynchronizedTexture(this.sharpDXContext.renderTarget);
            }

            synchronizedTexture = this.synchronizedTexture;
            textureSubRectangle = new RectangleF(0, 0, surfaceSize.Width, surfaceSize.Height);

            this.synchronizedTexture.BeginDraw();
            this.sharpDXContext.OnRender();
            this.synchronizedTexture.EndDraw();

            this.runtimeHost.RequestAdditionalFrame();
        }
    }
}
