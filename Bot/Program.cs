using Bot;
using Cudafy;
using Cudafy.Host;
using Cudafy.Translator;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Insaniquarium_Deluxe_Bot
{
    class Program
    {
        public const int N = 33 * 1024;
        public const int screenLeft = 0;
        public const int screenTop = 0;
        public const int screenWidth = 1920;
        public const int screenHeight = 1080;
        public static dim3 blockSize = new dim3(1024);
        public static dim3 gridSize = new dim3(screenWidth * screenHeight / blockSize.x);
        private static Stopwatch watch = new Stopwatch();
        public static void Main()
        {
            CudafyModule km = CudafyTranslator.Cudafy(ePlatform.x64, eArchitecture.OpenCL12);
            GPGPU gpu = new OpenCLDevice(2);
            gpu.LoadModule(km);
            double frameTime = 1000d / 60d;
            GPUColorRGB[] colors = new GPUColorRGB[] {
                new GPUColorRGB { Red = 0xAD, Green = 0x4A, Blue =0x00},
                new GPUColorRGB { Red = 0xCE, Green = 0x73, Blue =0x00},
                new GPUColorRGB { Red = 0x00, Green = 0xAD, Blue =0xF7},
                new GPUColorRGB { Red = 0x4A, Green = 0x4A, Blue = 0x4A },
                 };

            var ret = gpu.CopyToDevice(colors);

            float[] output = new float[gridSize.x *  blockSize.x];
            float[] devoutput = gpu.Allocate<float>(output.Length);
            int[] devindices = gpu.Allocate<int>(blockSize.x);
            List<float> res = new List<float>();
            Console.ReadKey();
            int[] indices = new int[blockSize.x];

            using (var screenShot = new DirectScreenshot(gpu, screenWidth, screenHeight))
            {
                while (true)
                {
                    watch.Restart();
                    screenShot.Capture();

                    //gpu.CopyToDevice(indices, devindices);
                    gpu.Set(devindices);
                    gpu.Launch(gridSize, blockSize).FindPixel(screenShot.rgbValues, ret, devindices, devoutput);

                    gpu.CopyFromDevice(devoutput, output);
                    gpu.CopyFromDevice(devindices, indices);

                    var asdq = indices.Select((x, i) => x==0 ? -1 : (i * gridSize.x)).Where(x=>x!=-1).ToList();

                    int count = 0;
                    for (int i = 0; i < output.Length; i+=gridSize.x)
                    {
                        for (int oz = i; oz < gridSize.x + i; oz++)
                        {
                            var item = (int)output[oz];
                            if (item < 0)
                                break;
                            count++;
                            Console.WriteLine(/* LeftClick*/(item % screenWidth, item / screenWidth));
                            //LeftClick(item % screenWidth, item / screenWidth);
                            //Thread.Sleep(333);
                        }
                    }
                    var l = output.Select((x,i) => (x,i)).Where(x => x.x >0).Select(x=>((float)x.i) / 2025).ToList();

                    //Console.Write("Found: " +  + " | ");
                    watch.Stop();
                    Console.Error.WriteLine("Milliseconds: " + watch.Elapsed.TotalMilliseconds);
                    Thread.Sleep((int)Math.Max((frameTime - watch.Elapsed.TotalMilliseconds), 0));
                    Console.ReadKey();
                }
            }
            gpu.FreeAll();
            Console.Read();
        }
        [Cudafy]
        public struct GPUColorRGB
        {
            public byte Blue;
            public byte Green;
            public byte Red;
            public byte Alpha;
        }


        [Cudafy]
        public static void FindPixel(GThread thread, GPUColorRGB[] rgbColors, GPUColorRGB[] colors, int[] indices, float[] output)
        {
            int o = thread.threadIdx.x /*0 bis threadcount*/ + thread.blockDim.x /*1024*/ * thread.blockIdx.x/*+1 in 1024 schritten*/;

            int offset = thread.gridDim.x * thread.threadIdx.x + indices[thread.threadIdx.x]; //2025 * 0 + 0 -> 2025 * 0 + 1
            output[offset] = -thread.threadIdx.x - 1;
            for (int i = 0; i < colors.Length; i++)
                if (rgbColors[o].Red == colors[i].Red && rgbColors[o].Green == colors[i].Green && rgbColors[o].Blue == colors[i].Blue)
                {
                    output[offset] = o;
                    indices[thread.threadIdx.x]++;
                    break;
                }
        }

        public static void LeftClick(int x, int y)
        {
            Cursor.Position = new Point(x, y);
            mouse_event((int)(MouseEventFlags.LEFTDOWN), 0, 0, 0, 0);
            mouse_event((int)(MouseEventFlags.LEFTUP), 0, 0, 0, 0);
        }

        [DllImport("user32.dll")]
        static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);
        [Flags]
        public enum MouseEventFlags
        {
            LEFTDOWN = 0x00000002,
            LEFTUP = 0x00000004,
            MIDDLEDOWN = 0x00000020,
            MIDDLEUP = 0x00000040,
            MOVE = 0x00000001,
            ABSOLUTE = 0x00008000,
            RIGHTDOWN = 0x00000008,
            RIGHTUP = 0x00000010
        }

    }
}
