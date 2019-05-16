using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Bootstrap.Infrastructures.Models.ResponseModels;
using Microsoft.AspNetCore.Identity;

namespace Bootstrap.Business.Components.Services.Captcha.Generators
{
    public class DefaultImageCaptchaGenerator : IImageCaptchaGenerator
    {
        private readonly Random _rand = new Random();

        private int _getFontSize(int imageWidth, int captchaCodeCount)
        {
            var averageSize = imageWidth / captchaCodeCount;

            return Convert.ToInt32(averageSize);
        }

        private Color _getRandomDeepColor()
        {
            int redLow = 160, greenLow = 100, blueLow = 160;
            return Color.FromArgb(_rand.Next(redLow), _rand.Next(greenLow), _rand.Next(blueLow));
        }

        private Color _getRandomLightColor()
        {
            int low = 180, high = 255;

            var nRend = _rand.Next(high) % (high - low) + low;
            var nGreen = _rand.Next(high) % (high - low) + low;
            var nBlue = _rand.Next(high) % (high - low) + low;

            return Color.FromArgb(nRend, nGreen, nBlue);
        }

        private void _drawCaptchaCode(Graphics graph, string code, int width, int height)
        {
            var fontBrush = new SolidBrush(Color.Black);
            var fontSize = _getFontSize(width, code.Length);
            var font = new Font(FontFamily.GenericSerif, fontSize, FontStyle.Bold, GraphicsUnit.Pixel);
            for (var i = 0; i < code.Length; i++)
            {
                fontBrush.Color = _getRandomDeepColor();

                var shiftPx = fontSize / 6;

                float x = i * fontSize + _rand.Next(-shiftPx, shiftPx) + _rand.Next(-shiftPx, shiftPx);
                var maxY = height - fontSize;
                if (maxY < 0) maxY = 0;
                float y = _rand.Next(0, maxY);

                graph.DrawString(code[i].ToString(), font, fontBrush, x, y);
            }
        }

        private void _drawDisorderLine(Graphics graph, int width, int height)
        {
            var linePen = new Pen(new SolidBrush(Color.Black), 3);
            for (var i = 0; i < _rand.Next(3, 5); i++)
            {
                linePen.Color = _getRandomDeepColor();

                var startPoint = new Point(_rand.Next(0, width), _rand.Next(0, height));
                var endPoint = new Point(_rand.Next(0, width), _rand.Next(0, height));
                graph.DrawLine(linePen, startPoint, endPoint);

                var bezierPoint1 = new Point(_rand.Next(0, width), _rand.Next(0, height));
                var bezierPoint2 = new Point(_rand.Next(0, width), _rand.Next(0, height));

                graph.DrawBezier(linePen, startPoint, bezierPoint1, bezierPoint2, endPoint);
            }
        }

        private void _adjustRippleEffect(Bitmap baseMap)
        {
            short nWave = 6;
            var nWidth = baseMap.Width;
            var nHeight = baseMap.Height;

            var pt = new Point[nWidth, nHeight];

            for (var x = 0; x < nWidth; ++x)
            {
                for (var y = 0; y < nHeight; ++y)
                {
                    var xo = nWave * Math.Sin(2.0 * 3.1415 * y / 128.0);
                    var yo = nWave * Math.Cos(2.0 * 3.1415 * x / 128.0);

                    var newX = x + xo;
                    var newY = y + yo;

                    if (newX > 0 && newX < nWidth)
                    {
                        pt[x, y].X = (int) newX;
                    }
                    else
                    {
                        pt[x, y].X = 0;
                    }


                    if (newY > 0 && newY < nHeight)
                    {
                        pt[x, y].Y = (int) newY;
                    }
                    else
                    {
                        pt[x, y].Y = 0;
                    }
                }
            }

            var bSrc = (Bitmap) baseMap.Clone();

            var bitmapData = baseMap.LockBits(new Rectangle(0, 0, baseMap.Width, baseMap.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            var bmSrc = bSrc.LockBits(new Rectangle(0, 0, bSrc.Width, bSrc.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            var scanLine = bitmapData.Stride;

            var scan0 = bitmapData.Scan0;
            var srcScan0 = bmSrc.Scan0;

            unsafe
            {
                var p = (byte*) (void*) scan0;
                var pSrc = (byte*) (void*) srcScan0;

                var nOffset = bitmapData.Stride - baseMap.Width * 3;

                for (var y = 0; y < nHeight; ++y)
                {
                    for (var x = 0; x < nWidth; ++x)
                    {
                        var xOffset = pt[x, y].X;
                        var yOffset = pt[x, y].Y;

                        if (yOffset >= 0 && yOffset < nHeight && xOffset >= 0 && xOffset < nWidth)
                        {
                            p[0] = pSrc[yOffset * scanLine + xOffset * 3];
                            p[1] = pSrc[yOffset * scanLine + xOffset * 3 + 1];
                            p[2] = pSrc[yOffset * scanLine + xOffset * 3 + 2];
                        }

                        p += 3;
                    }

                    p += nOffset;
                }
            }

            baseMap.UnlockBits(bitmapData);
            bSrc.UnlockBits(bmSrc);
            bSrc.Dispose();
        }

        public Task<SingletonResponse<Stream>> Generate(string captchaCode, int width = 104, int height = 36)
        {
            using (var baseMap = new Bitmap(width, height))
            {
                using (var graph = Graphics.FromImage(baseMap))
                {
                    graph.Clear(_getRandomLightColor());

                    _drawCaptchaCode(graph, captchaCode, width, height);

                    _drawDisorderLine(graph, width, height);

                    _adjustRippleEffect(baseMap);

                    var ms = new MemoryStream();

                    baseMap.Save(ms, ImageFormat.Png);

                    return Task.FromResult(new SingletonResponse<Stream>(ms));
                }
            }
        }
    }
}