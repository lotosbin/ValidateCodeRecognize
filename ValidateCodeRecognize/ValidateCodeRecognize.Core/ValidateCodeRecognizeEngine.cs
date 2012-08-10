// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ValidateCodeRecognizeEngine.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the ValidateCodeRecognizeEngine type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Net;

namespace ValidateCodeRecognize.Core
{
    using System;
    using System.Drawing;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// </summary>
    public class ValidateCodeRecognizeEngine
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidateCodeRecognizeEngine"/> class. 
        /// </summary>
        public ValidateCodeRecognizeEngine()
        {
            this.RecognizeSamples = new List<RecognizeSample>();
        }

        /// <summary>
        /// </summary>
        public List<RecognizeSample> RecognizeSamples { get; set; }

        public string Learn()
        {
            // 
            throw new NotImplementedException(
                );
        }
        public Bitmap GetToRecognizeBitmap()
        {
            var client = new WebClient();
            var fileName = Guid.NewGuid().ToString("N");
            client.DownloadFile("https://cmpay.10086.cn/pmodule/mkm/common/get-image-validator.jsp", fileName);
            var bitmap = (Bitmap)Bitmap.FromFile(fileName);
            return bitmap;
        }
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public List<Bitmap> GetToLearnBitmaps()
        {
            var client = new WebClient();
            var fileName = Guid.NewGuid().ToString("N");
            client.DownloadFile("https://cmpay.10086.cn/pmodule/mkm/common/get-image-validator.jsp", fileName);
            using (var bitmap = (Bitmap)Bitmap.FromFile(fileName))
            {
                bitmap.Save(fileName + "_resave" + ".bmp", ImageFormat.Bmp);
                var splitBitmaps = this.SplitBitmaps(bitmap, 4, 10);

                // debug
                for (int i = 0; i < splitBitmaps.Count; i++)
                {
                    var splitBitmap = splitBitmaps[i];
                    splitBitmap.Save(fileName + "_" + i + ".bmp", ImageFormat.Bmp);
                }

                return splitBitmaps;
            }
        }
        /// <summary>
        /// </summary>
        /// <param name="bitmap">
        /// </param>
        /// <param name="singlecode">
        /// </param>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void Learn(Bitmap bitmap, string singlecode)
        {
            var eigenValue = this.CalculateEigenValue(bitmap);
            var recognizeSample = new RecognizeSample(eigenValue, singlecode);
            this.RecognizeSamples.Add(recognizeSample);
        }

        public string Recognize(Bitmap bitmap, int length, int delta, int deltadifference)
        {

            var bitmaps = this.SplitBitmaps(bitmap, length, delta);

            // 计算特征值 
            return string.Join(string.Empty, bitmaps.Select(newbitmap => this.RecognizeSingle(newbitmap, deltadifference)).ToArray());
        }

        /// <summary>
        /// </summary>
        /// <param name="newbitmap"></param>
        /// <param name="deltadifference"> </param>
        /// <returns></returns>
        public string RecognizeSingle(Bitmap newbitmap, int deltadifference)
        {
            var eigenValue = this.CalculateEigenValue(newbitmap);
            var reco = " ";
            var samples = this.RecognizeSamples.Where(sample => this.Check(eigenValue, sample.EigenValue, deltadifference)).ToList();
            var grouped = from sample in samples
                          group sample by sample.Value
                              into g
                              select new
                              {
                                  value = g.Key,
                                  count = g.Count()
                              };
            var firstsample = grouped.OrderByDescending(g => g.count).FirstOrDefault();
            if (null != firstsample)
            {
                reco = firstsample.value;
            }
            return reco;
        }

        private List<Bitmap> SplitBitmaps(Bitmap bitmap, int length, int delta)
        {
            var unitwidth = bitmap.Width / length;
            var list = new List<Bitmap>();
            for (int i = 0; i < length; i++)
            {
                int start = i != 0 ? unitwidth * i - delta : 0;
                int end = i != length ? unitwidth * (i + 1) + delta : unitwidth;

                var newwidth = end - start;
                var newheight = bitmap.Height;
                var newbitmap = new Bitmap(newwidth, newheight);
                using (var g = Graphics.FromImage(newbitmap))
                {
                    g.DrawImage(bitmap, new Rectangle(0, 0, newwidth, newheight), new Rectangle(start, 0, newwidth, newheight), GraphicsUnit.Pixel);
                }
                list.Add(newbitmap);
            }

            return list;
        }

