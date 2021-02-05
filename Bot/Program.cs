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

using Vulcan.NET;

namespace Insaniquarium_Deluxe_Bot
{
    class Program
    {
        public const int N = 33 * 1024;
        //X2553 Y220 W64 H402
        //0x2B7177 | 666 378
        //0x2B7177 | 666 426
        //H Dif => 48
        public const int ourscreenLeft = 633;
        public const int ourscreenTop = 463;
        public const int ourscreenWidth = 64;
        public const int ourscreenHeight = 160;
        //public const int blockSizeX = 8;

        public const int screenLeft = 0;
        public const int screenTop = 0;
        public const int screenWidth = 1920;
        public const int screenHeight = 1080;
        private const int keysWidth = 25 * 4; //griddim x
        private const int keysHeight = 6; //griddim y

        private const int pixelPerKeyColumn = (screenWidth) / keysWidth; //blockdim.y
        private const int pixelPerKeyRow = (screenHeight) / keysHeight; //blockdim.y

        public const int blockSizeX = 1024;
        public static dim3 blockSize = new dim3(pixelPerKeyRow, pixelPerKeyColumn);
        public static dim3 gridSize = new dim3(keysWidth, keysHeight);
        private static Stopwatch watch = new Stopwatch();
        public static void Main()
        {
            //Thread.Sleep(3000);

            //Cursor.Position = new Point(screenLeft, screenTop);
            //Thread.Sleep(200);
            //Cursor.Position = new Point(screenLeft + screenWidth, screenTop);
            //Thread.Sleep(200);
            //Cursor.Position = new Point(screenLeft + screenWidth, screenTop + screenHeight);
            //Thread.Sleep(200);
            //Cursor.Position = new Point(screenLeft, screenTop + screenHeight);
            //Thread.Sleep(200);



            CudafyTranslator.GenerateDebug = true;
            CudafyModule km = CudafyTranslator.Cudafy(ePlatform.x64, eArchitecture.OpenCL);//, Version.Parse("9.0"), false, new Type[]{N.GetType()});
            km.GenerateDebug = true;
            GPGPU gpu = CudafyHost.GetDevice(eGPUType.OpenCL, 0);// CudafyModes.Target, CudafyModes.DeviceId);
                                                                 //GPGPU gpu = new OpenCLDevice(2);

            gpu.LoadModule(km);
            double frameTime = 1000d / 500d;
            GPUColorBGRA[] colors = new GPUColorBGRA[] {
                //new GPUColorRGB { Red = 0xAD, Green = 0x4A, Blue =0x00},
                //new GPUColorRGB { Red = 0xCE, Green = 0x73, Blue =0x00},
                //new GPUColorRGB { Red = 0x00, Green = 0xAD, Blue =0xF7},
                //new GPUColorRGB { Red = 0xD2, Green = 0xCC, Blue =0x2B},
                //new GPUColorRGB { Red = 0x8C, Green = 0x7A, Blue =0x8B},
                //new GPUColorRGB { Red = 0xEC, Green = 0x00, Blue =0x00},
                //new GPUColorRGB { Red = 0x39, Green = 0x03, Blue =0x03},
                //new GPUColorRGB { Red = 0x4A, Green = 0x4A, Blue = 0x4A },

                ////Minecraft
                new GPUColorBGRA { Red = 43, Green = 113, Blue = 119 },
                //new GPUColorRGB { Red = 43, Green = 119, Blue = 113 },
                //new GPUColorRGB { Red = 113, Green = 43, Blue = 119 },
                //new GPUColorRGB { Red = 113, Green = 119, Blue = 43 },
                //new GPUColorRGB { Red = 119, Green = 113, Blue = 43 },
                //new GPUColorRGB { Red = 119, Green = 43, Blue = 113 },
                 };

            //var ret = gpu.CopyToDevice(colors);

            float[,,] output = new float[keysWidth, keysHeight, 3];
            //float[] debugOutput = new float[screenWidth * screenHeight];
            float[,] outputs = new float[keysWidth, keysHeight];
            //float[] debugOutputs = new float[debugOutput.Length];
            float[,,] devoutput = gpu.Allocate<float>(keysWidth, keysHeight, 3);
            //float[] deboutput = gpu.Allocate<float>(debugOutput.Length);
            //int[] devindices = gpu.Allocate<int>(blockSize.x);
            List<float> res = new List<float>();
            HashSet<int> often = new HashSet<int>();
            HashSet<int> often2 = new HashSet<int>();
            int[] indices = new int[blockSize.x];
            byte clearOftens = 0;
            int x = 0;
            int y = 0;
            HashSet<int> ys = new HashSet<int>();
            byte b = 0;
            //Thread.Sleep(2000);
            int count = 0;
            Random r = new Random();
            using (VulcanKeyboard keyboard = VulcanKeyboard.Initialize())
            using (var screenShot = new DirectScreenshot(gpu, screenWidth, screenHeight))
            {
                if (keyboard == null)
                {
                    Console.WriteLine("Did not find vulcan!");
                    Console.ReadLine();
                    return;
                }



                while (true)
                {
                    //for (int loops = 0; loops < 150; loops++)
                    //{
                    watch.Restart();
                    (int x, int y) pointerPos = screenShot.Capture();

                    //gpu.CopyToDevice(indices, devindices);
                    //gpu.Set(devindices);
                    //gpu.Launch(gridSize, blockSize).ScaleImageKernel(screenShot.rgbValues, ret, devindices, devoutput, deboutput);
                    gpu.Launch(new dim3(keysWidth / 4, keysHeight / 3), new dim3(4, 3)).ScaleImageKernel(screenShot.rgbValues, screenWidth, screenHeight, devoutput);


                    //Thread.Sleep(500);
                    gpu.CopyFromDevice(devoutput, output);

                    //var bitmap = new Bitmap(100, 16, PixelFormat.Format24bppRgb);
                    //unsafe
                    //{
                    //    var bmpData = bitmap.LockBits(new Rectangle(new Point(), bitmap.Size), ImageLockMode.WriteOnly, bitmap.PixelFormat);
                    //    var ptr = (byte*)bmpData.Scan0;

                    //    for (int y1 = 0; y1 < 16; y1++)
                    //    {
                    //        for (int x1 = 0; x1 < 100; x1++)
                    //        {

                    //            ptr[(y1 * 100 + x1) * 3 + 0] = (byte)output[x1, y1, 2];
                    //            ptr[(y1 * 100 + x1) * 3 + 1] = (byte)output[x1, y1, 1];
                    //            ptr[(y1 * 100 + x1) * 3 + 2] = (byte)output[x1, y1, 0];
                    //        }
                    //    }
                    //    bitmap.UnlockBits(bmpData);
                    //}
                    //bitmap.Save("TestImage.bmp");
                    ;
                    //gpu.CopyFromDevice(deboutput, debugOutput);
                    //gpu.CopyFromDevice(devindices, indices);
                    //var asdq = indices.Select((x, i) => x==0 ? -1 : (i * gridSize.x)).Where(x=>x!=-1).ToList();
                    //Console.Error.WriteLine("Milliseconds Array Sort begin: " + watch.Elapsed.TotalMilliseconds);

                    //for (int i = 0; i < output.Length; i+=gridSize.x)
                    //{
                    //    for (int oz = i; oz < gridSize.x + i; oz++)
                    //    {
                    //        var item = (int)output[oz];
                    //        if (item <= 0)
                    //            break;
                    //        count++;
                    //        //Console.WriteLine(/* LeftClick*/(item % screenWidth, item / screenWidth));
                    //        LeftClick(item % screenWidth, item / screenWidth);
                    //        //Thread.Sleep(333);
                    //    }
                    //}
                    //var zerosCount = debugOutput.Count(z => z != 0);
                    //watch.Restart();
                    //var onesAndOnlys = output.Count(z => z == 0);
                    //outputs = output.Where(f => f != 0).ToArray();
                    //Console.WriteLine(watch.Elapsed + " took where");
                    //watch.Restart();

                    //Array.Sort(output);
                    //output = output.Reverse().ToArray();
                    //Console.WriteLine(watch.Elapsed + " took sort");
                    //watch.Stop();
                    //for (int i = 0; i < outputs.Length/*&& count < 200*/; i++)
                    //{
                    //    var item = (int)outputs[i];
                    //    if (item != 0)
                    //        count++;
                    //    if (Control.IsKeyLocked(Keys.CapsLock))
                    //        break;
                    //    if (item == 0)
                    //        break;
                    //    x = item % screenWidth;
                    //    y = item / screenWidth;
                    //    //if (x == pointerPos.x && y == pointerPos.y)
                    //    //    Console.WriteLine("Pointer in Array");
                    //    //if (often2.Contains(item))
                    //    //    continue;
                    //    //if (often.Contains(item))
                    //    //{
                    //    //    if (!often2.Contains(item))
                    //    //    {
                    //    if (x > ourscreenLeft && x < ourscreenWidth + ourscreenLeft
                    //        && y > ourscreenTop && y < ourscreenTop + ourscreenHeight
                    //        && x == 666
                    //        && false)
                    //    {
                    //        //ys.Add(y);
                    //        //count++;
                    //        //Console.WriteLine($"{x}:{y}");
                    //        LeftClick(item % screenWidth + 243, item / screenWidth + 20); //Insaniquarium
                    //    }
                    //    //else
                    //    {
                    //        ;
                    //    }
                    //    //if ((item % screenWidth) == 26)
                    //    //LeftClick((item % screenWidth) + screenLeft + 243, (item / screenWidth) + 20);
                    //    //often2.Add(item);
                    //    //    }
                    //    //}
                    //    //else
                    //    //{
                    //    //    b++;

                    //    //    LeftClick(item % screenWidth, item / screenWidth);
                    //    //    often.Add(item);
                    //    //}

                    //    count++;
                    //    //LeftClick(item % screenWidth, item / screenWidth);
                    //    //Thread.Sleep(1);


                    //    //break;
                    //    //count++;
                    //    //Console.WriteLine(/* LeftClick*/(item % screenWidth, item / screenWidth));
                    //}
                    //b++;
                    //if (!Control.IsKeyLocked(Keys.CapsLock))
                    //{
                    //    //mouse_event((int)MouseEventFlags.MOUSEEVENTF_WHEEL, 0, 0, -120, 0);
                    //    //Thread.Sleep(50);
                    //}

                    watch.Stop();
                    //Console.WriteLine("Found: " + count);
                    //count = 0;
                    //var l = output.Select((x,i) => (x,i)).Where(x => x.x > 0).Select(x=>((float)x.i) / 2025).ToList();
                    //Console.WriteLine(count + " | " + l.Count);
                    //Console.Write("Found: " +  + " | ");
                    double elapsed = watch.Elapsed.TotalMilliseconds;
                    //Console.Error.WriteLine("Milliseconds: " + watch.Elapsed.TotalMilliseconds);



                    keyboard.SetColor(Color.Green);
                    for (int i = 0; i < 131; i++)
                    {
                        keyboard.SetKeyColor((Key)i, Color.FromArgb(255 << 24 | r.Next(0, 255 << 16)));
                    }

                    keyboard.SetKeyColor(Key.W, Color.FromArgb((byte)output[6, 3, 0], (byte)output[6, 3, 1], (byte)output[6, 3, 2]));
                    keyboard.SetKeyColor(Key.A, Color.FromArgb((byte)output[3, 4, 0], (byte)output[3, 4, 1], (byte)output[3, 4, 2]));
                    keyboard.SetKeyColor(Key.S, Color.FromArgb((byte)output[7, 4, 0], (byte)output[7, 4, 1], (byte)output[7, 4, 2]));
                    keyboard.SetKeyColor(Key.D, Color.FromArgb((byte)output[10, 4, 0], (byte)output[10, 4, 1], (byte)output[10, 4, 2]));

                    keyboard.SetKeyColor(Key.NUM_ENTER, Color.FromArgb((byte)output[99, 5, 0], (byte)output[99, 5, 1], (byte)output[99, 5, 2]));

                    watch.Restart();
                    bool success = keyboard.Update();
                    watch.Stop();
                    //Console.WriteLine("Set colors: " + success + ", took :" + watch.ElapsedMilliseconds + "ms");
                    elapsed += watch.Elapsed.TotalMilliseconds;


                    //Thread.Sleep((int)Math.Max((frameTime - watch.Elapsed.TotalMilliseconds), 0));
                    Thread.Sleep(Math.Min(33, Math.Max(0, 33 - (int)elapsed)));
                    //clearOftens++;
                    //if (clearOftens % 20 == 0)
                    //    often.Clear();
                    //else if (clearOftens == 101)
                    //{
                    //    often.Clear();
                    //    often2.Clear();
                    //    clearOftens = 0;
                    //}
                    //mouse_event((int)MouseEventFlags.MOUSEEVENTF_WHEEL, 0, 0, -120, 0);
                    //}
                    //Console.ReadKey();
                }
            }
            gpu.FreeAll();
            Console.Read();
        }
        [Cudafy]
        public struct GPUColorBGRA
        {
            public byte Blue;
            public byte Green;
            public byte Red;
            public byte Alpha;
        }


