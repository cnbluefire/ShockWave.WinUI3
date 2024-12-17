using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ShockWave.App
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private byte[]? pixelShaderData;
        private SoftwareBitmap? softwareBitmap;
        private PixelShaderEffect? pixelShaderEffect;
        private System.Numerics.Vector2? centerPoint;
        private TimeSpan startTime;

        public MainWindow()
        {
            this.InitializeComponent();
        }

        private void animatedCanvas_CreateResources(Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedControl sender, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
        {
            args.TrackAsyncAction(UpdateCore().AsAsyncAction());

            async Task UpdateCore()
            {
                if (pixelShaderData == null)
                {
                    pixelShaderData = await System.IO.File.ReadAllBytesAsync(System.IO.Path.Combine(AppContext.BaseDirectory, "Shaders", "ShockWave.bin"));
                }

                if (softwareBitmap == null)
                {
                    var file = await StorageFile.GetFileFromPathAsync(System.IO.Path.Combine(AppContext.BaseDirectory, "Assets", "SampleImage.png"));
                    using var stream = await file.OpenReadAsync();
                    var bitmapDecoder = await BitmapDecoder.CreateAsync(stream);
                    var frame = await bitmapDecoder.GetFrameAsync(0);
                    using var bitmap = await frame.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                    softwareBitmap = new SoftwareBitmap(BitmapPixelFormat.Bgra8, bitmap.PixelWidth, bitmap.PixelHeight, BitmapAlphaMode.Premultiplied);
                    softwareBitmap.DpiX = 96;
                    softwareBitmap.DpiY = 96;
                    bitmap.CopyTo(softwareBitmap);
                }
            }
        }

        private void animatedCanvas_Draw(Microsoft.Graphics.Canvas.UI.Xaml.ICanvasAnimatedControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedDrawEventArgs args)
        {
            if (pixelShaderEffect == null)
            {
                pixelShaderEffect = new PixelShaderEffect(pixelShaderData);
                pixelShaderEffect.Properties["CenterPoint"] = new System.Numerics.Vector2(0.5f, 0.5f);
                pixelShaderEffect.Properties["Ratio"] = 1f;
            }
            const double durationSeconds = 3d;

            var centerPoint = this.centerPoint;
            this.centerPoint = null;
            if (centerPoint.HasValue)
            {
                pixelShaderEffect.Properties["CenterPoint"] = centerPoint.Value;
                startTime = args.Timing.TotalTime;
            }

            var progress = (float)((args.Timing.TotalTime - startTime).TotalSeconds / durationSeconds + 0.12);
            if (startTime.TotalSeconds == 0) progress = 1;

            pixelShaderEffect.Properties["Progress"] = progress;

            using var canvasBitmap = CanvasBitmap.CreateFromSoftwareBitmap(sender.Device, softwareBitmap);

            pixelShaderEffect.Source1 = canvasBitmap;

            args.DrawingSession.DrawImage(
                pixelShaderEffect,
                new Windows.Foundation.Rect(default, sender.Size),
                new Windows.Foundation.Rect(default, canvasBitmap.Size));
        }

        private void animatedCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var pos = e.GetCurrentPoint((UIElement)sender).Position;
            centerPoint = new System.Numerics.Vector2(
                (float)(pos.X / ((FrameworkElement)sender).ActualWidth),
                (float)(pos.Y / ((FrameworkElement)sender).ActualHeight));
        }
    }
}
