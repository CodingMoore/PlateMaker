using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.UI;
using Emgu.CV.Structure;
using System.Drawing;
using System.Windows.Forms;
using Emgu.CV.Features2D;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;


namespace PlateScanner
{
    class Plate
    {
        public string FileName { get; set; }
        public FileInfo SvgFilePath { get; set; }
        public FileInfo HtmlFilePath { get; set; }
        public Image<Bgr, byte> PlateImage { get; set; }
        public Mat OutputMat { get; set; }
        public int XGreenDot { get; set; }
        public int YGreenDot { get; set; }
        public int XPlateCenter { get; set; }
        public int YPlateCenter { get; set; }
        public double RotationAngle { get; set; }
        public int XImageCenter { get; set; }
        public int YImageCenter { get; set; }

        public Plate(string file, string fileSavingDirectoryStart)
        {
            // Extracts the original Image file name from the full file path, and removes the file extension.
            // This will be used later when we save each image's "index.html" file in its own respective folder
            FileName = Path.GetFileNameWithoutExtension(file);

            // Defines the full directory path for saving the file
            SvgFilePath = new FileInfo($"{fileSavingDirectoryStart}svg Files\\");

            // Defines the full directory path for saving the file
            HtmlFilePath = new FileInfo($"{fileSavingDirectoryStart}html Files\\");

            PlateImage = new Image<Bgr, Byte>(file);
            OutputMat = null;
            XGreenDot = 0;
            YGreenDot = 0;
            XPlateCenter = 0;
            YPlateCenter = 0;
            RotationAngle = 0;
            XImageCenter = 0;
            YImageCenter = 0;
        }

        public Image<Bgr, byte> NoiseSuppression()
        {
            Image<Bgr, byte> erodedImage = this.PlateImage.Erode(3);
            return erodedImage; //.SmoothGaussian(9);
        }

        public void GetPlateCenter(Image<Bgr, byte> smoothedPlateImage)
        {
            // Working with image altered in Affinity
            Bgr redDotLowerLimit = new Bgr(0, 0, 250);
            Bgr redDotUpperLimit = new Bgr(5, 5, 255);

            // for working with unaltered image
            //Bgr redDotLowerLimit = new Bgr(0, 0, 40);
            //Bgr redDotUpperLimit = new Bgr(8, 8, 255);

            // convert to greyscale image, bounded by red color limits
            Image<Gray, byte> redDotPlateImage = smoothedPlateImage.InRange(redDotLowerLimit, redDotUpperLimit);

            // find center point
            VectorOfVectorOfPoint centerPointContours = new VectorOfVectorOfPoint();
            Mat centerPointMat = new Mat();
            CvInvoke.FindContours(redDotPlateImage, centerPointContours, centerPointMat, RetrType.External, ChainApproxMethod.ChainApproxSimple);

            for (int i = 0; i < centerPointContours.Size; i++)
            {
                double centerPointPerimeter = CvInvoke.ArcLength(centerPointContours[i], true);
                VectorOfPoint centerPointApprox = new VectorOfPoint();
                CvInvoke.ApproxPolyDP(centerPointContours[i], centerPointApprox, 0.04 * centerPointPerimeter, true);
                CvInvoke.DrawContours(this.PlateImage, centerPointContours, i, new MCvScalar(255, 0, 255), 2);

                var moments = CvInvoke.Moments(centerPointContours[i]);
                this.XPlateCenter = (int)(moments.M10 / moments.M00);
                this.YPlateCenter = (int)(moments.M01 / moments.M00);

                CvInvoke.Line(redDotPlateImage, new Point(this.XPlateCenter, this.YPlateCenter), new Point(this.XPlateCenter, this.YPlateCenter), new MCvScalar(0, 0, 255), 5, LineType.EightConnected, 0);
            }

            ImageViewer centerDotViewer = new ImageViewer(redDotPlateImage);
            centerDotViewer.Image = redDotPlateImage;
            //centerDotViewer.ShowDialog();  // Turn on for Debug

        }