        [Cudafy]
        public static void FindPixel(GThread thread, GPUColorBGRA[] rgbColors, GPUColorBGRA[] colors, int[] indices, float[] output)
        {
            //int[] cache = thread.AllocateShared<int>("cache", 1025);
            //thread.SyncThreads();
            //int offset = thread.gridDim.x * thread.threadIdx.x + indices[thread.threadIdx.x]; //2025 * 0 + 0 -> 2025 * 0 + 1
            //thread.SyncThreads();
            //thread.SyncThreadsCount(true);

            //indices[thread.threadIdx.x]++;


            ////float[] cache = thread.AllocateShared<float>("cache", screenWidth * screenHeight / blockSizeX);
            int o = thread.threadIdx.x /*0 bis threadcount*/ + thread.blockDim.x /*1024*/ * thread.blockIdx.x/*+1 in 1024 schritten*/;

            //int offset = thread.gridDim.x * thread.threadIdx.x + indices[thread.threadIdx.x]++; //2025 * 0 + 0 -> 2025 * 0 + 1
            //cache[offset] = -1;
            output[o] = 0;



            for (int i = 0; i < colors.Length; i++)
            {

                //if (rgbColors[o].Red + 5 >= colors[i].Red && rgbColors[o].Red - 5 <= colors[i].Red
                //    && rgbColors[o].Green + 5 >= colors[i].Green && rgbColors[o].Green - 5 <= colors[i].Green
                //    && rgbColors[o].Blue + 5 >= colors[i].Blue && rgbColors[o].Blue - 5 <= colors[i].Blue)
                if (rgbColors[o].Red == colors[i].Red
                && rgbColors[o].Green == colors[i].Green
                && rgbColors[o].Blue == colors[i].Blue)
                {
                    //cache[offset] = o;

                    thread.SyncThreads();
                    output[o] = o;
                    break;
                }
            }
            //indices[thread.threadIdx.x]++;
            //thread.SyncThreads();

            //output[offset] = cache[offset];
        }

