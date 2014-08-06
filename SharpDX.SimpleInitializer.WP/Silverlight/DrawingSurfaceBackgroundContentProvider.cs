// The MIT License (MIT)
// 
// Copyright (c) 2014 Rodrigo 'r2d2rigo' Diaz
// Portions of this code Copyright (c) 2010-2013 Alexandre Mutel
//
// See LICENSE for full license.

using SharpDX.Direct3D11;
using System;
using Windows.Foundation;

namespace SharpDX.SimpleInitializer.Silverlight
{
    /// <summary>
    /// Implements a content provider for a DrawingSurfaceBackgroundGrid.
    /// </summary>
    internal class DrawingSurfaceBackgroundContentProvider : DrawingSurfaceBackgroundContentProviderNativeBase
    {
        private SharpDXContext sharpDXContext;
        private DrawingSurfaceRuntimeHost runtimeHost;

        /// <summary>
        /// Default construtor.
        /// </summary>
        /// <param name="context">Parent SharpDXContext object.</param>
        public DrawingSurfaceBackgroundContentProvider(SharpDXContext context)
        {
            this.sharpDXContext = context;
        }

        /// <summary>
        /// Called when the content provider is connected to a runtime host.
        /// </summary>
        /// <param name="host">DrawingSurfaceRuntimeHost host object.</param>
        /// <param name="device">Direct3D11 device.</param>
        public override void Connect(DrawingSurfaceRuntimeHost host, Device device)
        {
            this.runtimeHost = host;
        }

        /// <summary>
        /// Called when the content provider is disconnected from the host.
        /// </summary>
        public override void Disconnect()
        {
            this.runtimeHost = null;
        }

        /// <summary>
        /// Called when the host request a new frame.
        /// </summary>
        /// <param name="device">Direct3D11 device.</param>
        /// <param name="context">Direct3D11 device context.</param>
        /// <param name="renderTargetView">Render target used for drawing.</param>
        public override void Draw(Device device, DeviceContext context, RenderTargetView renderTargetView)
        {
            bool deviceReset = false;

            if (device != this.sharpDXContext.D3DDevice)
            {
                this.sharpDXContext.OnDeviceReset(device, context);
                deviceReset = true;
            }

            this.sharpDXContext.D3DContext.ClearState();
            this.sharpDXContext.BackBufferView = renderTargetView;

            using (Texture2D backBufferTexture = new Texture2D(this.sharpDXContext.BackBufferView.Resource.NativePointer))
            {
                int currentWidth = (int)this.sharpDXContext.BackBufferSize.Width;
                int currentHeight = (int)this.sharpDXContext.BackBufferSize.Height;

                this.sharpDXContext.BackBuffer = backBufferTexture;

                if ((currentWidth != backBufferTexture.Description.Width && currentHeight != backBufferTexture.Description.Height)
                    || deviceReset)
                {
                    this.sharpDXContext.BackBufferSize = new Size(backBufferTexture.Description.Width, backBufferTexture.Description.Height);

                    this.sharpDXContext.RecreateDepthStencil(this.sharpDXContext.BackBufferSize);
                }
            }

            ViewportF viewport = new ViewportF(0, 0, (float)this.sharpDXContext.BackBufferSize.Width, (float)this.sharpDXContext.BackBufferSize.Height);
            sharpDXContext.D3DContext.Rasterizer.SetViewport(viewport);

            this.sharpDXContext.OnRender();

            this.runtimeHost.RequestAdditionalFrame();
        }

        /// <summary>
        /// Called when the resources need to be prepared.
        /// </summary>
        /// <param name="presentTargetTime">Present target time.</param>
        /// <param name="desiredRenderTargetSize">Desired drawing size.</param>
        public override void PrepareResources(DateTime presentTargetTime, ref Size2F desiredRenderTargetSize)
        {
        }
    }
}