        public void GetPlateTopPoint(Image<Bgr, byte> smoothedPlateImage)
        {
            // Find second rotaion point coordinates

            // Working with image altered in Affinity
            Bgr greenDotLowerLimit = new Bgr(0, 250, 0);
            Bgr greenDotUpperLimit = new Bgr(5, 255, 5);

            // Working with unaltered image
            //Bgr greenDotLowerLimit = new Bgr(0, 35, 0);
            //Bgr greenDotUpperLimit = new Bgr(5, 255, 15);

            Image<Gray, byte> greenDotPlateImage = smoothedPlateImage.InRange(greenDotLowerLimit, greenDotUpperLimit);

            // find center point
            VectorOfVectorOfPoint greenDotPointContours = new VectorOfVectorOfPoint();
            Mat greenDotPointMat = new Mat();
            CvInvoke.FindContours(greenDotPlateImage, greenDotPointContours, greenDotPointMat, RetrType.External, ChainApproxMethod.ChainApproxSimple);

            for (int i = 0; i < greenDotPointContours.Size; i++)
            {
                double greenDotPointPerimeter = CvInvoke.ArcLength(greenDotPointContours[i], true);
                VectorOfPoint greenDotPointApprox = new VectorOfPoint();
                CvInvoke.ApproxPolyDP(greenDotPointContours[i], greenDotPointApprox, 0.04 * greenDotPointPerimeter, true);
                CvInvoke.DrawContours(this.PlateImage, greenDotPointContours, i, new MCvScalar(255, 0, 255), 2);

                var moments = CvInvoke.Moments(greenDotPointContours[i]);
                this.XGreenDot = (int)(moments.M10 / moments.M00);
                this.YGreenDot = (int)(moments.M01 / moments.M00);

                CvInvoke.Line(greenDotPlateImage, new Point(this.XGreenDot, this.YGreenDot), new Point(this.XGreenDot, this.YGreenDot), new MCvScalar(0, 0, 255), 5, LineType.EightConnected, 0);
            }

            ImageViewer greenDotViewer = new ImageViewer(smoothedPlateImage);
            greenDotViewer.Image = greenDotPlateImage;
            //greenDotViewer.ShowDialog();  // Turn on for Debug

        }

        public Image<Bgr, byte> TranslatePlateCenter(Image<Bgr, byte> smoothedPlateImage)
        {
            // translate image to match centerDot to image center

            this.XImageCenter = this.PlateImage.Width / 2;
            this.YImageCenter = this.PlateImage.Height / 2;

            int xDistance = this.XPlateCenter - this.XImageCenter;
            int yDistance = this.YPlateCenter - this.YImageCenter;

            PointF[] sourcePoints = new PointF[]
            {
                new PointF(this.XPlateCenter,this.YPlateCenter),
                new PointF(1, 0),
                new PointF(0, 1)

            };

            PointF[] destinationPoints = new PointF[]
            {
                new PointF(this.XImageCenter, this.YImageCenter),
                new PointF(1 - xDistance, 0 - yDistance),
                new PointF(0 - xDistance, 1 - yDistance)

            };

            // Gets TranslationMap for ues in WarpAffine function.
            Mat translationMap = CvInvoke.GetAffineTransform(sourcePoints, destinationPoints);

            // Make the translation move
            CvInvoke.WarpAffine(this.PlateImage, smoothedPlateImage, translationMap, new Size(this.PlateImage.Width, this.PlateImage.Height)); //Check source and destination and return on this.

            ImageViewer translationViewer = new ImageViewer(smoothedPlateImage);
            translationViewer.Image = smoothedPlateImage;
            //translationViewer.ShowDialog();  // Turn on for Debug

            return smoothedPlateImage;
        }

        public Image<Bgr, byte> RotatePlatetoVertical(Image<Bgr, byte> translatedPlateImage)
        {

            bool leftSide = false;
            bool topSide = false;

            // Quadrant checks
            leftSide = (this.XPlateCenter - this.XGreenDot) > 0 ? true : false;
            topSide = (this.YPlateCenter - this.YGreenDot) > 0 ? true : false;

            if (leftSide && topSide) // Top Left quadrant
            {
                this.RotationAngle = (180 / Math.PI) * Math.Atan2((this.XPlateCenter - this.XGreenDot), (this.YPlateCenter - this.YGreenDot));
            }
            else if (!leftSide && topSide) // Top Right quadrant
            {
                this.RotationAngle = -(180 / Math.PI) * Math.Atan2((this.XGreenDot - this.XPlateCenter), (this.YPlateCenter - this.YGreenDot));
            }
            else if (leftSide && !topSide) // Bottom Left quadrant
            {
                this.RotationAngle = 180 - ((180 / Math.PI) * Math.Atan2((this.XPlateCenter - this.XGreenDot), (this.YGreenDot - this.YPlateCenter)));
            }
            else // Bottom Right Quadrant
            {
                this.RotationAngle = -(180 - (180 / Math.PI) * Math.Atan2((this.XGreenDot - this.XPlateCenter), (this.YGreenDot - this.YPlateCenter)));
            }

            var rotatedPlateImage = translatedPlateImage.Rotate(this.RotationAngle, new Bgr(150, 150, 150), true);

            ImageViewer rotationViewer = new ImageViewer(rotatedPlateImage);
            rotationViewer.Image = rotatedPlateImage;
            //rotationViewer.ShowDialog();  // Turn on for Debug

            return rotatedPlateImage;
        }

