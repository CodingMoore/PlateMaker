
//using Emgu.CV;
//using Emgu.CV.UI;
//using Emgu.CV.Structure;
//using System.Drawing;
//using System.Windows.Forms;
//using Emgu.CV.Features2D;
//using Emgu.CV.CvEnum;
//using Emgu.CV.Util;

//// https://www.youtube.com/watch?v=0ibBYinRiEA
//// https://www.codeproject.com/Articles/257502/Creating-Your-First-EMGU-Image-Processing-Project



//// imports image
//Image<Bgr, Byte> Plate = new Image<Bgr, Byte>("C:\\Users\\Randel\\source\\repos\\PlateScanner\\PlateScanner\\Images\\FakePlateV2.jpg");

//// noise suppression
//var smoothedPlate = Plate.SmoothGaussian(9);

//// Find/create plate (x/y) center

//// create one central object
//Bgr redDotLowerLimit = new Bgr(0, 0, 100);
//Bgr redDotUpperLimit = new Bgr(0, 0, 255);
//Image<Gray, byte> redDotPlate = smoothedPlate.InRange(redDotLowerLimit, redDotUpperLimit);

//// find center point
//VectorOfVectorOfPoint centerPointContours = new VectorOfVectorOfPoint();
//Mat centerPointMat = new Mat();
//CvInvoke.FindContours(redDotPlate, centerPointContours, centerPointMat, RetrType.External, ChainApproxMethod.ChainApproxSimple);

//int xPlateCenter = 0;
//int yPlateCenter = 0;

//ImageViewer centerDotViewer = new ImageViewer(Plate);

//for (int i = 0; i < centerPointContours.Size; i++)
//{
//    double centerPointPerimeter = CvInvoke.ArcLength(centerPointContours[i], true);
//    VectorOfPoint centerPointApprox = new VectorOfPoint();
//    CvInvoke.ApproxPolyDP(centerPointContours[i], centerPointApprox, 0.04 * centerPointPerimeter, true);
//    CvInvoke.DrawContours(Plate, centerPointContours, i, new MCvScalar(255, 0, 255), 2);


//    var moments = CvInvoke.Moments(centerPointContours[i]);
//    xPlateCenter = (int)(moments.M10 / moments.M00);
//    yPlateCenter = (int)(moments.M01 / moments.M00);

//    CvInvoke.Line(Plate, new Point(xPlateCenter, yPlateCenter), new Point(xPlateCenter, yPlateCenter), new MCvScalar(255, 0, 0), 5, LineType.EightConnected, 0);

//    centerDotViewer.Image = Plate;
//}

//// Find second rotaion point coordinates

//Bgr greenDotLowerLimit = new Bgr(0, 200, 0);
//Bgr greenDotUpperLimit = new Bgr(30, 255, 30);
//Image<Gray, byte> greenDotPlate = smoothedPlate.InRange(greenDotLowerLimit, greenDotUpperLimit);

//// find center point
//VectorOfVectorOfPoint greenDotPointContours = new VectorOfVectorOfPoint();
//Mat greenDotPointMat = new Mat();
//CvInvoke.FindContours(greenDotPlate, greenDotPointContours, greenDotPointMat, RetrType.External, ChainApproxMethod.ChainApproxSimple);

//int xGreenDot = 0;
//int yGreenDot = 0;

//ImageViewer greenDotViewer = new ImageViewer(Plate);

//for (int i = 0; i < greenDotPointContours.Size; i++)
//{
//    double greenDotPointPerimeter = CvInvoke.ArcLength(greenDotPointContours[i], true);
//    VectorOfPoint greenDotPointApprox = new VectorOfPoint();
//    CvInvoke.ApproxPolyDP(greenDotPointContours[i], greenDotPointApprox, 0.04 * greenDotPointPerimeter, true);
//    CvInvoke.DrawContours(Plate, greenDotPointContours, i, new MCvScalar(255, 0, 255), 2);


//    var moments = CvInvoke.Moments(greenDotPointContours[i]);
//    xGreenDot = (int)(moments.M10 / moments.M00);
//    yGreenDot = (int)(moments.M01 / moments.M00);

//    CvInvoke.Line(Plate, new Point(xGreenDot, yGreenDot), new Point(xGreenDot, yGreenDot), new MCvScalar(0, 0, 255), 5, LineType.EightConnected, 0);

//    greenDotViewer.Image = Plate;

//}

//// translate image to match centerDot to image center

//int xImageCenter = Plate.Width / 2;
//int yImageCenter = Plate.Height / 2;

//int xDistance = xPlateCenter - xImageCenter;
//int yDistance = yPlateCenter - yImageCenter;   

//PointF[] sourcePoints = new PointF[]
//{
//    new PointF(xPlateCenter,yPlateCenter),
//    new PointF(1, 0),
//    new PointF(0, 1)

//};

//PointF[] destinationPoints = new PointF[]
//{
//    new PointF(xImageCenter, yImageCenter),
//    new PointF(1 - xDistance, 0 - yDistance),
//    new PointF(0 - xDistance, 1 - yDistance)