        [Cudafy]
        public static void ScaleImageKernel(GThread gThread, GPUColorBGRA[] sourceImage, int sourceWidth,
 int sourceHeight, float[,,] output)
        {
            var scaledX = gThread.blockIdx.x * gThread.blockDim.x + gThread.threadIdx.x;
            var scaledY = gThread.blockIdx.y * gThread.blockDim.y + gThread.threadIdx.y;
            var sourceX = scaledX * sourceWidth / output.GetLength(0);
            var sourceY = scaledY * sourceHeight / output.GetLength(1);
            var index = sourceX + sourceY * sourceWidth;

            output[scaledX, scaledY, 0] = sourceImage[index].Red;
            output[scaledX, scaledY, 1] = sourceImage[index].Green;
            output[scaledX, scaledY, 2] = sourceImage[index].Blue;
        }


        [Cudafy]
        public static void AverageColor(GThread thread, GPUColorBGRA[] screenshotColors, GPUColorBGRA[] colors, int[] indices, float[] output, float[] debugOutput)
        {
            //int[] cache = thread.AllocateShared<int>("cache", 1025);
            //thread.SyncThreads();
            //int offset = thread.gridDim.x * thread.threadIdx.x + indices[thread.threadIdx.x]; //2025 * 0 + 0 -> 2025 * 0 + 1
            //thread.SyncThreads();
            //thread.SyncThreadsCount(true);

            //indices[thread.threadIdx.x]++;
            //(25 * ) + () * *

            //keysWidth = 25 * 4; //griddim x
            //keysHeight = 6 * 4; //griddim y
            //pixelPerKeyColumn = (screenWidth) / keysWidth; //blockdim.y
            //pixelPerKeyRow = (screenHeight) / keysHeight; //blockdim.y

            //int o = (thread.gridDim.x * thread.blockIdx.x + thread.threadIdx.x) +
            //(thread.gridDim.y * thread.blockIdx.y + thread.threadIdx.y) * thread.gridDim.x * thread.blockDim.x;

            ////float[] cache = thread.AllocateShared<float>("cache", screenWidth * screenHeight / blockSizeX);
            int o = thread.threadIdx.x /*0 bis threadcount*/ + thread.blockDim.x /*100*/ * thread.blockIdx.x + thread.blockDim.y * thread.blockIdx.y/*+1 in 1024 schritten*/;

            //threadIdx => Thread im Block (900)
            //BlockIdx => Block im Grid (100,24)

            //int offset = thread.gridDim.x * thread.threadIdx.x + indices[thread.threadIdx.x]++; //2025 * 0 + 0 -> 2025 * 0 + 1
            //cache[offset] = -1;
            //output[o] = 0;
            //float o2;

            var column = o % screenWidth;
            var row = (o / screenWidth);

            var keyColumnIndex = column / pixelPerKeyColumn;
            var keyRowIndex = row / pixelPerKeyRow;

            //debugOutput[o] = keyColumnIndex * keysHeight + keyRowIndex;
            //if (output[0] == 0)
            //    output[0] = thread.gridDim.x;
            //if(output[1] == 0)
            //    output[1] = thread.gridDim.y;
            thread.SyncThreads();

            debugOutput[o] = thread.blockIdx.x;
            //if (debugOutput[o] == 0)
            //{
            //    debugOutput[o] = thread.blockDim.x;
            //    debugOutput[o + 1] = thread.blockDim.y;
            //    debugOutput[o + 2] = thread.gridDim.x;
            //    debugOutput[o + 3] = thread.gridDim.y;
            //    debugOutput[o + 5] = thread.gridDim.y;
            //}
            output[(((keyColumnIndex * keysHeight) + keyRowIndex) * 3) + 0] += (float)screenshotColors[o].Red;
            output[(((keyColumnIndex * keysHeight) + keyRowIndex) * 3) + 1] += (float)screenshotColors[o].Green;
            output[(((keyColumnIndex * keysHeight) + keyRowIndex) * 3) + 2] += (float)screenshotColors[o].Blue;
            //output[thread.gridDim.x * keysHeight + thread.gridDim.y] += 10f;
            thread.SyncThreads();
            //for (int i = 0; i < colors.Length; i++)
            //{

            //    //if (rgbColors[o].Red + 5 >= colors[i].Red && rgbColors[o].Red - 5 <= colors[i].Red
            //    //    && rgbColors[o].Green + 5 >= colors[i].Green && rgbColors[o].Green - 5 <= colors[i].Green
            //    //    && rgbColors[o].Blue + 5 >= colors[i].Blue && rgbColors[o].Blue - 5 <= colors[i].Blue)
            //    if (screenshotColors[o].Red == colors[i].Red
            //    && screenshotColors[o].Green == colors[i].Green
            //    && screenshotColors[o].Blue == colors[i].Blue)
            //    {
            //        //cache[offset] = o;

            //        thread.SyncThreads();
            //        output[o] = o;
            //        break;
            //    }
            //}
            //indices[thread.threadIdx.x]++;
            //thread.SyncThreads();

            //output[offset] = cache[offset];
        }