        public Image<Gray, byte> MakeImageBinary(Image<Bgr, byte> rotatedPlateImage)
        {
            // converts to binary image
            Image<Gray, byte> binaryPlateImage = rotatedPlateImage.Convert<Gray, byte>().ThresholdBinary(new Gray(240), new Gray(255));
            // removes noise by eroding the outside edge
            Image<Gray, byte> erodedImage = binaryPlateImage.Erode(1);
            // fills in missing gaps created by errode and sligly increase contour size
            Image<Gray, byte> dilatedImage = erodedImage.Dilate(1);

            return dilatedImage;
        }

        public List<int[]> FindContoursCenterAndRadius(Image<Gray, byte> binaryPlateImage)
        {
            // creates new overlay mask
            Mat forgroundMat = new Mat();
            Mat outputMat = new Mat(binaryPlateImage.Size, DepthType.Cv8U, 1);

            this.OutputMat = outputMat;

            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();

            // Finds the contour points.  Since we are using a simple version, it will discard unnecessary points.  For example, a square will only have 4 poits.
            CvInvoke.FindContours(binaryPlateImage, contours, forgroundMat, RetrType.External, ChainApproxMethod.ChainApproxSimple);

            // Create list to store center and radius values
            List<int[]> centerAndRadius = new List<int[]>();

            for (int i = 0; i < contours.Size; i++)
            {
                //Find subset of points that will aproxamately maintain the shape of the current contrours. (not sure this has any use currently)
                //double perimeter = CvInvoke.ArcLength(contours[i], true);

                //Something to store the below approxamation in (not sure this has any use currently)
                //VectorOfPoint approx = new VectorOfPoint();

                // Fine tune how approxomate the shape needs to be to match (not sure this has any use currently)
                //CvInvoke.ApproxPolyDP(contours[i], approx, 0.04 * perimeter, true);

                // Draw new contours on original image
                //CvInvoke.DrawContours(binaryPlateImage, contours, i, new MCvScalar(255, 0, 255), 2);

                // moments (center of the shape), getting x/y coordinates
                var moments = CvInvoke.Moments(contours[i]);
                int x = (int)(moments.M10 / moments.M00);
                int y = (int)(moments.M01 / moments.M00);

                // creates a dot (zero length line) at the shape's centroid
                //CvInvoke.Line(binaryPlateImage, new Point(x, y), new Point(x, y), new MCvScalar(100, 100, 100), 1, LineType.EightConnected, 0);

                // Gets dimensions of contour
                Rectangle boundingRectangle = CvInvoke.BoundingRectangle(contours[i]);
                int contourHeight = boundingRectangle.Height;
                int contourWidth = boundingRectangle.Width;
                double contourArea = CvInvoke.ContourArea(contours[i]);

                int diameter = contourHeight > contourWidth ? contourHeight : contourWidth;
                int radius = (diameter / 2) + 1;


                //alternate binary radious calculation
                //int radius;
                //if (diameter > 6)
                //{
                //    radius = 5;
                //}
                //else
                //{
                //    radius = 3;
                //}

                // creates a small circle around the shape's centroid
                CvInvoke.Circle(outputMat, new Point(x, y), radius, new MCvScalar(100, 100, 100), -1, LineType.EightConnected, 0);
                //CvInvoke.Circle(binaryPlateImage, new Point(x, y), radius, new MCvScalar(100, 100, 100), -1, LineType.EightConnected, 0);

                // Attemp to store contour center's and radious
                int[] array = { x, y, radius };
                centerAndRadius.Add(array);


            }

            // Image Viewer ( showDialog() is what displays the image) 
            ImageViewer binaryPlateViewer = new ImageViewer(outputMat);
            binaryPlateViewer.Image = outputMat;
            //binaryPlateViewer.ShowDialog();  // Turn on for Debug

            return centerAndRadius;

        }

