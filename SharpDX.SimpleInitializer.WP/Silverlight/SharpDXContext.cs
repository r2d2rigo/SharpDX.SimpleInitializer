// The MIT License (MIT)
// 
// Copyright (c) 2014 Rodrigo 'r2d2rigo' Diaz
// Portions of this code Copyright (c) 2010-2013 Alexandre Mutel
//
// See LICENSE for full license.

using SharpDX.Direct3D11;
using SharpDX.SimpleInitializer.Silverlight;
using System.Windows.Controls;
using Windows.Foundation;
using Windows.Phone.Input.Interop;
using Windows.UI.Core;

namespace SharpDX.SimpleInitializer
{
    public class SharpDXContext : SharpDXContextBase
    {
        private DrawingSurfaceBackgroundContentProvider contentProvider;
        private DrawingSurfaceContentProvider surfaceContentProvider;
        private DrawingSurfaceManipulationHandler manipulationHandler;

        public event TypedEventHandler<DrawingSurfaceManipulationHost, PointerEventArgs> PointerMoved;
        public event TypedEventHandler<DrawingSurfaceManipulationHost, PointerEventArgs> PointerPressed;
        public event TypedEventHandler<DrawingSurfaceManipulationHost, PointerEventArgs> PointerReleased;

        public SharpDXContext()
            : base()
        {
            this.manipulationHandler = new DrawingSurfaceManipulationHandler();
        }

        public void BindToControl(DrawingSurfaceBackgroundGrid backgroundGrid)
        {
            this.ThrowIfDisposed();
            this.ThrowIfBound();

            this.contentProvider = new DrawingSurfaceBackgroundContentProvider(this);

            this.manipulationHandler.ManipulationHostChanged += OnManipulationHostChanged;

            backgroundGrid.SetBackgroundContentProvider(this.contentProvider);
            backgroundGrid.SetBackgroundManipulationHandler(this.manipulationHandler);

            this.IsBound = true;
        }

        public void BindToControl(DrawingSurface drawingSurface)
        {
            this.ThrowIfDisposed();
            this.ThrowIfBound();

            this.surfaceContentProvider = new DrawingSurfaceContentProvider(this);

            this.manipulationHandler.ManipulationHostChanged += OnManipulationHostChanged;

            drawingSurface.SetContentProvider(this.surfaceContentProvider);
            drawingSurface.SetManipulationHandler(this.manipulationHandler);

            this.IsBound = true;
        }

        private void OnManipulationHostChanged(object sender, DrawingSurfaceManipulationHost host)
        {
            host.PointerPressed += this.OnPointerPressed;
            host.PointerMoved += this.OnPointerMoved;
            host.PointerReleased += this.OnPointerReleased;
        }

        private void OnPointerPressed(DrawingSurfaceManipulationHost sender, PointerEventArgs args)
        {
            if (this.PointerPressed != null)
            {
                this.PointerPressed(sender, args);
            }
        }

        private void OnPointerMoved(DrawingSurfaceManipulationHost sender, PointerEventArgs args)
        {
            if (this.PointerMoved != null)
            {
                this.PointerMoved(sender, args);
            }
        }

        private void OnPointerReleased(DrawingSurfaceManipulationHost sender, PointerEventArgs args)
        {
            if (this.PointerReleased != null)
            {
                this.PointerReleased(sender, args);
            }
        }

        internal override void RecreateBackBuffer(Size backBufferSize)
        {
            base.RecreateBackBuffer(backBufferSize);

            this.BackBuffer = new Texture2D(this.D3DDevice, new Texture2DDescription()
            {
                Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                Width = (int)backBufferSize.Width,
                Height = (int)backBufferSize.Height,
                ArraySize = 1,
                MipLevels = 1,
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                Usage = ResourceUsage.Default,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.SharedKeyedmutex | ResourceOptionFlags.SharedNthandle,
                SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0)
            });

            this.BackBufferView = new RenderTargetView(this.D3DDevice, this.BackBuffer);
        }

        internal override void RecreateDepthStencil(Size depthStencilSize)
        {
            base.RecreateDepthStencil(depthStencilSize);

            using (Texture2D depthTexture = new Texture2D(this.D3DDevice,
                new Texture2DDescription()
                {
                    Width = (int)depthStencilSize.Width,
                    Height = (int)depthStencilSize.Height,
                    ArraySize = 1,
                    BindFlags = BindFlags.DepthStencil,
                    CpuAccessFlags = CpuAccessFlags.None,
                    Format = SharpDX.DXGI.Format.D24_UNorm_S8_UInt,
                    MipLevels = 1,
                    OptionFlags = ResourceOptionFlags.None,
                    SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                    Usage = ResourceUsage.Default
                }))
            {
                this.DepthStencilView = new DepthStencilView(this.D3DDevice, depthTexture);
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (this.manipulationHandler != null)
                {
                    this.manipulationHandler.ManipulationHostChanged -= OnManipulationHostChanged;
                    this.manipulationHandler = null;
                }
            }
        }
    }
}