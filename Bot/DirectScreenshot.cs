using System;
using System.Drawing.Imaging;
using System.IO;

using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using System.Drawing;
using static Insaniquarium_Deluxe_Bot.Program;
using Cudafy.Host;

namespace Bot
{
    /// <summary>
    ///   Screen capture of the desktop using DXGI OutputDuplication.
    /// </summary>
    internal class DirectScreenshot : IDisposable
    {
        private Texture2DDescription textureDesc;
        private Texture2D screenTexture;
        private OutputDuplication duplicatedOutput;
        private Factory1 factory;
        private Adapter1 adapter;
        private Device device;
        private Output output;
        private Output1 output1;
        private int width;
        private int height;

        private GPGPU _gpu;
        public GPUColorRGB[] rgbValues;


        public DirectScreenshot(GPGPU gpu, int screenWidth, int screenHeight)
        {
            const int numAdapter = 0;
            const int numOutput = 0;

            _gpu = gpu;
            rgbValues = gpu.Allocate<GPUColorRGB>(screenWidth * screenHeight);
            // Create DXGI Factory1
            factory = new Factory1();
            adapter = factory.GetAdapter1(numAdapter);

            // Create device from Adapter
            device = new Device(adapter);

            // Get DXGI.Output
            output = adapter.GetOutput(numOutput);
            output1 = output.QueryInterface<Output1>();

            // Width/Height of desktop to capture
            width = screenWidth;
            height = screenHeight;



            // Create Staging texture CPU-accessible
            textureDesc = new Texture2DDescription
            {
                CpuAccessFlags = CpuAccessFlags.Read,
                BindFlags = BindFlags.None,
                Format = Format.B8G8R8A8_UNorm,
                Width = width,
                Height = height,
                OptionFlags = ResourceOptionFlags.None,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = { Count = 1, Quality = 0 },
                Usage = ResourceUsage.Staging
            };
            screenTexture = new Texture2D(device, textureDesc);

            // Duplicate the output
            duplicatedOutput = output1.DuplicateOutput(device);

            var screenResource = BeginCapture();
            screenResource.Dispose();
            duplicatedOutput.ReleaseFrame();
        }

        public void Dispose()
        {
        }

        private SharpDX.DXGI.Resource BeginCapture()
        {

            OutputDuplicateFrameInformation duplicateFrameInformation;

            // Try to get duplicated frame within given time
            duplicatedOutput.AcquireNextFrame(10000, out duplicateFrameInformation, out var screenResource);
            return screenResource;
        }
        public void Capture()
        {
            const string outputFileName = "ScreenCapture.bmp";

            while (true)
            {
                try
                {
                    SharpDX.DXGI.Resource screenResource = BeginCapture();

                    // copy resource into memory that can be accessed by the CPU
                    using (var screenTexture2D = screenResource.QueryInterface<Texture2D>())
                        device.ImmediateContext.CopyResource(screenTexture2D, screenTexture);

                    // Get the desktop capture texture
                    var mapSource = device.ImmediateContext.MapSubresource(screenTexture, 0, MapMode.Read, MapFlags.None);
                    

                    // Copy pixels from screen capture Texture to GDI bitmap
                    var sourcePtr = mapSource.DataPointer;

                    _gpu.CopyToDevice(sourcePtr, 0, rgbValues, 0, width * height);
                    device.ImmediateContext.UnmapSubresource(screenTexture, 0);

                    // Save the output
                    //bitmap.Save(outputFileName);

                    // Capture done

                    screenResource.Dispose();
                    duplicatedOutput.ReleaseFrame();
                    return;
                }
                catch (SharpDXException e)
                {
                    if (e.ResultCode.Code != SharpDX.DXGI.ResultCode.WaitTimeout.Result.Code)
                    {
                        throw e;
                    }
                }

            }

        }

    }
}