        /// <summary>
        /// 计算特征值
        /// </summary>
        /// <param name="newbitmap">
        /// The newbitmap.
        /// </param>
        /// <returns>
        /// The System.String.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        private string CalculateEigenValue(Bitmap newbitmap)
        {
            // 缩小尺寸到 8x8a
            var scalewidth = 16;
            var scaleheight = 16;
            var scaledBitmap = new Bitmap(scalewidth, scaleheight);
            using (var g = Graphics.FromImage(scaledBitmap))
            {
                g.DrawImage(newbitmap, 0, 0, scalewidth, scaleheight);
            }

            // 简化色彩
            // 将缩小后的图片，转为64级灰度。也就是说，所有像素点总共只有64种颜色。
            int[] pixels = new int[scalewidth * scaleheight];
            for (int i = 0; i < scalewidth; i++)
            {
                for (int j = 0; j < scaleheight; j++)
                {
                    pixels[i * scalewidth + j] = rgbToGray(scaledBitmap.GetPixel(i, j).ToArgb());
                }
            }

            // 计算平均值
            int m = pixels.Sum();
            m = m / pixels.Length;
            int avgPixel = m;

            // 第四步，比较像素的灰度。  
            // 将每个像素的灰度，与平均值进行比较。大于或等于平均值，记为1；小于平均值，记为0。//
            int[] comps = new int[scalewidth * scaleheight];
            for (int i = 0; i < comps.Length; i++)
            {
                comps[i] = pixels[i] >= avgPixel ? 1 : 0;
            }

            // 第五步，计算哈希值。
            // 将上一步的比较结果，组合在一起，就构成了一个64位的整数，这就是这张图片的指纹。组合的次序并不重要，只要保证所有图片都采用同样次序就行了。
            var hashCode = new StringBuilder();
            for (int i = 0; i < comps.Length; i += 4)
            {
                int result = comps[i] * (int)Math.Pow(2, 3) + comps[i + 1] * (int)Math.Pow(2, 2) + comps[i + 2] * (int)Math.Pow(2, 1) + comps[i + 2];
                hashCode.Append(this.BinaryToHex(result)); // 二进制转为16进制  
            }

            return hashCode.ToString();
        }

        /// <summary>
        /// </summary>
        /// <param name="sourceHashCode"></param>
        /// <param name="hashCode"></param>
        /// <param name="deltadifference"> </param>
        /// <returns></returns>
        public bool Check(string sourceHashCode, string hashCode, int deltadifference)
        {
            //得到指纹以后，就可以对比不同的图片，看看64位中有多少位是不一样的。在理论上，这等同于计算"汉明距离"（Hammingdistance）。如果不相同的数据位不超过5，就说明两张图片很相似；如果大于10，就说明这是两张不同的图片。
            int difference = 0;
            int len = sourceHashCode.Length;

            for (int i = 0; i < len; i++)
            {
                if (sourceHashCode[i] != hashCode[i])
                {
                    difference++;
                }
            }
            return difference < deltadifference;
        }
        private char BinaryToHex(int result)
        {
            return result.ToString("X")[0];
        }

        /**  
         * 灰度值计算  
         * @param pixels 彩色RGB值(Red-Green-Blue 红绿蓝)  
         * @return int 灰度值  
         */
        public static int rgbToGray(int pixels)
        {
            int _alpha = (pixels >> 24) & 0xFF;
            int _red = (pixels >> 16) & 0xFF;
            int _green = (pixels >> 8) & 0xFF;
            int _blue = (pixels) & 0xFF;
            return (int)(0.3 * _red + 0.59 * _green + 0.11 * _blue);
        }
    }
}