//};

//// Gets TranslationMap for ues in WarpAffine function.
//Mat translationMap = CvInvoke.GetAffineTransform(sourcePoints, destinationPoints);

//// Make the translation move
//CvInvoke.WarpAffine(Plate, smoothedPlate, translationMap, new Size(Plate.Width, Plate.Height));


//double rotationRadians = 0;
//double rotationAngle = 0;

//// checks to see if the plate is upside down (this matters because the coordinate 0,0 is in the upper left corner of the image)
//if ((yImageCenter - yGreenDot) > 0)
//{
//    rotationRadians = Math.Atan2(xGreenDot - xImageCenter, yImageCenter - yGreenDot);
//} 
//else
//{
//    rotationRadians = Math.Atan2(xGreenDot - xImageCenter, yGreenDot - yImageCenter);
//}

//double absoluteRotationAngle = rotationRadians * (180 / Math.PI);

//// checks to see if the rotation should be clockwise our counter clockwise
//if ((xGreenDot - xImageCenter) > 0)
//{
//    rotationAngle = -absoluteRotationAngle;
//} 
//else
//{
//    rotationAngle = absoluteRotationAngle;
//}

//var rotatedPlate = smoothedPlate.Rotate(rotationAngle, new Bgr(150, 150, 150), true);

//// Converts to grey scale, then to binary.  ThreshholdBinary(new Gray(myvalue), new Gray(255)
//var binaryPlate = rotatedPlate.Convert<Gray, byte>().ThresholdBinary(new Gray(10), new Gray(255));

//// creates new overlay mask
//Mat forgroundMat = new Mat();

//VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();

//// Finds the contour points.  Since we are using a simple version, it will discard unnecessary points.  For example, a square will only have 4 poits.
//CvInvoke.FindContours(binaryPlate, contours, forgroundMat, RetrType.External, ChainApproxMethod.ChainApproxSimple);

//ImageViewer viewer = new ImageViewer(Plate);

//for (int i = 0; i < contours.Size; i++)
//{
//    //Find subset of points that will aproxamately maintain the shape of the current contrours.
//    double perimeter = CvInvoke.ArcLength(contours[i], true);

//    //Something to store the below approxamation in
//    VectorOfPoint approx = new VectorOfPoint();

//    // Fine tune how approxomate the shape needs to be to match
//    CvInvoke.ApproxPolyDP(contours[i], approx, 0.04 * perimeter, true);

//    // Draw new contours on original image
//    CvInvoke.DrawContours(binaryPlate, contours, i, new MCvScalar(255, 0, 255),2);

//    // moments (center of the shape), getting x/y coordinates
//    var moments = CvInvoke.Moments(contours[i]);
//    int x = (int) (moments.M10 / moments.M00);
//    int y = (int) (moments.M01 / moments.M00);


//    if (approx.Size == 3)
//    {
//        // Detects if it is a triangle, and if so, adds text.
//        CvInvoke.PutText(Plate, "Triangle", new Point(x, y), FontFace.HersheySimplex, 0.5, new MCvScalar(0, 0, 255), 1);
//    }
//    if (approx.Size == 4)
//    {
//        Rectangle fourPointShape = CvInvoke.BoundingRectangle(contours[i]);
//        double ar = (double)fourPointShape.Width / fourPointShape.Height;
//        // checking ratio of height/width
//        if (ar >= .95 && ar <= 1.05)
//        {
//            // Detects if it is a square, and if so, adds text.
//            CvInvoke.PutText(Plate, "Square", new Point(x, y), FontFace.HersheySimplex, 0.5, new MCvScalar(0, 0, 255), 1);
//        }
//        else
//        {
//            // Detects if it is a Rectagle, and if so, adds text.
//            CvInvoke.PutText(Plate, "Rectangle", new Point(x, y), FontFace.HersheySimplex, 0.5, new MCvScalar(0, 0, 255), 1);
//        }

//    }
//    if (approx.Size == 6)
//    {
//        // Detects if it is a hexagon, and if so, adds text.
//        CvInvoke.PutText(Plate, "Hexagon", new Point(x, y), FontFace.HersheySimplex, 0.5, new MCvScalar(0, 0, 255), 1);
//    }
//    if (approx.Size > 6)
//    {
//        // Detects if it is a circle, and if so, adds text.  
//        CvInvoke.PutText(Plate, "Circle", new Point(x, y), FontFace.HersheySimplex, 0.5, new MCvScalar(0, 0, 255), 1);
//    }

//    // creates a dot (zero length line) at the shape's centroid
//    CvInvoke.Line(binaryPlate, new Point(x, y), new Point(x, y), new MCvScalar(255, 0, 0), 5, LineType.EightConnected, 0);

//    // creates a small circle around the shape's centroid
//    CvInvoke.Circle(binaryPlate, new Point(x,y), 4, new MCvScalar(0, 255, 0), 5, LineType.EightConnected, 0);


//    viewer.Image = binaryPlate;


//}


////centerDotViewer.ShowDialog();
////greenDotViewer.ShowDialog();
//viewer.ShowDialog();

