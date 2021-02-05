using System;

using OpenCvSharp;

namespace OpenCVTest
{
    class Program
    {
        static void Main(string[] args)
        {
            using var src = new Mat("lenna.png", ImreadModes.Grayscale);
            using var dst = new Mat();
            
            Cv2.Resize(src, dst, 50, 200);
           
        }
    }
}
