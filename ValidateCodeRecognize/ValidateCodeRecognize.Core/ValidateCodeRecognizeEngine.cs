using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace ValidateCodeRecognize.Core
{
    public class ValidateCodeRecognizeEngine
    {
        public string Learn(Bitmap bitmap, string code)
        {
            throw new NotImplementedException();
        }
        public string Recognize(Bitmap bitmap, int length, int delta)
        {
            //split

            var unitwidth = bitmap.Width / length;
            for (int i = 0; i < length; i++)
            {
                int start = i != 0 ? length * i - delta : 0;
                int end = i != length ? length * (i + 1) + delta : length;

                var newwidth = end - start;
                using (var newbitmap = new Bitmap(newwidth, bitmap.Height))
                {
                    using (var g = Graphics.FromImage(bitmap))
                    {
                        g.Clear(Color.Transparent);
                        g.DrawImage(newbitmap, new Rectangle(0, 0, newwidth, bitmap.Height), new Rectangle(start, 0, newwidth, bitmap.Height), GraphicsUnit.Pixel);
                    }
                    //计算特征值
                    var eigenValue = CalculateEigenValue(newbitmap);
                }
            }
            throw new NotImplementedException();
        }

        private string CalculateEigenValue(Bitmap newbitmap)
        {
            //缩小尺寸到 8x8
            var scaledBitmap = new Bitmap(8, 8);
            using (var g = Graphics.FromImage(newbitmap))
            {
                g.DrawImage(scaledBitmap, 0, 0, 8, 8);
            }
            //简化色彩
            //将缩小后的图片，转为64级灰度。也就是说，所有像素点总共只有64种颜色。
            int[] pixels = new int[8 * 8];
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    pixels[i * 8 + j] = rgbToGray(scaledBitmap.GetPixel(i, j).ToArgb());
                }
            }
            //计算平均值
            int m = pixels.Sum();
            m = m / pixels.Length;
            int avgPixel = m;
            //第四步，比较像素的灰度。
            //将每个像素的灰度，与平均值进行比较。大于或等于平均值，记为1；小于平均值，记为0。//
            int[] comps = new int[8 * 8];
            for (int i = 0; i < comps.Length; i++)
            {
                if (pixels[i] >= avgPixel)
                {
                    comps[i] = 1;
                }
                else
                {
                    comps[i] = 0;
                }
            }
            //第五步，计算哈希值。
            //将上一步的比较结果，组合在一起，就构成了一个64位的整数，这就是这张图片的指纹。组合的次序并不重要，只要保证所有图片都采用同样次序就行了。
            var hashCode = new StringBuilder();
            for (int i = 0; i < comps.Length; i += 4)
            {
                int result = comps[i] * (int)Math.Pow(2, 3) + comps[i + 1] * (int)Math.Pow(2, 2) + comps[i + 2] * (int)Math.Pow(2, 1) + comps[i + 2];
                hashCode.Append(BinaryToHex(result));//二进制转为16进制  
            }
            String sourceHashCode = hashCode.ToString();
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
            //end
            throw new NotImplementedException();
        }

        private char BinaryToHex(int result)
        {
            throw new NotImplementedException();
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
