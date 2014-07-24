using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.SimpleInitializer.Silverlight;
using System;
using System.Windows.Controls;
using Windows.Foundation;
using Windows.Phone.Input.Interop;
using Windows.UI.Core;

namespace SharpDX.SimpleInitializer
{
    public class SharpDXContext
    {
        internal DrawingSurfaceBackgroundContentProvider contentProvider;
        internal DrawingSurfaceContentProvider surfaceContentProvider;
        internal DrawingSurfaceManipulationHandler manipulationHandler;

        internal Texture2D renderTarget;
        private bool depthStencilEnabled;

        public event TypedEventHandler<DrawingSurfaceManipulationHost, PointerEventArgs> PointerMoved;
        public event TypedEventHandler<DrawingSurfaceManipulationHost, PointerEventArgs> PointerPressed;
        public event TypedEventHandler<DrawingSurfaceManipulationHost, PointerEventArgs> PointerReleased;
        public event EventHandler<DeviceResetEventArgs> DeviceReset;
        public event EventHandler Render;

        private Device d3DDevice;
        public Device D3DDevice
        {
            get
            {
                return this.d3DDevice;
            }
        }

        private DeviceContext d3DContext;
        public DeviceContext D3DContext
        {
            get
            {
                return this.d3DContext;
            }
        }

        internal RenderTargetView renderTargetView;
        public RenderTargetView RenderTargetView
        {
            get
            {
                return this.renderTargetView;
            }
        }

        internal DepthStencilView depthStencilView;
        public DepthStencilView DepthStencilView
        {
            get
            {
                return this.depthStencilView;
            }
        }

        public SharpDXContext(bool useDepthStencil = true)
        {
            this.depthStencilEnabled = useDepthStencil;

            this.manipulationHandler = new DrawingSurfaceManipulationHandler();
        }

        public void BindToControl(DrawingSurfaceBackgroundGrid backgroundGrid)
        {
            this.contentProvider = new DrawingSurfaceBackgroundContentProvider(this);

            this.manipulationHandler.ManipulationHostChanged += (sender, host) =>
            {
                host.PointerPressed += this.OnPointerPressed;
                host.PointerMoved += this.OnPointerMoved;
                host.PointerReleased += this.OnPointerReleased;
            };

            backgroundGrid.SetBackgroundContentProvider(this.contentProvider);
            backgroundGrid.SetBackgroundManipulationHandler(this.manipulationHandler);
        }

        public void BindToControl(DrawingSurface drawingSurface)
        {
            this.surfaceContentProvider = new DrawingSurfaceContentProvider(this);

            this.manipulationHandler.ManipulationHostChanged += (sender, host) =>
            {
                host.PointerPressed += this.OnPointerPressed;
                host.PointerMoved += this.OnPointerMoved;
                host.PointerReleased += this.OnPointerReleased;
            };

            drawingSurface.SetContentProvider(this.surfaceContentProvider);
            drawingSurface.SetManipulationHandler(this.manipulationHandler);
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

        internal void OnRender()
        {
            if (this.Render != null)
            {
                this.Render(this, EventArgs.Empty);
            }
        }

        internal void OnDeviceReset(Device device, DeviceContext context)
        {
            Utilities.Dispose(ref this.d3DDevice);
            Utilities.Dispose(ref this.d3DContext);

            this.d3DDevice = device;
            this.d3DContext = context;

            if (this.DeviceReset != null)
            {
                this.DeviceReset(this, new DeviceResetEventArgs(device, context));
            }
        }

        internal void RecreateBackBuffer(Size dimensions)
        {
            Utilities.Dispose(ref this.renderTarget);
            this.renderTarget = new Texture2D(this.D3DDevice, new Texture2DDescription()
            {
                Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                Width = (int)dimensions.Width,
                Height = (int)dimensions.Height,
                ArraySize = 1,
                MipLevels = 1,
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                Usage = ResourceUsage.Default,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.SharedKeyedmutex | ResourceOptionFlags.SharedNthandle,
                SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0)
            });

            Utilities.Dispose(ref this.renderTargetView);
            this.renderTargetView = new RenderTargetView(this.D3DDevice, this.renderTarget);
        }

        internal void RecreateDepthStencilBuffer(Size dimensions)
        {
            if (this.depthStencilEnabled)
            {
                Utilities.Dispose(ref this.depthStencilView);

                using (Texture2D depthTexture = new Texture2D(this.D3DDevice,
                    new Texture2DDescription()
                    {
                        Width = (int)dimensions.Width,
                        Height = (int)dimensions.Height,
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
                    this.depthStencilView = new DepthStencilView(this.D3DDevice, depthTexture);
                }
            }
        }
    }
}
