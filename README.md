SharpDX.SimpleInitializer
=========================

A library for easy SharpDX initialization on Windows Phone, Windows Store and Universal XAML applications.


What is it?
-----------

A library that exposes a single class for initializing a Direct3D 11 device and context from an existing XAML control. The following project types are currently supported:
* Windows Phone 8.
* Windows Phone 8.1 Silverlight.
* Windows Store 8.1.
* Windows Phone 8.1/Windows Store 8.1 Universal.


Installing
----------

Install the package from NuGet: https://www.nuget.org/packages/SharpDX.SimpleInitializer/

Alternatively, you can download the project and add it your solution or download one of the precompiled libraries from the **Binaries** folder.

How to use
----------

Create an instance of the **SharpDXContext** class and call its **BindToControl** method, passing as a parameter the drawing control of your Page (**SwapChainPanel**, **SwapChainBackgroundPanel**, **DrawingSurface** or **DrawingSurfaceBackgroundGrid**).

The object will raise the **Render** event where you can perform your own drawing.

The object will raise the **DeviceReset** event when the Direct3D 11 device is lost and recreated. You should manage the reloading of your resources (textures, vertex buffers...) in there.

Don't forget to unsubscribe the events and call **Dispose** when you are navigating away from the Page, so all unmanaged memory claimed by DirectX can be reclaimed.

    private SharpDXContext context;

    protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        this.context = new SharpDXContext();
        this.context.Render += context_Render;

        this.context.BindToControl(this.DrawingSurface);
    }

    protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);

        this.context.Render -= context_Render;
        this.context.Dispose();
    }

    void context_Render(object sender, System.EventArgs e)
    {
      // Perform rendering
    }

A repository with some samples can be found at https://github.com/r2d2rigo/SharpDX.SimpleInitializer-Samples
