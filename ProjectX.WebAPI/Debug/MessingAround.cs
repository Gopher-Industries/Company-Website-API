using ImageMagick;
using Newtonsoft.Json.Schema;
using System.Diagnostics;

namespace ProjectX.WebAPI.Debug
{
    public class MessingAround
    {

        public static void Run()
        {

            var imgsource = File.ReadAllBytes("C:\\Users\\Nathan\\Downloads\\surprising-flower-meanings-balloon-flowers-1650767465.jpg");

            //foreach (var Method in Enum.GetValues(typeof(CompressionMethod)))
            {

                var timer = Stopwatch.StartNew();

                using var image = new MagickImage(imgsource);

                image.Format = MagickFormat.Jpg; // Get or Set the format of the image.
                                                 //image.Resize(500, 500); // fit the image into the requested width and height. 
                image.Quality = 50; // This is the Compression level.
                                    //image.AdaptiveSharpen();
                                    //image.SetCompression((CompressionMethod)Method);
                image.SetCompression(CompressionMethod.LZMA);
                var compress = image.ToByteArray();
                var compressStringBase64 = image.ToBase64(MagickFormat.Jpg);
                image.Write(@"C:\Users\Nathan\Downloads\Compressed.jpg");


                var st = image.ToString();

                var compressTIme = timer.ElapsedMilliseconds;

                var Diff = compress.Length - imgsource.Length;

                //Console.WriteLine($"{ Method }: { compressTIme }ms saving { Diff } bytes.");

            }

            return;
        }

    }
}