        public string CreateSvgFromPhoto(List<int[]> centerAndRadius)
        {
            // Creates a new StringBuilder object
            var svgStringBuilder = new StringBuilder();

            // Creates our opening and closing svg strings to be tacked on to the Stringbuilder
            string svgOpen = "<svg width = '100vh' height = '100vh'>";
            string svgClose = "</svg>";

            // Adds the opening SVG string to the Stringbuilder
            svgStringBuilder.Append(svgOpen);

            // for every center and radius in our list, create a sub string to be use the the svg file
            for (int i = 0; i < centerAndRadius.Count; i++)
            {
                svgStringBuilder.Append(
                    $"<a  href='https://techspot.com' target='_blank'> " +
                    $"<circle cx='{centerAndRadius[i][0]}' cy='{centerAndRadius[i][1]}' r='{centerAndRadius[i][2]}' stroke='black' stroke-width='5' fill='blue'/>" +
                    $"</a>"
                );
            }

            // Adds the closing SVG strign to the String Builder
            svgStringBuilder.Append(svgClose);

            // Converts the Stringbuilder to a string
            string svgString = svgStringBuilder.ToString();

            // Creates the file directory if the directory does not already exist.  If the directory does already exist, this method does nothing.
            this.SvgFilePath.Directory.Create();

            // Writes the svg file to disk based on the svgString
            File.WriteAllText($"{this.SvgFilePath}{this.FileName}.svg", svgString);

            return svgString;

        }

        public void CreateHtml(string svgString)
        {
            // Creates a new StringBuilder object
            StringBuilder htmlStringBuilder = new StringBuilder();

            // Creates our opening and closing html strings to be tacked on to the Stringbuilder
            string htmlOpen = "" +
                "<!DOCTYPE html>" +
                "<html lang='en'>" +
                "<head>" +
                    "<meta charset='UTF-8'>" +
                    "<meta http-equiv='X-UA-Compatible' content='IE=edge'>" +
                    "<meta name='viewport' content='width=device-width, initial-scale=1.0'>" +
                    "<title>Document</title>" +
                 "</head>" +
                "<body>" +
                    "<h1>Test!</h1>" +
                "";

            string htmlClose = "" +
                "</body>" +
                "</html>" +
                "";

            // Adds the opening html string, then the svgString, and finally the closing html string to the Stringbuilder
            htmlStringBuilder.Append(htmlOpen);
            htmlStringBuilder.Append(svgString);
            htmlStringBuilder.Append(htmlClose);

            // Converts the Stringbuilder to a string
            string htmlString = htmlStringBuilder.ToString();

            // Creates the file directory if the directory does not already exist.  If the directory does already exist, this method does nothing.
            this.HtmlFilePath.Directory.Create();

            // Writes the html file to disk based on the htmlString, which in turn is based partially on the svgString
            File.WriteAllText($"{this.HtmlFilePath}\\index.html", htmlString);

        }