        public static void LeftClick(int x, int y)
        {
            Cursor.Position = new Point(x, y);
            Thread.Sleep(100);
            Console.WriteLine($"{x}:{y}");
            mouse_event((int)(MouseEventFlags.LEFTDOWN), 0, 0, 0, 0);
            Thread.Sleep(100);
            mouse_event((int)(MouseEventFlags.LEFTUP), 0, 0, 0, 0);
            Thread.Sleep(100);
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
            RIGHTUP = 0x00000010,
            MOUSEEVENTF_WHEEL = 0x0800
        }
        static void QuicksortSequential<T>(T[] arr, int left, int right)
        where T : IComparable<T>
        {
            if (right > left)
            {
                int pivot = Partition(arr, left, right);
                QuicksortSequential(arr, left, pivot - 1);
                QuicksortSequential(arr, pivot + 1, right);
            }
        }
        static void QuicksortParallelOptimised<T>(T[] arr, int left, int right)
        where T : IComparable<T>
        {
            const int SEQUENTIAL_THRESHOLD = 2048;
            if (right > left)
            {
                if (right - left < SEQUENTIAL_THRESHOLD)
                {
                    QuicksortSequential(arr, left, right);
                }
                else
                {
                    int pivot = Partition(arr, left, right);
                    Parallel.Invoke(
                        () => QuicksortParallelOptimised(arr, left, pivot - 1),
                        () => QuicksortParallelOptimised(arr, pivot + 1, right));
                }
            }
        }

        static int Partition<T>(T[] arr, int low, int high) where T : IComparable<T>
        {
            int pivotPos = (high + low) / 2;
            T pivot = arr[pivotPos];
            Swap(arr, low, pivotPos);

            int left = low;
            for (int i = low + 1; i <= high; i++)
            {
                if (arr[i].CompareTo(pivot) < 0)
                {
                    left++;
                    Swap(arr, i, left);
                }
            }

            Swap(arr, low, left);
            return left;
        }

        static void Swap<T>(T[] arr, int i, int j)
        {
            T tmp = arr[i];
            arr[i] = arr[j];
            arr[j] = tmp;
        }
    }
}
