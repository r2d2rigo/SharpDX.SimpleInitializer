// The MIT License (MIT)
// 
// Copyright (c) 2014 Rodrigo 'r2d2rigo' Diaz
// Portions of this code Copyright (c) 2010-2013 Alexandre Mutel
//
// See LICENSE for full license.

using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace SharpDX.SimpleInitializer
{
    /// <summary>
    /// SharpDXContext implementation for WinRT applications.
    /// </summary>
    public class SharpDXContext : SharpDXContextBase
    {
        /// <summary>
        /// Default DPI por pixel size calculation.
        /// </summary>
        private static readonly float DEFAULT_DPI = 96.0f;

        /// <summary>
        /// Minimum back buffer width/height, because sometimes bound controls aren't measured yet and their values are 0.
        /// </summary>
        private static readonly float MIN_BACKBUFFER_DIMENSION = 1.0f;

        private SwapChain1 swapChain;
        private SwapChainBackgroundPanel backgroundPanel;
        private SwapChainPanel panel;
        private ISwapChainBackgroundPanelNative nativeBackgroundPanel;
        private ISwapChainPanelNative nativePanel;

        /// <summary>
        /// Creates a new SharpDXContext instance.
        /// </summary>
        public SharpDXContext()
            : base()
        {
        }

        /// <summary>
        /// Binds the object to a SwapChainBackgroundPanel and initializes Direct3D11 resources.
        /// </summary>
        /// <param name="backgroundPanel">SwapChainBackgroundPanel control used for drawing.</param>
        public void BindToControl(SwapChainBackgroundPanel backgroundPanel)
        {
            this.ThrowIfDisposed();
            this.ThrowIfBound();

            this.backgroundPanel = backgroundPanel;
            this.nativeBackgroundPanel = ComObject.As<ISwapChainBackgroundPanelNative>(this.backgroundPanel);
            this.UpdateBackBufferSize();

            this.CreateDeviceDependentResources();
            this.CreateSizeDependentResources();

            CompositionTarget.Rendering += CompositionTarget_Rendering;
            this.backgroundPanel.SizeChanged += HostControl_SizeChanged;
            DisplayInformation.GetForCurrentView().DpiChanged += DisplayInformation_DpiChanged;
            
            this.IsBound = true;
        }

        /// <summary>
        /// Binds the object to a SwapChainPanel and initializes Direct3D11 resources.
        /// </summary>
        /// <param name="panel">SwapChainPanel control used for drawing.</param>
        public void BindToControl(SwapChainPanel panel)
        {
            this.ThrowIfDisposed();
            this.ThrowIfBound();

            this.panel = panel;
            this.nativePanel = ComObject.As<ISwapChainPanelNative>(this.panel);
            this.UpdateBackBufferSize();

            this.CreateDeviceDependentResources();
            this.CreateSizeDependentResources();

            CompositionTarget.Rendering += CompositionTarget_Rendering;
            this.panel.SizeChanged += HostControl_SizeChanged;
            DisplayInformation.GetForCurrentView().DpiChanged += DisplayInformation_DpiChanged;
            
            this.IsBound = true;
        }

        /// <summary>
        /// Called when the composition target request a rendering operation.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Parameters.</param>
        private void CompositionTarget_Rendering(object sender, object e)
        {
            this.OnRender();

            PresentParameters parameters = new PresentParameters();

            try
            {
                swapChain.Present(1, PresentFlags.None, parameters);
            }
            catch (SharpDXException ex)
            {
                if (ex.ResultCode == SharpDX.DXGI.ResultCode.DeviceRemoved
                    || ex.ResultCode == SharpDX.DXGI.ResultCode.DeviceReset)
                {
                    this.CreateDeviceDependentResources();
                    this.CreateSizeDependentResources();
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Creates resources that depend on the current device.
        /// </summary>
        protected override void CreateDeviceDependentResources()
        {
            this.ReleaseDeviceDependentResources();

#if DEBUG
            DeviceCreationFlags creationFlags = DeviceCreationFlags.Debug;
#else
            DeviceCreationFlags creationFlags = DeviceCreationFlags.None;
#endif

            using (SharpDX.Direct3D11.Device defaultDevice = new SharpDX.Direct3D11.Device(DriverType.Hardware, creationFlags))
            {
                SharpDX.Direct3D11.Device1 newDevice = defaultDevice.QueryInterface<SharpDX.Direct3D11.Device1>();
                DeviceContext1 newContext = newDevice.ImmediateContext.QueryInterface<DeviceContext1>();

                this.OnDeviceReset(newDevice, newContext);
            }
        }

        /// <summary>
        /// Creates resources that depend on the current back buffer size.
        /// </summary>
        private void CreateSizeDependentResources()
        {
            this.ReleaseSizeDependentResources();

            if (swapChain != null)
            {
                swapChain.ResizeBuffers(2, (int)this.BackBufferSize.Width, (int)this.BackBufferSize.Height, Format.B8G8R8A8_UNorm, SwapChainFlags.None);
            }
            else
            {
                SwapChainDescription1 swapChainDescription = new SwapChainDescription1()
                {
                    Width = (int)this.BackBufferSize.Width,
                    Height = (int)this.BackBufferSize.Height,
                    Format = Format.B8G8R8A8_UNorm,
                    Stereo = false,
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = Usage.BackBuffer | Usage.RenderTargetOutput,
                    BufferCount = 2,
                    Scaling = Scaling.Stretch,
                    SwapEffect = SwapEffect.FlipSequential,
                };

                using (SharpDX.DXGI.Device2 dxgiDevice2 = this.D3DDevice.QueryInterface<SharpDX.DXGI.Device2>())
                {
                    using (SharpDX.DXGI.Adapter dxgiAdapter = dxgiDevice2.Adapter)
                    {
                        using (SharpDX.DXGI.Factory2 dxgiFactory2 = dxgiAdapter.GetParent<SharpDX.DXGI.Factory2>())
                        {
                            swapChain = new SwapChain1(dxgiFactory2, this.D3DDevice, ref swapChainDescription);

                            if (this.backgroundPanel != null)
                            {
                                nativeBackgroundPanel.SwapChain = swapChain;
                            }
                            else if (this.panel != null)
                            {
                                nativePanel.SwapChain = swapChain;
                            }

                            dxgiDevice2.MaximumFrameLatency = 1;
                        }
                    }
                }
            }

            this.BackBuffer = Texture2D.FromSwapChain<Texture2D>(swapChain, 0);
            this.BackBufferView = new RenderTargetView(this.D3DDevice, this.BackBuffer);
            this.UpdateBackBufferSize();

            using (Texture2D depthBuffer = new Texture2D(this.D3DDevice, new Texture2DDescription()
            {
                Format = SharpDX.DXGI.Format.D24_UNorm_S8_UInt,
                ArraySize = 1,
                MipLevels = 1,
                Width = (int)this.BackBufferSize.Width,
                Height = (int)this.BackBufferSize.Height,
                SampleDescription = new SampleDescription(1, 0),
                BindFlags = BindFlags.DepthStencil,
            }))
            {
                this.DepthStencilView = new DepthStencilView(this.D3DDevice, depthBuffer, new DepthStencilViewDescription()
                {
                    Dimension = DepthStencilViewDimension.Texture2D
                });
            }

            ViewportF viewport = new ViewportF(0, 0, (float)this.BackBufferSize.Width, (float)this.BackBufferSize.Height, 0.0f, 1.0f);
            this.D3DContext.Rasterizer.SetViewport(viewport);
        }

        /// <summary>
        /// Disposes this object's resources.
        /// </summary>
        /// <param name="disposing">true if releasing managed resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Utilities.Dispose(ref this.swapChain);

                DisplayInformation.GetForCurrentView().DpiChanged -= DisplayInformation_DpiChanged;
                CompositionTarget.Rendering -= CompositionTarget_Rendering;

                if (this.nativeBackgroundPanel != null)
                {
                    Utilities.Dispose(ref this.nativeBackgroundPanel);
                    this.backgroundPanel.SizeChanged -= HostControl_SizeChanged;
                }

                if (this.nativePanel != null)
                {
                    Utilities.Dispose(ref this.nativePanel);
                    this.panel.SizeChanged -= HostControl_SizeChanged;
                }

                this.backgroundPanel = null;
                this.panel = null;
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Called when the size of the host control changes.
        /// </summary>
        /// <param name="sender">Sender control.</param>
        /// <param name="e">Event arguments.</param>
        private void HostControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.UpdateBackBufferSize();
            this.CreateSizeDependentResources();
        }

        /// <summary>
        /// Called when the DPI of the current display change.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="args">Event arguments.</param>
        private void DisplayInformation_DpiChanged(DisplayInformation sender, object args)
        {
            this.UpdateBackBufferSize();
            this.CreateSizeDependentResources();
        }   
        
        /// <summary>
        /// Calculates the correct back buffer size, taking into account display DPI and hosted control size.
        /// </summary>
        private void UpdateBackBufferSize()
        {
            float dpi = DisplayInformation.GetForCurrentView().LogicalDpi;

            if (this.backgroundPanel != null)
            {
                this.BackBufferSize = new Size(
                    Math.Max(this.backgroundPanel.ActualWidth * (dpi / DEFAULT_DPI), MIN_BACKBUFFER_DIMENSION),
                    Math.Max(this.backgroundPanel.ActualHeight * (dpi / DEFAULT_DPI), MIN_BACKBUFFER_DIMENSION)
                    );
            }

            if (this.panel != null)
            {
                this.BackBufferSize = new Size(
                    Math.Max(this.panel.ActualWidth * (dpi / DEFAULT_DPI), MIN_BACKBUFFER_DIMENSION),
                    Math.Max(this.panel.ActualHeight * (dpi / DEFAULT_DPI), MIN_BACKBUFFER_DIMENSION)
                    );
            }
        }
    }
}