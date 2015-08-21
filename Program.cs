using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Xml;
using System.IO;

namespace GifToGomez {
    public static class GifToGomez {

        public static int SpacingX = 2;
        public static int SpacingY = 2;

        public static void ConvertToGomez(this Bitmap gif, string outputFolder, string outputName) {
            int frames = 0;

            FrameDimension[] frameDimensions = new FrameDimension[gif.FrameDimensionsList.Length];
            for (int i = 0; i < gif.FrameDimensionsList.Length; i++) {
                frameDimensions[i] = new FrameDimension(gif.FrameDimensionsList[i]);
                frames += gif.GetFrameCount(frameDimensions[i]);
            }

            FrameDimension[] frameDimensionsPerFrame = new FrameDimension[frames];
            for (int i = 0; i < gif.FrameDimensionsList.Length;) {
                int subframes = gif.GetFrameCount(frameDimensions[i]);
                for (int ii = 0; ii < subframes; ii++) {
                    frameDimensionsPerFrame[i + ii] = frameDimensions[i];
                }
                i += subframes;
            }

            int columns = frames;
            int rows = 1;
            int width = (gif.Width + SpacingX) * columns;
            int height = (gif.Height + SpacingY) * rows;
            int pngWidth = (int) Math.Pow(2, Math.Ceiling(Math.Log(width, 2)));
            int pngHeight = (int) Math.Pow(2, Math.Ceiling(Math.Log(height, 2)));
            while (pngWidth >= pngHeight) {
                columns--;
                rows = (int) Math.Ceiling((double) frames / columns);
                width = (gif.Width + SpacingX) * columns;
                height = (gif.Height + SpacingY) * rows;
                pngWidth = (int) Math.Pow(2, Math.Ceiling(Math.Log(width, 2)));
                pngHeight = (int) Math.Pow(2, Math.Ceiling(Math.Log(height, 2)));
            }

            Bitmap png = new Bitmap(pngWidth, pngHeight);
            XmlDocument xml = new XmlDocument();
            XmlElement xmlAnimatedTexture = xml.CreateElement("AnimatedTexturePC");
            xmlAnimatedTexture.SetAttribute("width", width.ToString());
            xmlAnimatedTexture.SetAttribute("height", height.ToString());
            xmlAnimatedTexture.SetAttribute("actualWidth", gif.Width.ToString());
            xmlAnimatedTexture.SetAttribute("actualHeight", gif.Height.ToString());
            xml.AppendChild(xmlAnimatedTexture);
            XmlElement xmlFrames = xml.CreateElement("Frames");
            xmlAnimatedTexture.AppendChild(xmlFrames);

            //TODO do something faster than getting / setting pixel...
            int xo = 0;
            int yo = 0;
            for (int i = 0; i < frames; i++) {
                gif.SelectActiveFrame(frameDimensionsPerFrame[i], i);

                //Console.WriteLine("xo " + xo + "; yo " + yo + "; w " + width + "; h " + height);
                for (int y = 0; y < gif.Height; y++) {
                    for (int x = 0; x < gif.Width; x++) {
                        //Console.WriteLine("x " + (x + xo) + "; y " + (y + yo) + "; w " + width + "; h " + height);
                        png.SetPixel(x + xo, y + yo, gif.GetPixel(x, y));
                    }
                }

                byte[] frameDelays = gif.GetPropertyItem(0x5100).Value;

                XmlElement xmlFrame = xml.CreateElement("FramePC");
                xmlFrame.SetAttribute("duration", (TimeSpan.TicksPerSecond * (BitConverter.ToInt32(frameDelays, 0) / 100D)).ToString());
                xmlFrames.AppendChild(xmlFrame);
                XmlElement xmlRectangle = xml.CreateElement("Rectangle");
                xmlRectangle.SetAttribute("x", xo.ToString());
                xmlRectangle.SetAttribute("y", yo.ToString());
                xmlRectangle.SetAttribute("w", gif.Width.ToString());
                xmlRectangle.SetAttribute("h", gif.Height.ToString());
                xmlFrame.AppendChild(xmlRectangle);

                xo += gif.Width + SpacingX;
                if (xo >= width) {
                    xo = 0;
                    yo += gif.Height + SpacingY;
                }
            }

            string outputPng = Path.Combine(outputFolder, outputName + ".ani.png");
            if (File.Exists(outputPng)) {
                File.Delete(outputPng);
            }
            png.Save(outputPng);

            png.Dispose();

            string outputXml = Path.Combine(outputFolder, outputName + ".xml");
            if (File.Exists(outputXml)) {
                File.Delete(outputXml);
            }
            using (FileStream fos = new FileStream(outputXml, FileMode.CreateNew)) {
                XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
                xmlWriterSettings.Indent = true;
                using (XmlWriter xmlWriter = XmlWriter.Create(fos, xmlWriterSettings)) {
                    xml.Save(xmlWriter);
                }
            }
        }

        public static void Main(string[] args) {
            if (args.Length != 2) {
                Console.WriteLine("GifToGomez requires two parameter: the path to the gif and the output animation name.");
                Console.WriteLine("Example: GifToGomez.exe stetik.gif walk");
                Console.WriteLine("Note: All frames must contain the complete frame with each frame having the same size.");
                return;
            }
            Bitmap gif = (Bitmap) Image.FromFile(args[0]);
            ConvertToGomez(gif, ".", args[1]);
            gif.Dispose();
        }
    }
}