        public string CreateSvgFromApiCoordinates(Dictionary<int, string[]> stellarObjectData)
        {
            // Creates a new StringBuilder object
            var svgStringBuilder = new StringBuilder();

            // Creates the a scaling multiplier to adjust the zoom of the object image on the sdss website
            string photoScaler = ".5";

            // Creates a Scaling Multiplier to adjust the viewbox size while keeping the location of the svg contents scaled to match.
            int plateScalingMultiplier = 2;

            // Creates the radius of the dots on the plate.  The ratio of this number to the plateScalingMulitplier affect the proportional dot size.
            int svgDotRadius = 2;

            // Creates Viewbox Size variables
            int xViewBoxMin = 0 * plateScalingMultiplier;
            int yViewBoxMin = 0 * plateScalingMultiplier;
            int xViewBoxMax = 800 * plateScalingMultiplier;
            int yViewBoxMax = 800 * plateScalingMultiplier;

            // Creates PlateEdge Size variables
            int xSvgPlateCenter = xViewBoxMax / 2;
            int ySvgPlateCenter = yViewBoxMax / 2;
            int SvgPlateRadius = 394 * plateScalingMultiplier;

            // Creates our opening and closing svg strings to be tacked on to the Stringbuilder
            string svgOpen = $"<svg id='svgImage' width='100vw' height='100vh' viewBox='{xViewBoxMin} {yViewBoxMin} {xViewBoxMax} {yViewBoxMax}' transform-origin='0 0'>";
            string svgPlateEdge = $"<circle cx='{xSvgPlateCenter}' cy='{ySvgPlateCenter}' r='{SvgPlateRadius}' strok='grey' stroke-width='1' />";
            string svgClose = "</svg>";

            // Adds the opening SVG string to the Stringbuilder
            svgStringBuilder.Append(svgOpen);
            svgStringBuilder.Append(svgPlateEdge);


            // for every center and radius in our list, create a sub string to be use the the svg file
            for (int i = 0; i < stellarObjectData.Count; i++)
            {
                double cxScaledAndTranslatedInt = (double.Parse(stellarObjectData[i][0]) * plateScalingMultiplier) + (400 * plateScalingMultiplier);
                double cyScaledAndTranslatedInt = (double.Parse(stellarObjectData[i][1]) * plateScalingMultiplier) + (400 * plateScalingMultiplier);

                string cxScaledAndTranslatedString = cxScaledAndTranslatedInt.ToString();
                string cyScaledAndTranslatedString = cyScaledAndTranslatedInt.ToString();

                svgStringBuilder.Append(
                    // Ampersands "&" in the href query string have been replaced with "&amp;" since a regular Ampersand is a escapement character in XML (svg).
                    $"<a href='https://skyserver.sdss.org/dr17/VisualTools/navi?ra={stellarObjectData[i][4]}&amp;dec={stellarObjectData[i][5]}&amp;scale={photoScaler}' target='_blank'> " +
                    $"<circle cx='{cxScaledAndTranslatedString}' cy='{cyScaledAndTranslatedString}' r='{svgDotRadius}' stroke='black' stroke-width='1' fill='red'/>" +
                    $"{stellarObjectData[i][2]}, plate: {stellarObjectData[i][3]}</a>"
                );
            }

            // Adds the closing SVG strign to the String Builder
            svgStringBuilder.Append(svgClose);

            // Converts the Stringbuilder to a string
            string svgString = svgStringBuilder.ToString();

            // Creates the file directory if the directory does not already exist.  If the directory does already exist, this method does nothing.
            this.SvgFilePath.Directory.Create();

            // Writes the svg file to disk based on the svgString
            File.WriteAllText($"{this.SvgFilePath}{this.FileName}.svg", svgString);

            return svgString;
        }

        public void CreateHtmlFromSvgFromApi(string svgStringFromApi)
        {
            // Creates a new StringBuilder object
            StringBuilder htmlStringBuilder = new StringBuilder();

            // Creates our opening and closing html strings to be tacked on to the Stringbuilder
            string htmlOpen = "" +
                "<!DOCTYPE html>" +
                "<html lang='en'>" +
                "<head>" +
                    "<meta charset='UTF-8'>" +
                    "<meta http-equiv='X-UA-Compatible' content='IE=edge'>" +
                    "<meta name='viewport' content='width=device-width, initial-scale=1.0'>" +
                    "<title>Document</title>" +
                    "<script src='https://www.unpkg.com/@panzoom/panzoom/dist/panzoom.js'></script>" +
                "</head>" +
                "<body>" +
                    "<div id='svgWrapper' style='display: flex; justify-content: center; align-items: center'>" +
                 "";

            string htmlClose = "" +
                    "</div>" +
                    "<script> " +
                        "const element = document.getElementById('svgWrapper')," +
                        // the below reads like it has a "const" at the beginning because of the comma from the above line.
                        "panzoom = Panzoom(element, {" +
                        // options here
                        "maxScale: 50," +
                        "minScale: .75" +
                        "});" +
                        // enable mouse wheel
                        "const parent = element.parentElement;" +
                        "parent.addEventListener('wheel', panzoom.zoomWithWheel);" +
                    "</script>" +
                "</body>" +
                "</html>" +
                "";

            // Adds the opening html string, then the svgString, and finally the closing html string to the Stringbuilder
            htmlStringBuilder.Append(htmlOpen);
            htmlStringBuilder.Append(svgStringFromApi);
            htmlStringBuilder.Append(htmlClose);

            // Converts the Stringbuilder to a string
            string htmlString = htmlStringBuilder.ToString();

            // Creates the file directory if the directory does not already exist.  If the directory does already exist, this method does nothing.
            this.HtmlFilePath.Directory.Create();

            // Writes the html file to disk based on the htmlString, which in turn is based partially on the svgString
            File.WriteAllText($"{this.HtmlFilePath}{this.FileName}.html", htmlString);

        }

    }
}