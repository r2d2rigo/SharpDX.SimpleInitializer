// The MIT License (MIT)
// 
// Copyright (c) 2014 Rodrigo 'r2d2rigo' Diaz
// Portions of this code Copyright (c) 2010-2013 Alexandre Mutel
//
// See LICENSE for full license.

using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using System;
using Windows.Foundation;

namespace SharpDX.SimpleInitializer.Silverlight
{
    /// <summary>
    /// Implements a content provider for a DrawingSurface.
    /// </summary>
    internal class DrawingSurfaceContentProvider : DrawingSurfaceContentProviderNativeBase
    {
        private SharpDXContext sharpDXContext;
        private DrawingSurfaceRuntimeHost runtimeHost;
        private DrawingSurfaceSynchronizedTexture synchronizedTexture;

        /// <summary>
        /// Default construtor.
        /// </summary>
        /// <param name="context">Parent SharpDXContext object.</param>
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

        /// <summary>
        /// Called when the content provider is connected to a runtime host.
        /// </summary>
        /// <param name="host">DrawingSurfaceRuntimeHost host object.</param>
        public override void Connect(DrawingSurfaceRuntimeHost host)
        {
            this.runtimeHost = host;
        }

        /// <summary>
        /// Called when the content provider is disconnected from the host.
        /// </summary>
        public override void Disconnect()
        {
            this.runtimeHost = null;
            Utilities.Dispose(ref this.synchronizedTexture);
        }

        /// <summary>
        /// Called when the resources need to be prepared.
        /// </summary>
        /// <param name="presentTargetTime">Present target time.</param>
        /// <param name="isContentDirty">Checks if the contents of the control have changed.</param>
        public override void PrepareResources(DateTime presentTargetTime, out Bool isContentDirty)
        {
            isContentDirty = true;
        }

        /// <summary>
        /// Gets the synchronized texture used for drawing.
        /// </summary>
        /// <param name="surfaceSize">Size of the drawing surface.</param>
        /// <param name="synchronizedTexture">Synchronized texture object.</param>
        /// <param name="textureSubRectangle">Area of the texture that has changed.</param>
        public override void GetTexture(Size2F surfaceSize, out DrawingSurfaceSynchronizedTexture synchronizedTexture, out RectangleF textureSubRectangle)
        {
            if (this.synchronizedTexture == null)
            {
                this.sharpDXContext.BackBufferSize = new Size(surfaceSize.Width, surfaceSize.Height);

                this.sharpDXContext.RecreateBackBuffer(this.sharpDXContext.BackBufferSize);
                this.sharpDXContext.RecreateDepthStencil(this.sharpDXContext.BackBufferSize);

                ViewportF viewport = new ViewportF(0, 0, surfaceSize.Width, surfaceSize.Height);
                this.sharpDXContext.D3DContext.Rasterizer.SetViewport(viewport);

                this.synchronizedTexture = this.runtimeHost.CreateSynchronizedTexture(this.sharpDXContext.BackBuffer);
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
