using System.Text;

namespace PlateMaker
{
    class Plate
    {
        public string PlateNumber { get; set; }
        public FileInfo SvgFilePath { get; set; }
        public FileInfo HtmlFilePath { get; set; }
        public double OpenSeaDragonMaxZoomPixelRatio { get; set; }

        public Plate(string plateNumber, string fileSavingDirectory)
        {
            // Extracts the original Image file name from the full file path, and removes the file extension.
            // This will be used later when we save each image's svg and html files as they will use the same file name.
            PlateNumber = Path.GetFileNameWithoutExtension(plateNumber);

            // Defines the full directory path for saving the svg files.
            SvgFilePath = new FileInfo($"{fileSavingDirectory}\\svg Files\\");

            // Defines the full directory path for saving the html files.
            HtmlFilePath = new FileInfo($"{fileSavingDirectory}\\html Files\\");

            // Defines how far you can zoom in on an image using OpenSeaDragon.
            // The default value for OSD is 1, which represents a 1:1 pixel ratio of the image to the screen.
            // Since this value is needed in multiple methods, it is housed here.
            OpenSeaDragonMaxZoomPixelRatio = 4;
        }

        public string CreateSvgFromApiCoordinates(Dictionary<int, string[]> stellarObjectData)
        {
            ////////////////////////////////
            ///// Adjustable Constants /////
            ////////////////////////////////
    
            // Creates the a scaling multiplier to adjust the zoom of the object image on the sdss website
            // This is used when the plateDot use links that go to the SDSS "Visual Tools" webpage.
            //string photoScaler = ".5"; 

            // The dotScaler affects distance between the dots since it is multiplied by each dot's x and y coordinates.
            // Since the area of the dots is automatically scaled to fit its container, this has the effect of changing the perceived dot size.
            // The HIGHER the number, the SMALLER the dots (.5 is larger than 3).
            double dotScaler = 1;

            // Creates a Relative size multiplier for the plate dot area in relation to the plate border.
            // If the multiplier was set at 1, the dots would completely fill the plate from border to border.
            double dotAreaScaler = .8;

            // Creates a scaling multiplier to define the width of the stroke in relation to the size of the dot.
            // If you change the dotScaler, the stroke width will scale with it.
            // If you change the strokeWidthScaler, then you change the stroke width relative to the dot size.
            double strokeWidthScaler = .5;

            // Creates a constant to keep the ratio of the OSD tile source width to the svgImage size constant.
            double OpenSeaDragonOuterTileSourceWidthSvg = 1000 * this.OpenSeaDragonMaxZoomPixelRatio;


            // Styling constants (These are not in the HTML Style section because someone may want to use the SVGs by themselves, without the HTML functionality).
            string plateBorderStyle = "style='stroke:rgb(92,92,92); stroke-width:.25;'";

            string plateFillStyle = "style='fill:rgb(92,92,92);'";

            string plateEighthInchCenterHoleStyle = "style='fill:rgb(76,0,255);'";

            string plateQuarterInchHoleStyle = "style='fill:rgb(255,0,0);'";

            string plateHalfInch180DegreeSpacedHoleStyle = "style='fill:rgb(0,181,255);'";

            string plateHalfInch30DegreeSpacedHoleStyle = "style='fill:rgb(102,255,0);'";

            // default colors for the dot stroke and fill
            string dotStrokeColor = "rgb(255, 255, 255)";
            string dotFillColor = "rgb(255, 255, 255)";

            // GALAXY - colors for dot stroke and fill
            string galaxyStrokeColor = "rgb(255, 0, 132)";
            string galaxyFillColor = "rgb(255, 255, 255)";

            // STAR - colors for dot stroke and fill
            string starStrokeColor = "rgb(0,181,255)";
            string starFillColor = "rgb(255, 255, 255)";

            // QSO - colors for dot stroke and fill
            string qsoStrokeColor = "rgb(102,255,0)";
            string qsoFillColor = "rgb(255, 255, 255)";

            ////////////////////////////////
            ////////////////////////////////


            ////////////////////////////////////
            ///// Non-Adjustable Constants /////
            ////////////////////////////////////

            // The stroke width of the dots, as calculated from the adjustable constants above.
            double strokeWidth = strokeWidthScaler * dotAreaScaler;

            // Creates a compound scaler used to calculate how the dots are laid out on the screen. Calculated from the adjustable constants above.
            double relativeSizeScaler = dotAreaScaler * dotScaler;

            // Creates Viewbox Size constants. You should probably leave these alone.
            // If you want to change the relative size of the area that the dots take up, change the "dotAreaScaler" instead.
            double xViewBoxMin = 0 * dotScaler;
            double yViewBoxMin = 0 * dotScaler;
            double xViewBoxMax = 800 * dotScaler;
            double yViewBoxMax = 800 * dotScaler;

            ////////////////////////////////////
            ////////////////////////////////////

            // Creates our opening svg string to be tacked on to the beginning of the Stringbuilder

            // panzoom version (IF the panzoom library is to be used instead of OpenSeadragon (OSD).
            //string svgOpenOuter = $"\t\t<svg id='svgImage' width='100vw' height='100vh' viewBox='{xViewBoxMin} {yViewBoxMin} {xViewBoxMax} {yViewBoxMax}' transform-origin='0 0'>   \n";

            //OpenSeaDragon Version
            string svgOpenOuter = $"\t\t<svg id='svgImage' width='{OpenSeaDragonOuterTileSourceWidthSvg}px' height='{OpenSeaDragonOuterTileSourceWidthSvg}px' viewBox='{xViewBoxMin} {yViewBoxMin} {xViewBoxMax} {yViewBoxMax}' transform-origin='0 0'>   \n";


            // Creates container for the plate's night sky background image.
            string svgSkyImageBox =
                "\t\t\t<!-- The viewBox max values determine how the background image lines up with dots on the plate.  -->   \n" +
                //          The viewBox max values determine how the background image lines up with dots on the plate.
                "\t\t\t<svg id='skyImageBox' width='100%' height='100%' viewBox='0 0 120 100' preserveAspectRatio='xMidYMid meet' display='none'>   \n" +
                "\t\t\t\t<!-- Original panzoom version -->   \n" +
                //            Original panzoom version
               $"\t\t\t\t<!-- <image id='skyImageBoxBackgroundImage' preserveAspectRatio='xMidYMid slice' width='100%' height='100%' href='../assets/plateBackgroundImages/PlateID-{this.PlateNumber}.jpg'/> -->   \n" +
                "\t\t\t\t<image id='skyImageBoxBackgroundImage' preserveAspectRatio='xMidYMid slice' width='100%' height='100%'/>   \n" +
                "\t\t\t\t<!-- This rectangle is just used for visualizing the edges of the parent svg when setting up the program -->   \n" +
                //            This rectangle is just used for visualizing the edges of the parent svg when setting up the program
                "\t\t\t\t<!-- <rect width='100%' height='100%' viewBox='0 0 100 100' stroke='pink' stroke-width='2px' fill='url(#spaceBoxBackground)'/> -->   \n" +
                "\t\t\t</svg>   \n";

            // Creates our closing svg string to be tacked on to the end of the Stringbuilder
            string svgCloseOuter = "\t\t</svg>    \n";

            // Creates the plate border, which will be added to the Stringbuilder.
            string svgPlateBorder =
                "\t\t\t<svg viewBox='0 0 120 120' style='fill-rule:evenodd;clip-rule:evenodd;stroke-linejoin:round;stroke-miterlimit:2;'>   \n" +
                "\t\t\t\t<g id='Plate-Edge-Exterior-Stroke' transform='matrix(-1,-1.22465e-16,1.22465e-16,-1,120,120)'>   \n" +
                "\t\t\t\t\t<path d='M61.474,10.597C88.071,11.377 109.424,33.215 109.424,60C109.424,87.278 87.278,109.424 60,109.424C32.722," +
                                "109.424 10.576,87.278 10.576,60C10.576,33.215 31.929,11.377 58.526,10.597L58.526,9.266C58.526,9.172 58.601," +
                                "9.095 58.696,9.092C59.129,9.081 59.564,9.076 60,9.076C60.436,9.076 60.871,9.081 61.304,9.092C61.399," +
                                "9.095 61.474,9.172 61.474,9.266L61.474,10.597ZM61.3,9.266L61.3,10.767C87.882,11.457 109.25,33.253 109.25," +
                                "60C109.25,87.182 87.182,109.25 60,109.25C32.818,109.25 10.75,87.182 10.75,60C10.75,33.253 32.118,11.457 58.7," +
                                "10.767L58.7,9.266C59.132,9.255 59.565,9.25 60,9.25C60.435,9.25 60.868,9.255 61.3,9.266Z'   \n" +
               $"\t\t\t\t\t{plateBorderStyle}/>   \n" +
                "\t\t\t\t</g>   \n" +
                "\t\t\t</svg>   \n";

            // Creates the plate fill, which will be added to the Stringbuilder
            string svgPlateFill =
                "\t\t\t<svg id='backgroundFill' class='fillOn' viewBox='0 0 120 120' style='fill-rule:evenodd;clip-rule:evenodd;stroke-linejoin:round;stroke-miterlimit:2;'>   \n" +
                "\t\t\t\t<g id='Plate-Edge-with-Subtracted-Holes' transform = 'matrix(-1,-1.22465e-16,1.22465e-16,-1,120,120)'>" +
                "\t\t\t\t\t<path id='backgroundFillPath' d='M61.3,9.266L61.3,10.767C87.882,11.457 109.25,33.253 109.25,60C109.25,87.182 87.182,109.25 60," +
                                "109.25C32.818,109.25 10.75,87.182 10.75,60C10.75,33.253 32.118,11.457 58.7,10.767L58.7,9.266C59.132,9.255 59.565,9.25 60," +
                                "9.25C60.435,9.25 60.868,9.255 61.3,9.266ZM71.802,104.047C71.376,104.161 70.937,103.908 70.823,103.481C70.708,103.055 70.962," +
                                "102.616 71.388,102.501C71.815,102.387 72.254,102.641 72.368,103.067C72.482,103.494 72.229,103.933 71.802,104.047ZM48.198," +
                                "104.047C47.771,103.933 47.518,103.494 47.632,103.067C47.746,102.641 48.185,102.387 48.612,102.501C49.038,102.616 49.292," +
                                "103.055 49.177,103.481C49.063,103.908 48.624,104.161 48.198,104.047ZM40.728,101.328C40.328,101.142 40.155,100.665 40.341," +
                                "100.265C40.528,99.865 41.004,99.692 41.405,99.878C41.805,100.065 41.978,100.541 41.791,100.941C41.605,101.341 41.128," +
                                "101.515 40.728,101.328ZM27.755,92.245C27.443,91.932 27.443,91.425 27.755,91.113C28.068,90.801 28.575,90.801 28.887," +
                                "91.113C29.199,91.425 29.199,91.932 28.887,92.245C28.575,92.557 28.068,92.557 27.755,92.245ZM92.245,92.245C91.932," +
                                "92.557 91.425,92.557 91.113,92.245C90.801,91.932 90.801,91.425 91.113,91.113C91.425,90.801 91.932,90.801 92.245," +
                                "91.113C92.557,91.425 92.557,91.932 92.245,92.245ZM99.145,82.6C99.035,82.792 98.79,82.857 98.599,82.747C98.407,82.636 98.342," +
                                "82.392 98.452,82.2C98.562,82.009 98.807,81.944 98.999,82.054C99.19,82.164 99.255,82.409 99.145,82.6ZM20.855,82.6C20.745," +
                                "82.409 20.81,82.164 21.001,82.054C21.193,81.944 21.438,82.009 21.548,82.2C21.658,82.392 21.593,82.636 21.401,82.747C21.21," +
                                "82.857 20.965,82.792 20.855,82.6ZM104.047,71.802C103.933,72.229 103.494,72.482 103.067,72.368C102.641,72.254 102.387," +
                                "71.815 102.501,71.388C102.616,70.962 103.055,70.708 103.481,70.823C103.908,70.937 104.161,71.376 104.047,71.802ZM15.953," +
                                "71.802C15.839,71.376 16.092,70.937 16.519,70.823C16.945,70.708 17.384,70.962 17.499,71.388C17.613,71.815 17.359," +
                                "72.254 16.933,72.368C16.506,72.482 16.067,72.229 15.953,71.802ZM60,59.8C60.11,59.8 60.2,59.89 60.2,60C60.2,60.11 60.11," +
                                "60.2 60,60.2C59.89,60.2 59.8,60.11 59.8,60C59.8,59.89 59.89,59.8 60,59.8ZM104.047,48.198C104.161,48.624 103.908," +
                                "49.063 103.481,49.177C103.055,49.292 102.616,49.038 102.501,48.612C102.387,48.185 102.641,47.746 103.067,47.632C103.494," +
                                "47.518 103.933,47.771 104.047,48.198ZM15.953,48.198C16.067,47.771 16.506,47.518 16.933,47.632C17.359,47.746 17.613," +
                                "48.185 17.499,48.612C17.384,49.038 16.945,49.292 16.519,49.177C16.092,49.063 15.839,48.624 15.953,48.198ZM27.755," +
                                "27.755C28.068,27.443 28.575,27.443 28.887,27.755C29.199,28.068 29.199,28.575 28.887,28.887C28.575,29.199 28.068," +
                                "29.199 27.755,28.887C27.443,28.575 27.443,28.068 27.755,27.755ZM92.245,27.755C92.557,28.068 92.557,28.575 92.245," +
                                "28.887C91.932,29.199 91.425,29.199 91.113,28.887C90.801,28.575 90.801,28.068 91.113,27.755C91.425,27.443 91.932," +
                                "27.443 92.245,27.755ZM79.272,18.672C79.672,18.858 79.845,19.335 79.659,19.735C79.472,20.135 78.996,20.308 78.595," +
                                "20.122C78.195,19.935 78.022,19.459 78.209,19.059C78.395,18.659 78.872,18.485 79.272,18.672ZM48.198,15.953C48.624," +
                                "15.839 49.063,16.092 49.177,16.519C49.292,16.945 49.038,17.384 48.612,17.499C48.185,17.613 47.746,17.359 47.632," +
                                "16.933C47.518,16.506 47.771,16.067 48.198,15.953ZM71.802,15.953C72.229,16.067 72.482,16.506 72.368,16.933C72.254," +
                                "17.359 71.815,17.613 71.388,17.499C70.962,17.384 70.708,16.945 70.823,16.519C70.937,16.092 71.376,15.839 71.802,15.953ZM60," +
                                "14.799C60.221,14.799 60.4,14.979 60.4,15.199C60.4,15.42 60.221,15.599 60,15.599C59.779,15.599 59.6,15.42 59.6,15.199C59.6," +
                                "14.979 59.779,14.799 60,14.799Z'   \n" +
               $"\t\t\t\t\t{plateFillStyle}/>   \n" +
                "\t\t\t\t</g>   \n" +
                "\t\t\t</svg>   \n";

            // Creates a fill for the 1/8 inch hole in the center of the plate, which will be added to the Stringbuilder
            string svgPlateEighthInchCenterHole =
                "\t\t\t<svg viewBox='0 0 120 120' style='fill-rule:evenodd;clip-rule:evenodd;stroke-linejoin:round;stroke-miterlimit:2;'>   \n" +
                "\t\t\t\t<g id='_1-8-inch-center-hole' serif:id='1/8 inch center hole' transform='matrix(0.0391862,0,0,0.0391862,57.5523,57.6538)'>   \n" +
               $"\t\t\t\t\t<circle cx='62.463' cy='59.874' r='5.104' {plateEighthInchCenterHoleStyle}/>   \n" +
                "\t\t\t\t\t<!-- The 'dominant-baseline='middle' text-anchor='middle' centers the text. -->   \n" +
                "\t\t\t\t\t<!-- There seems to be an issue that the this center dot is not acutally centered (see its cx and cy). -->   \n" +
                "\t\t\t\t\t<!-- For some reason the text's x coordinate needs to be a 52% instead of 50% to get it centered. -->   \n" +
                //              The 'dominant-baseline='middle' text-anchor='middle' centers the text.
                //              There seems to be an issue that the this center dot is not acutally centered (see its cx and cy).
                //              For some reason the text's x coordinate needs to be a 52% instead of 50% to get it centered.
                "\t\t\t\t\t<text font-size='1.5px' x='52%' y='50%' fill='black' dominant-baseline='middle' text-anchor='middle'>Plate Center</text>   \n" +
                "\t\t\t\t</g>   \n" +
                "\t\t\t</svg>   \n";

            // Creates a fill for the three evenly space (120 Degrees apart) 1/4 inch holes in the plate, which will be added to the Stringbuilder
            string svgPlateQuarterInchHoles =
                "\t\t\t<svg viewBox='0 0 120 120' style='fill-rule:evenodd;clip-rule:evenodd;stroke-linejoin:round;stroke-miterlimit:2;'>   \n" +
                "\t\t\t\t<g id='_1-4-inch-holes'   \n" +
                "\t\t\t\t\t<g transform='matrix(0.430075,0.744911,-0.744911,0.430075,83.4796,-11.1266)'>   \n" +
               $"\t\t\t\t\t\t<circle cx='57.964' cy='12.901' r='0.465' {plateQuarterInchHoleStyle}/>    \n" +
                "\t\t\t\t\t</g>   \n" +
                "\t\t\t\t\t<g transform='matrix(0.430075,-0.744911,0.744911,0.430075,-13.3372,75.2294)'>   \n" +
               $"\t\t\t\t\t\t<circle cx='57.964' cy='12.901' r='0.465' {plateQuarterInchHoleStyle}/>   \n" +
                "\t\t\t\t\t</g>   \n" +
                "\t\t\t\t\t<g transform='matrix(-0.860149,-1.05338e-16,1.05338e-16,-0.860149,109.858,115.897)'>   \n" +
               $"\t\t\t\t\t\t<circle cx='57.964' cy='12.901' r='0.465' {plateQuarterInchHoleStyle}/>   \n" +
                "\t\t\t\t\t</g>   \n" +
                "\t\t\t\t</g>   \n" +
                "\t\t\t</svg>   \n";

            // Creates a fill for the two evently spaced (180 Degrees apart) 1/2 inch holes in the plate, which will be added to the Stringbuilder
            string svgPlateHalfInch180DegreeSpacedHoles =
                "\t\t\t<svg viewBox='0 0 120 120' style='fill-rule:evenodd;clip-rule:evenodd;stroke-linejoin:round;stroke-miterlimit:2;'>   \n" +
                "\t\t\t\t<g id='_1-2-inch-holes--180-degree-spacing-'>   \n" +
                "\t\t\t\t\t<g transform='matrix(0.579984,0.270451,-0.270451,0.579984,48.2348,-5.80127)'>   \n" +
               $"\t\t\t\t\t\t<circle cx='60.118' cy='15.413' r='1.25' {plateHalfInch180DegreeSpacedHoleStyle}/>   \n" +
                "\t\t\t\t\t</g>   \n" +
                "\t\t\t\t\t<g transform='matrix(-0.579984,-0.270451,0.270451,-0.579984,71.7652,125.801)'>   \n" +
               $"\t\t\t\t\t\t<circle cx='60.118' cy='15.413' r='1.25' {plateHalfInch180DegreeSpacedHoleStyle}/>   \n" +
                "\t\t\t\t\t</g>   \n" +
                "\t\t\t\t</g>   \n" +
                "\t\t\t</svg>   \n";

            // Creates a fill for the twelve evently spaced (30 Degrees apart) 1/2 inch holes in the plate, which will be added to the Stringbuilder
            string svgPlateHalfInch30DegreeSpacedHoles =
                "\t\t\t<svg viewBox='0 0 120 120' style='fill-rule:evenodd;clip-rule:evenodd;stroke-linejoin:round;stroke-miterlimit:2;'>   \n" +
                "\t\t\t\t<g id='_1-2-inch-holes--30-degree-spacing--'>   \n" +
                "\t\t\t\t\t<g transform='matrix(0.452507,-0.452507,0.452507,0.452507,-5.8569,48.5503)'>   \n" +
               $"\t\t\t\t\t\t<circle cx='60.118' cy='15.413' r='1.25' {plateHalfInch30DegreeSpacedHoleStyle}/>   \n" +
                "\t\t\t\t\t</g>   \n" +
                "\t\t\t\t\t<g transform='matrix(0.165629,-0.618136,0.618136,0.165629,-2.75859,83.0127)'>   \n" +
               $"\t\t\t\t\t\t<circle cx='60.118' cy='15.413' r='1.25' {plateHalfInch30DegreeSpacedHoleStyle}/>   \n" +
                "\t\t\t\t\t</g>   \n" +
                "\t\t\t\t\t<g transform='matrix(-0.165629,-0.618136,0.618136,-0.165629,17.1558,111.309)'>   \n" +
               $"\t\t\t\t\t\t<circle cx='60.118' cy='15.413' r='1.25' {plateHalfInch30DegreeSpacedHoleStyle}/>   \n" +
                "\t\t\t\t\t</g>   \n" +
                "\t\t\t\t\t<g transform='matrix(-0.452507,-0.452507,0.452507,-0.452507,48.5503,125.857)'>   \n" +
               $"\t\t\t\t\t\t<circle cx='60.118' cy='15.413' r='1.25' {plateHalfInch30DegreeSpacedHoleStyle}/>   \n" +
                "\t\t\t\t\t</g>   \n" +
                "\t\t\t\t\t<g transform='matrix(-0.618136,-0.165629,0.165629,-0.618136,83.0127,122.759)'>   \n" +
               $"\t\t\t\t\t\t<circle cx='60.118' cy='15.413' r='1.25' {plateHalfInch30DegreeSpacedHoleStyle}/>   \n" +
                "\t\t\t\t\t</g>   \n" +
                "\t\t\t\t\t<g transform='matrix(-0.618136,0.165629,-0.165629,-0.618136,111.309,102.844)'>   \n" +
               $"\t\t\t\t\t\t<circle cx='60.118' cy='15.413' r='1.25' {plateHalfInch30DegreeSpacedHoleStyle}/>   \n" +
                "\t\t\t\t\t</g>   \n" +
                "\t\t\t\t\t<g transform='matrix(-0.452507,0.452507,-0.452507,-0.452507,125.857,71.4497)'>   \n" +
               $"\t\t\t\t\t\t<circle cx='60.118' cy='15.413' r='1.25' {plateHalfInch30DegreeSpacedHoleStyle}/>   \n" +
                "\t\t\t\t\t</g>   \n" +
                "\t\t\t\t\t<g transform='matrix(-0.165629,0.618136,-0.618136,-0.165629,122.759,36.9873)'>   \n" +
               $"\t\t\t\t\t\t<circle cx='60.118' cy='15.413' r='1.25' {plateHalfInch30DegreeSpacedHoleStyle}/>   \n" +
                "\t\t\t\t\t</g>   \n" +
                "\t\t\t\t\t<g transform='matrix(0.165629,0.618136,-0.618136,0.165629,102.844,8.6911)'>   \n" +
               $"\t\t\t\t\t\t<circle cx='60.118' cy='15.413' r='1.25' {plateHalfInch30DegreeSpacedHoleStyle}/>   \n" +
                "\t\t\t\t\t</g>   \n" +
                "\t\t\t\t\t<g transform='matrix(0.452507,0.452507,-0.452507,0.452507,71.4497,-5.8569)'>   \n" +
               $"\t\t\t\t\t\t<circle cx='60.118' cy='15.413' r='1.25' {plateHalfInch30DegreeSpacedHoleStyle}/>   \n" +
                "\t\t\t\t\t</g>   \n" +
                "\t\t\t\t\t<g transform='matrix(0.618136,0.165629,-0.165629,0.618136,36.9873,-2.75859)'>   \n" +
               $"\t\t\t\t\t\t<circle cx='60.118' cy='15.413' r='1.25' {plateHalfInch30DegreeSpacedHoleStyle}/>   \n" +
                "\t\t\t\t\t</g>   \n" +
                "\t\t\t\t\t<g transform='matrix(0.618136,-0.165629,0.165629,0.618136,8.6911,17.1558)'>   \n" +
               $"\t\t\t\t\t\t<circle cx='60.118' cy='15.413' r='1.25' {plateHalfInch30DegreeSpacedHoleStyle}/>   \n" +
                "\t\t\t\t\t</g>   \n" +
                "\t\t\t\t</g>   \n" +
                "\t\t\t</svg>   \n";

            // Creates a new StringBuilder object, to which we can add all our SVG elements.
            var svgStringBuilder = new StringBuilder();

            // Adds the initial SVG elements to the Stringbuilder
            svgStringBuilder.Append(svgOpenOuter);
            svgStringBuilder.Append(svgSkyImageBox);
            svgStringBuilder.Append(svgPlateBorder);
            svgStringBuilder.Append(svgPlateFill);
            svgStringBuilder.Append(svgPlateEighthInchCenterHole);
            svgStringBuilder.Append(svgPlateQuarterInchHoles);
            svgStringBuilder.Append(svgPlateHalfInch180DegreeSpacedHoles);
            svgStringBuilder.Append(svgPlateHalfInch30DegreeSpacedHoles);

            // For every center and radius in our list list of stellar objects (stellarObjectData), creates variables to be use the the svg file
            for (int i = 0; i < stellarObjectData.Count; i++)
            {
                // Converts coordinates from strings to numbers (double)
                // To orient the plate SVGs correctly against the night sky (and the night sky background images), we need to invert the coordinates retrieved by the API call (hence the '-' operator).
                // While the reason WHY this needs to be done isn't strictly relevant to this application, it MAY have something to do with how the plates are mounted in the telescopes. 
                double xCoordinate = -double.Parse(stellarObjectData[i][0]);
                double yCoordinate = -double.Parse(stellarObjectData[i][1]);

                // When the stellarObjectData (xCoordinate and yCoordinate) is multiplied by the relativeSizeScaler, we change the size of the overall area of the dots.
                // When we add the xViewBoxMax/2 and yViewBoxMax/2 value, we are translating each dot's x,y coordinates so that they are all
                // in the positive x and positive y coordinate quadrant, which places them in the viewbox.
                double cxScaledAndTranslatedInt = xCoordinate * relativeSizeScaler + xViewBoxMax / 2;
                double cyScaledAndTranslatedInt = yCoordinate * relativeSizeScaler + yViewBoxMax / 2;

                // This may not be necessary (ToDo: test if the string conversion can be dropped)
                string cxScaledAndTranslatedString = cxScaledAndTranslatedInt.ToString();
                string cyScaledAndTranslatedString = cyScaledAndTranslatedInt.ToString();

                // Applies the correct styling to the stellar object based on its type.
                switch (stellarObjectData[i][6])
                {
                    case "GALAXY":
                        dotStrokeColor = galaxyStrokeColor;
                        dotFillColor = galaxyFillColor;
                        break;
                    case "STAR":
                        dotStrokeColor = starStrokeColor;
                        dotFillColor = starFillColor;
                        break;
                    case "QSO":
                        dotStrokeColor = qsoStrokeColor;
                        dotFillColor = qsoFillColor;
                        break;
                    default:
                        break;
                }

                svgStringBuilder.Append(

                   // This link can be used instead, to see a photo of the object in the night sky, but the image website is not very mobile friendly.
                   // Ampersands "&" in the href query string have been replaced with "&amp;" since a regular Ampersand is a escapement character in XML (svg).
                   //$"\t\t\t<a href='https://skyserver.sdss.org/dr17/VisualTools/navi?ra={stellarObjectData[i][4]}&amp;dec={stellarObjectData[i][5]}&amp;scale={photoScaler}' target='_blank'>   \n" +

                   $"\t\t\t<a href='https://skyserver.sdss.org/dr17/VisualTools/quickobj?objId={stellarObjectData[i][2]}' target='_blank'>   \n" +
                   $"\t\t\t\t<circle class='plateDot' cx='{cxScaledAndTranslatedString}' cy='{cyScaledAndTranslatedString}' r='{dotAreaScaler}' stroke='{dotStrokeColor}' stroke-width='{strokeWidth}' fill='{dotFillColor}'/>   \n" +
                   $"\t\t\t\t{stellarObjectData[i][2]}, plate: {stellarObjectData[i][3]}   \n" +
                    "\t\t\t</a>   \n"
                );
            }

            // Adds the closing SVG string to the String Builder.
            svgStringBuilder.Append(svgCloseOuter);

            // Converts the Stringbuilder to a string.
            string svgString = svgStringBuilder.ToString();

            // Suppresses "CS8602: Dereference of possibly null reference" warning for fileSavingDirectory.
            // As this application controls the directory structure, and we have already done a null check in Program.cs, we can know that svgFilePath will not be null.
            #pragma warning disable CS8602 // Dereference of a possibly null reference.

            // Creates the file directory if the directory does not already exist.  If the directory does already exist, this method does nothing.
            SvgFilePath.Directory.Create();

            // Un-suppresses 8602 warnings
            #pragma warning restore CS8602 // Dereference of a possibly null reference.

            // Writes the svg file to disk, based on the svgString.
            File.WriteAllText($"{SvgFilePath}{PlateNumber}.svg", svgString);

            return svgString;
        }

        public void CreateHtmlFromSvgFromApi(string svgStringFromApi)
        {

            ////////////////////////////////
            ///// Adjustable Constants /////
            ////////////////////////////////
            
            // Creates the Background Color of the website behind the plate svg
            string backgroundColor = "style='background-color:rgb(0,0,0);'";

            // (panzoom) Creates a limit on how far panzoom will zoom into the plate svg
            string maxScale = "maxScale: 50";

            // (panzoom) Creates a limit on how far panzoom will zoom out of the plate svg
            string minScale = "minScale: .75";

            // (panzoom) Creates a constat to adjust the Mouse wheel and Pinch zoom sensitiviety. The default is '.3'
            // I do not yet see a way to separte mouse vs. pinch zoom sensitivity
            string zoomStep = "step: 2";

            // (OpenSeaDragon) Creates a constant to adjust the Mouse wheel sensitivity while zooming. The default is 1
            double OpenSeaDragonZoomPerScroll = 2;

            ////////////////////////////////////
            ///// Non-Adjustable Constants /////
            ////////////////////////////////////

            // Creates a constant to keep the ratio of the OSD tile source width to the svgImage size constant
            double OpenSeaDragonOuterTileSourceWidthHtml = 1000 * this.OpenSeaDragonMaxZoomPixelRatio;

            ////////////////////////////////////
            ////////////////////////////////////

            // Creates a new StringBuilder object
            StringBuilder htmlStringBuilder = new StringBuilder();

            // Creates our opening and closing html strings to be tacked on to the Stringbuilder
            string htmlOpen = "" +
                "<!DOCTYPE html>   \n" +
                "<html lang='en'>   \n" +
                "<head>   \n" +
               $"\t<title>Plate {this.PlateNumber}</title>   \n" +
                "\t<meta charset='UTF-8'>   \n" +
                "\t<meta http-equiv='X-UA-Compatible' content='IE=edge'>   \n" +
                "\t<meta name='viewport' content='width=device-width, initial-scale=1.0'>   \n" +
                "\t<!-- <link href='../css/bootstrap.css' rel='stylesheet' type='text/css'> -->   \n" +
                "\t<link href='https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/css/bootstrap.min.css' rel='stylesheet' integrity='sha384-1BmE4kWBq78iYhFldvKuhfTAU6auU8tT94WrHftjDbrCEXSU1oBoqyl2QvZ6jIW3' crossorigin='anonymous'>   \n" +

                "\t<!-- The custom style sheet should only be used if you move the contents of the styling tag to it -->   \n" +
                //      The custom style sheet should only be used if you move the contents of the styling tag to it
                "\t<!-- <link href='../css/plateStyles.css' rel='stylesheet' type='text/css'> -->   \n" +
                "\t<!-- jQuery from local file should only be used if you are not using the CDN reference -->   \n" +
                //      jQuery from local file should only be used if you are not using the CDN reference
                "\t<!-- <script src='../js/jquery-3.6.0.js' type='module'></script> -->   \n" +
                "\t<script src='https://ajax.googleapis.com/ajax/libs/jquery/3.6.0/jquery.min.js'></script>   \n" +
                "\n" +

                "\t<!-- Commented out because the OpenSeaDragon library is being used instead. -->   \n" +
                // Commented out because the OpenSeaDragon library is being used instead.
                "\t<!-- <script src='https://www.unpkg.com/@panzoom/panzoom/dist/panzoom.js'></script> -->   \n" +
                "\n" +

                "\t<!-- The custom scripts file should only be used if you move the contents of the scripts tag (from the bottom of the page) to it. -->   \n" +
                //      The custom scripts file should only be used if you move the contents of the scripts tag (from the bottom of the page) to it.
                "\t<!-- <script src='../js/plateScripts.js' type='module'></script> -->   \n" +
                "\n" +

                "\t<!-- ToDo - Replace with CDN -->   \n" +
                //      ToDo - Replace with CDN
                "\t<!-- <script src='../openseadragon/openseadragon.min.js'></script> -->   \n" +
                "\t<script src='https://cdnjs.cloudflare.com/ajax/libs/openseadragon/3.0.0/openseadragon.min.js' integrity='sha512-Dq5iZeGNxm7Ql/Ix10sugr98niMRyuObKlIzKN1SzUysEXBxti479CTsCiTV00gFlDeDO3zhBsyCOO+v6QVwJw==' crossorigin='anonymous' referrerpolicy='no-referrer'></script>   \n" +
                "\t<!-- ToDo - Replace with CDN once 'svg flip' has been added to the library -->   \n" +
                //      ToDo - Replace with CDN once 'svg flip' has been added to the library
                "\t<!-- <script src='../svg-overlay-master/openseadragon-svg-overlay.js'></script> -->   \n" +
                "\t<script src='https://openseadragon.github.io/svg-overlay/openseadragon-svg-overlay.js'></script>   \n" +
                "\t<!-- <script src='../js/jquery-ui-1.13.1.custom/jquery-ui.js'></script> -->   \n" +
                "\t<link rel='stylesheet' href='https://code.jquery.com/ui/1.13.1/themes/ui-darkness/jquery-ui.css'>   \n" +
                "\t<script src='https://code.jquery.com/ui/1.13.1/jquery-ui.js'></script>   \n" +
                "\t<!-- Allows jQuery UI slider to work properly with touch -->   \n" +
                //      Allows jQuery UI slider to work properly with touch
                "\t<script type='text/javascript' src='https://cdnjs.cloudflare.com/ajax/libs/jqueryui-touch-punch/0.2.3/jquery.ui.touch-punch.min.js'></script>   \n" +
                "\n" +

                "\t<!-- Questionable CDN link - Check with Devs to get the official one -->   \n" +
                //      Questionable CDN link - Check with Devs to get the official one
                "\t<!-- <script src='https://cdnjs.cloudflare.com/ajax/libs/openseadragon/3.0.0/openseadragon.min.js' integrity='sha512-Dq5iZeGNxm7Ql/Ix10sugr98niMRyuObKlIzKN1SzUysEXBxti479CTsCiTV00gFlDeDO3zhBsyCOO+v6QVwJw==' crossorigin='anonymous' referrerpolicy='no-referrer'></script> -->   \n" +
                "\n" +

                "\t<style>   \n" +
                "\t\t#headerWrapper {   \n" +
                "\t\t\theight: fit-content;   \n" +
                "\t\t\tposition: -webkit-sticky;   \n" +
                "\t\t\tposition: sticky;   \n" +
                "\t\t\ttop: 0;   \n" +
                "\t\t\tdisplay: grid;   \n" +
                "\t\t\t/* To keep the header on top of the plate */   \n" +
                //        To keep the header on top of the plate
                "\t\t\tz-index: 1;   \n" +
                "\t\t}   \n" +
                "\n" +

                "\t\t#headerBackgroundBox {   \n" +
                "\t\t\tbackground-color: rgb(46, 46, 46);   \n" +
                "\t\t\tmargin: auto;   \n" +
                "\t\t\tpadding: 10px 20px;   \n" +
                "\t\t}   \n" +
                "\t\t@media (max-width: 400px) {   \n" +
                "\t\t\t#headerBackgroundBox {   \n" +
                "\t\t\t\tpadding: 10px 10px;   \n" +
                "\t\t\t}   \n" +
                "\t\t}   \n" +
                "\n" +

                "\t\t#headerTextBox {   \n" +
                "\t\t\tdisplay: flex;   \n" +
                "\t\t\tjustify-content: center;   \n" +
                "\t\t\talign-items: center;   \n" +
                "\t\t}   \n" +
                "\t\t@media (max-width: 520px) {   \n" +
                "\t\t\t#headerTextBox {   \n" +
                "\t\t\t\tdisplay: block;   \n" +
                "\t\t\t}   \n" +
                "\t\t}    \n" +
                "\n" +

                "\t\t.headerText {   \n" +
                "\t\t\tdisplay: flex;   \n" +
                "\t\t\tjustify-content: center;   \n" +
                "\t\t\talign-items: center;   \n" +
                "\t\t\twhite-space: nowrap;   \n" +
                "\t\t\tfont-size: 20px;   \n" +
                "\t\t\tcolor: rgb(220, 220, 220);   \n" +
                "\t\t\tpadding: 0px 5px;   \n" +
                "\t\t}   \n" +
                "\n" +

                "\t\t#plateKey {   \n" +
                "\t\t\tdisplay: flex;   \n" +
                "\t\t\tjustify-content: center;   \n" +
                "\t\t\tmax-width: 480px;   \n" +
                "\t\t\theight: 30px;   \n" +
                "\t\t\tmargin: 5px 0px;   \n" +
                "\t\t}   \n" +
                "\t\t@media (max-width: 599px) {   \n" +
                "\t\t\t#plateKey {   \n" +
                "\t\t\t\tjustify-content: space-around;   \n" +
                "\t\t\t}   \n" +
                "\t\t}   \n" +
                "\n" +

                "\t\t.keyItem {   \n" +
                "\t\t\tdisplay: flex;   \n" +
                "\t\t\tjustify-content: center;   \n" +
                "\t\t\talign-items: center;   \n" +
                "\t\t\tmargin: 0px 20px;   \n" +
                "\t\t}   \n" +
                "\t\t@media (max-width: 599px) {   \n" +
                "\t\t\t.keyItem {   \n" +
                "\t\t\t\tmargin: 0px 0px;   \n" +
                "\t\t\t}   \n" +
                "\t\t}   \n" +
                "\n" +

                "\t\t.keyDot {   \n" +
                "\t\t\tmin-width: 20px;   \n" +
                "\t\t\tmin-height: 30px;   \n" +
                "\t\t}   \n" +
                "\n" +

                "\t\t.keyText {   \n" +
                "\t\t\tpadding-left: 10px;   \n" +
                "\t\t\tpadding-right: 10px;   \n" +
                "\t\t\tcolor: rgb(220, 220, 220);   \n" +
                "\t\t}   \n" +
                "\n" +

                "\t\t#plateButtonBox {   \n" +
                "\t\t\tdisplay: flex;   \n" +
                "\t\t\tjustify-content: center;   \n" +
                "\t\t\tpadding-top: 5px;   \n" +
                "\t\t}   \n" +
                "\t\t@media (max-width: 599px) {   \n" +
                "\t\t\t#plateButtonBox {   \n" +
                "\t\t\t\tjustify-content: space-around;   \n" +
                "\t\t\t}   \n" +
                "\t\t}   \n" +
                "\n" +

                "\t\t.plateButton {   \n" +
                "\t\t\tmargin: 0px 20px;   \n" +
                "\t\t\tbackground-color: lightgray;   \n" +
                "\t\t\tborder-radius: 8px;   \n" +
                "\t\t}   \n" +
                "\t\t@media (max-width: 599px) {   \n" +
                "\t\t\t.plateButton {   \n" +
                "\t\t\t\tmargin: 0px 0px;   \n" +
                "\t\t\t}   \n" +
                "\t\t}   \n" +

                "\t\t#plateNumberDisplayWrapper {   \n" +
                "\t\t\theight: 0;   \n" +
                "\t\t}   \n" +
                "\n" +

                "\t\t/* Original panzoom version */   \n" +
                //      Original panzoom version
                "\t\t/*#plateNumberDisplay {   \n" +
                "\t\t\tposition: relative;   \n" +
                "\t\t\ttop: 20px;   \n" +
                "\t\t\tcolor: rgb(215,215,215);   \n" +
                "\t\t}*/   \n" +
                "\n" +

                "\t\t/* OpenSeaDragon version */   \n" +
                //      OpenSeaDragon version
                "\t\t#plateNumberDisplay {   \n" +
                "\t\t\tposition: relative;   \n" +
                "\t\t\ttop: 20px;   \n" +
                "\t\t\tmargin-left: 20px;   \n" +
                "\t\t\tfont-size: 20px;   \n" +
                "\t\t\tcolor: rgb(215,215,215);   \n" +
                "\t\t\tz-index: 1;   \n" +
                "\t\t}   \n" +
                "\n" +

                "\t\t/* Probably not useful */   \n" +
                //      Probably not useful
                "\t\t#skyImageBoxBackgroundImage {   \n" +
                "\t\t\timage-rendering: smooth;   \n" +
                "\t\t}   \n" +
                "\n" +

                "\t\t#openSeaDragonWrapper {   \n" +
                "\t\t\twidth: 100%;   \n" +
                "\t\t\t/*border-style: solid;*/   \n" +
                "\t\t\t/*border-width: 2px;*/   \n" +
                "\t\t\t/*border-color: crimson;*/   \n" +
                "\t\t}   \n" +
                "\n" +

                "\t\t#slider {   \n" +
                "\t\t\tdisplay: relative;   \n" +
                "\t\t\ttop: 10px;   \n" +
                "\t\t\tmargin: 0 20%;   \n" +
                "\t\t\theight: 30px;   \n" +
                "\t\t\tz-index: 1;   \n" +
                "\t\t}   \n" +
                "\n" +

                "\t\t.ui-slider-handle {   \n" +
                "\t\t\tdisplay: flex;   \n" +
                "\t\t\tjustify-content: center;   \n" +
                "\t\t}   \n" +
                "\n" +

                "\t</style>   \n" +
                "\n" +

                "</head>   \n" +
               $"<body {backgroundColor}>   \n" +
                "\n" +

                //      This is the Plate Header
                 "\t<!--This is the Plate Header-->   \n" +
                "\t<div class='container'>   \n" +
                "\t\t<div id='headerWrapper'>   \n" +
                "\t\t\t<div id='headerBackgroundBox'>   \n" +
                "\t\t\t\t<div id='headerTextBox'>   \n" +
                "\t\t\t\t\t<div class='headerText'>   \n" +
                "\t\t\t\t\t\tZoom, Pan, and Explore.   \n" +
                "\t\t\t\t\t</div>   \n" +
                "\t\t\t\t\t<div class='headerText'>   \n" +
                "\t\t\t\t\t\tTap a Dot to Learn More.   \n" +
                "\t\t\t\t\t</div>   \n" +
                "\t\t\t\t</div>   \n" +
                "\t\t\t\t<div id='plateKey'>   \n" +
                "\t\t\t\t\t<div id='plateHeader1' class='keyItem'>   \n" +
                "\t\t\t\t\t\t<svg class='keyDot' width='auto' height='100%' viewBox='0 0 50 50'>   \n" +
                "\t\t\t\t\t\t\t<circle cx='25' cy='25' r='20px' stroke='rgb(255,0,132)' stroke-width='10px' fill='rgb(255,255,255)'/>   \n" +
                "\t\t\t\t\t\t</svg>   \n" +
                "\t\t\t\t\t\t<div class='keyText'>   \n" +
                "\t\t\t\t\t\t\tGalaxy   \n" +
                "\t\t\t\t\t\t</div>   \n" +
                "\t\t\t\t\t</div>   \n" +
                "\t\t\t\t\t<div id='plateHeader2' class='keyItem'>   \n" +
                "\t\t\t\t\t\t<svg class='keyDot' width='auto' height='100%' viewBox='0 0 50 50'>   \n" +
                "\t\t\t\t\t\t\t<circle cx='25' cy='25' r='20px' stroke='rgb(0,181,255)' stroke-width='10px' fill='rgb(255,255,255)'/>   \n" +
                "\t\t\t\t\t\t</svg>   \n" +
                "\t\t\t\t\t\t<div class='keyText'>   \n" +
                "\t\t\t\t\t\t\tStar   \n" +
                "\t\t\t\t\t\t</div>   \n" +
                "\t\t\t\t\t</div>   \n" +
                "\t\t\t\t\t<div id='plateHeader3' class='keyItem'>   \n" +
                "\t\t\t\t\t\t<svg class='keyDot' width='auto' height='100%' viewBox='0 0 50 50'>   \n" +
                "\t\t\t\t\t\t\t<circle cx='25' cy='25' r='20px' stroke='rgb(102,255,0)' stroke-width='10px' fill='rgb(255,255,255)'/>   \n" +
                "\t\t\t\t\t\t</svg>   \n" +
                "\t\t\t\t\t\t<div class='keyText'>   \n" +
                "\t\t\t\t\t\t\tQuasar   \n" +
                "\t\t\t\t\t\t</div>   \n" +
                "\t\t\t\t\t</div>   \n" +
                "\t\t\t\t</div>   \n" +
                "\t\t\t\t<div id='plateButtonBox'>   \n" +
                "\t\t\t\t\t<button id='plateResetPanZoomButton' class='plateButton'>Reset: Pan & Zoom</button>   \n" +
                "\t\t\t\t\t<button id='plateFlipButton' class='plateButton'>Flip Plate</button>   \n" +
                "\t\t\t\t\t<button id='backgroundImageButton' class='plateButton'>Background</button>   \n" +
                "\t\t\t\t</div>   \n" +
                "\t\t\t\t<!-- Original panzoom version -->   \n" +
                //            Original panzoom version
                "\t\t\t\t<!-- <div id='plateNumberDisplayWrapper'>   \n" +
                "\t\t\t\t\t<div id='plateNumberDisplay'>   \n" +
               $"\t\t\t\t\t\tPlate {this.PlateNumber}   \n" +
                "\t\t\t\t\t</div>   \n" +
                "\t\t\t\t</div> -->   \n" +
                "\t\t\t</div>   \n" +
                "\t\t</div>   \n" +
                "\t</div>   \n" +
                "\n" +

                "\t<div id='openSeaDragonWrapper'>   \n" +
                "\t\t<div id='slider'></div>   \n" +
                "\t\t<div class='container'>   \n" +
                "\t\t\t<div id='plateNumberDisplayWrapper'>   \n" +
                "\t\t\t\t<div id='plateNumberDisplay' >   \n" +
               $"\t\t\t\t\tPlate {this.PlateNumber}   \n" +
                "\t\t\t\t</div>   \n" +
                "\t\t\t</div>   \n" +
                "\t\t</div>   \n" +
                "\t</div>   \n" +

                "\n" +

                "\t<div id='svgWrapper' style='display: flex; justify-content: center; align-items: center'>   \n" +
                "";

            string htmlClose = "" +
                "\t</div>   \n" +
                "\t<script>   \n" +
                "\t//If you want to run scripts from the plateScripts.js file, you will also probably need to move all your scripts from here to the plateScript.js file.   \n" +
                "\t//Trying to use scripts both in the html and the plateScripts.js file causes jquery reference errors because things won't load in the correct order.   \n" +
                //   If you want to run scripts from the plateScripts.js file, you will also probably need to move all your scripts from here to the plateScript.js file.
                //   Trying to use scripts both in the html and the plateScripts.js file causes jquery reference errors because things won't load in the correct order.
                "\n" +

                "\t\t// Changes height of #svgImage on screen resize (and page load) so that the svg doesn't overflow   \n" +
                //      Changes height of #svgImage on screen resize (and page load) so that the svg doesn't overflow
                "\t\t$(window).resize(function() {   \n" +
                "\t\t\t$('#svgImage').height((visualViewport.height - $('#headerWrapper').height()));   \n" +
                "\t\t}).resize();   \n" +
                "\n" +

                "\t\t// Changes height of #OpenSeaDragonWrapper on screen resize (and page load) so that the image doesn't overflow   \n" +
                //      Changes height of #OpenSeaDragonWrapper on screen resize (and page load) so that the image doesn't overflow
                "\t\t$(window).resize(function() {   \n" +
                "\t\t\t$('#openSeaDragonWrapper').height((visualViewport.height - $('#headerWrapper').height()) -20);   \n" +
                "\t\t}).resize();   \n" +
                "\n" +

                "\t\t// panzoom reset logic   \n" +
                "\t\t// Commented out because the OpenSeaDragon library is being used instead.   \n" +
                //      panzoom reset logic
                //      Commented out because the OpenSeaDragon library is being used instead.
                "\t\t//$('#plateResetPanZoomButton').click(function() {   \n" +
                "\t\t\t//panzoom.reset();   \n" +
                "\t\t//});   \n" +
                "\n" +

                "\t\t// Plate flipping logic   \n" +
                //      Plate flipping logic
                "\t\t$('#plateFlipButton').click(function() {   \n" +
                "\n" +

                "\t\t\t// Triggers a plate reset so that the flip animates   \n" +
                "\t\t\t// Commented out because the OpenSeaDragon library is being used instead.   \n" +
                //        Triggers a plate reset so that the flip animates
                //        Commented out because the OpenSeaDragon library is being used instead.
                "\t\t\t//panzoom.reset();   \n" +
                "\n" +

                "\t\t\t// Does the flipping and highlights button   \n" +
                //        Does the flipping and highlights button
                "\t\t\tif($('#svgWrapper').hasClass('plateflipped')) {   \n" +
                "\t\t\t\t$('#svgWrapper').removeClass('plateflipped');   \n" +
                "\t\t\t\t$('#plateFlipButton').css('background-color', 'lightgray');   \n" +
                "\t\t\t}   \n" +
                "\t\t\telse {   \n" +
                "\t\t\t\t$('#svgWrapper').addClass('plateflipped');   \n" +
                "\t\t\t\t$('#plateFlipButton').css('background-color', 'rgb(134, 134, 134)');   \n" +
                "\t\t\t}   \n" +
                "\n" +

                "\t\t});   \n" +
                "\n" +

                "\t\t// Plate background image logic   \n" +
                //      Plate background image logic
                "\t\t$('#backgroundImageButton').click(function() {   \n" +
                "\n" +

                "\t\t\t// Turns on the skyImage background and hilights button   \n" +
                //        Turns on the skyImage background and hilights button
                "\t\t\tif($('#skyImageBox').hasClass('skyImageOn')) {   \n" +
                "\t\t\t\t$('#skyImageBox').removeClass('skyImageOn');   \n" +
                "\t\t\t\t$('#skyImageBox').hide();   \n" +
                "\t\t\t\t$('#backgroundImageButton').css('background-color', 'lightgray');   \n" +
                "\t\t\t}   \n" +
                "\t\t\t// Turns off the skyImage background and un-hilights button   \n" +
                //        Turns off the skyImage background and un-hilights button
                "\t\t\telse {   \n" +
                "\t\t\t\t$('#skyImageBox').addClass('skyImageOn');   \n" +
                "\t\t\t\t$('#skyImageBox').show();   \n" +
                "\t\t\t\t$('#backgroundImageButton').css('background-color', 'rgb(134, 134, 134)');   \n" +
                "\t\t\t}   \n" +
                "\n" +

                "\t\t\t// Turns off the plate fill (so that it doesn't cover the skyImage background)   \n" +
                //        Turns off the plate fill (so that it doesn't cover the skyImage background)
                "\t\t\tif ($('#backgroundFill').hasClass('fillOn')) {   \n" +
                "\t\t\t\t$('#backgroundFill').removeClass('fillOn')   \n" +
                "\t\t\t\t$('#backgroundFill').hide();   \n" +
                "\t\t\t}   \n" +
                "\t\t\t// Turns on the plate fill   \n" +
                //        Turns on the plate fill
                "\t\t\telse {   \n" +
                "\t\t\t\t$('#backgroundFill').addClass('fillOn')   \n" +
                "\t\t\t\t$('#backgroundFill').show();   \n" +
                "\t\t\t}   \n" +
                "\n" +

                "\t\t});   \n" +
                "\n" +

                "\t\t// Commented out because the OpenSeaDragon library is being used instead.   \n" +
                "\t\t// panzoom library settings   \n" +
                //      Commented out because the OpenSeaDragon library is being used instead.
                //      panzoom library settings
                "\t\t//const element = document.getElementById('svgWrapper');   \n" +
                "\t\t//const panzoom = Panzoom(element, {   \n" +
                "\t\t\t//// options here   \n" +
                //          options here
               $"\t\t\t//{maxScale},   \n" +
               $"\t\t\t//{minScale},   \n" +
               $"\t\t\t//{zoomStep},   \n" +
                "\t\t\t////determines how the transorms function.   \n" +
                //         determines how the transorms function.
                "\t\t\t//setTransform: (_, { scale, x, y }) => {   \n" +
                "\t\t\t\t////You need a different setStyle property depending on if you flip the plate or not.   \n" +
                //           You need a different setStyle property depending on if you flip the plate or not.
                "\t\t\t\t//if($('#svgWrapper').hasClass('plateflipped')){   \n" +
                "\t\t\t\t\t//// If you put `-${x}` into the translate property below, you can not pan the svg into negative x values.   \n" +
                //              If you put `-${x}` into the translate property below, you can not pan the svg into negative x values.
                "\t\t\t\t\t////Instead you must calculate the negative value of x before pluggin it in.   \n" +
                //             Instead you must calculate the negative value of x before pluggin it in.
                "\t\t\t\t\t//let negativeX = -x;   \n" +
                "\t\t\t\t\t//panzoom.setStyle('transform', `scale(-${scale},${scale}) translate(${negativeX}px, ${y}px)`)   \n" +
                "\t\t\t\t//}   \n" +
                "\t\t\t\t//else {   \n" +
                "\t\t\t\t\t//panzoom.setStyle('transform', `scale(${scale},${scale}) translate(${x}px, ${y}px)`)   \n" +
                "\t\t\t\t//}   \n" +
                "\t\t\t//}   \n" +
                "\t\t//});   \n" +
                "\n" +

                "\t\t// Commented out because the OpenSeaDragon library is being used instead.   \n" +
                //      Commented out because the OpenSeaDragon library is being used instead.
                "\t\t//// enable mouse wheel   \n" +
                //        enable mouse wheel 
                "\t\t//const parent = element.parentElement;   \n" +
                "\t\t//parent.addEventListener('wheel', panzoom.zoomWithWheel);   \n" +
                "\n" +
                "\t\t// OpenSeadragon settings   \n" +
                //      OpenSeadragon settings
                "\t\tvar viewer = OpenSeadragon({   \n" +
                "\t\t\tid: 'openSeaDragonWrapper',   \n" +
                "\t\t\tprefixUrl: '../openseadragon/images/',   \n" +
                "\t\t\tdefaultZoomLevel: 0,   \n" +
                "\t\t\tpreload: true,   \n" +
                "\t\t\timmediateRender: true,   \n" +
                "\t\t\thomeButton: 'plateResetPanZoomButton',   \n" +
                "\t\t\tflipButton: 'plateFlipButton',   \n" +
                "\t\t\tshowFlipControl: true,   \n" +
                "\t\t\tshowRotationControl: true,   \n" +
                "\t\t\t//showNavigator: true,   \n" +
                "\t\t\t// Degrees set to != 0 to prevent bug -- Where while the tiled image is flipped,   \n" +
                "\t\t\t// if you zoom in beyond the max pixel ratio (maxZoomPixelRation) of 1.1,   \n" +
                "\t\t\t// the tiled image flips back to its original orientation.   \n" +
                //        Degrees set to != 0 to prevent bug -- Where while the tiled image is flipped
                //        if you zoom in beyond the max pixel ratio (maxZoomPixelRation) of 1.1,
                //        the tiled image flips back to its original orientation.
                "\t\t\tdegrees: .000001,   \n" +
               $"\t\t\tmaxZoomPixelRatio: {this.OpenSeaDragonMaxZoomPixelRatio},   \n" +
               $"\t\t\tzoomPerScroll: {OpenSeaDragonZoomPerScroll},   \n" +
                "\t\t\ttileSources: [   \n" +
                "\t\t\t\t{   \n" +
               $"\t\t\t\t\twidth: {OpenSeaDragonOuterTileSourceWidthHtml},   \n" +
                "\t\t\t\t\ttileSource: {   \n" +
                "\t\t\t\t\t\t//Required   \n" +
                //             Required
                "\t\t\t\t\t\ttype: 'zoomifytileservice',   \n" +
                "\t\t\t\t\t\twidth: 2048,   \n" +
                "\t\t\t\t\t\theight: 2048,   \n" +
                "\t\t\t\t\t\ttilesUrl:   \n" +
               $"\t\t\t\t\t\t\t'../assets/Zoomify_Images-8.05-2048/{this.PlateNumber}/',   \n" +
                "\t\t\t\t\t\t//Optional   \n" +
                //             Optional
                "\t\t\t\t\t\ttileSize: 256,   \n" +
                "\t\t\t\t\t\tfileFormat: 'jpg'   \n" +
                "\t\t\t\t\t},   \n" +
                "\t\t\t\t},   \n" +
                "\t\t\t],   \n" +
                "\t\t});   \n" +
                "\n" +

                "\t\tvar overlay = viewer.svgOverlay();   \n" +
                "\t\toverlay.node().append(svgImage)   \n" +
                "\n" +

                "\t\tnew OpenSeadragon.MouseTracker({   \n" +
                "\t\t\telement: overlay._node,   \n" +
                "\t\t\tclickHandler: function(event) {   \n" +
                "\t\t\t\tvar target = event.originalTarget.parentNode;   \n" +
                "\t\t\t\tif (target.matches('a')) {   \n" +
                "\t\t\t\t\tif (target.getAttribute('target') === '_blank') {   \n" +
                "\t\t\t\t\t\twindow.open(target.getAttribute('href'));   \n" +
                "\t\t\t\t\t} else {   \n" +
                "\t\t\t\t\t\tlocation.href = target.getAttribute('href');   \n" +
                "\t\t\t\t\t}   \n " +
                "\t\t\t\t}   \n " +
                "\t\t\t}   \n " +
                "\t\t});   \n " +
                "\n" +

                "\t\t// jQuery UI slider scripts   \n" +
                //      jQuery UI slider scripts
                "\t\t$('#slider').slider({   \n" +
                "\t\t\t// min and max values can not be 0 or 360 or an OpenSeadragon bug is triggered where the tile image flips when zooming in past a maxZoomPixelRation of 1.1   \n" +
                //        min and max values can not be 0 or 360 or an OpenSeadragon bug is triggered where the tile image flips when zooming in past a maxZoomPixelRation of 1.1
                "\t\t\tmin: 0.000001,   \n" +
                "\t\t\tmax: 359.999999,   \n" +
                "\t\t\tstep: .000001   \n" +
                "\t\t});   \n " +
                "\n" +

                "\t\t//This edits the size and placement of the slider handle.  Doing it in the styles doesn't seem to work.   \n" +
                //     This edits the size and placement of the slider handle.  Doing it in the styles doesn't seem to work.
                "\t\t$('.ui-slider-handle')   \n" +
                "\t\t\t.height(30)   \n" +
                "\t\t\t.width(70)   \n" +
                "\t\t\t.css('margin-left', '-35px')   \n" +
                "\t\t\t.css('margin-top', '3px')   \n" +
                "\t\t\t.text('Rotate')   \n" +
                "\t\t\t.css('color', 'lightgray');   \n " +
                "\n" +

                "\t\t// Sets the rotation angle of the SVG to that of the value of the slider   \n" +
                //      Sets the rotation angle of the SVG to that of the value of the slider
                "\t\t$('#slider').slider({slide: function(event, ui) {   \n" +
                "\t\t\t\tviewer.viewport.setRotation(ui.value);   \n" +
                "\t\t\t\tconsole.log('jQuery Script', ui.value)   \n" +
                "\t\t\t}   \n" +
                "\t\t});   \n " +
                "\n" +

                "\t</script>   \n" +
                "</body>   \n" +
                "</html>   \n" +
                "";

            // Adds the opening html string, then the svgString, and finally the closing html string to the Stringbuilder
            htmlStringBuilder.Append(htmlOpen);
            htmlStringBuilder.Append(svgStringFromApi);
            htmlStringBuilder.Append(htmlClose);

            // Converts the Stringbuilder to a string
            string htmlString = htmlStringBuilder.ToString();

            // Suppresses "CS8602: Dereference of possibly null reference" warning for fileSavingDirectory.
            // As this application controls the directory structure, and we have already done a null check in Program.cs, we can know that svgFilePath will not be null.
            #pragma warning disable CS8602 // Dereference of a possibly null reference.

            // Creates the file directory if the directory does not already exist.  If the directory does already exist, this method does nothing.
            HtmlFilePath.Directory.Create();

            // Un-suppresses 8602 warnings
            #pragma warning restore CS8602 // Dereference of a possibly null reference.

            // Writes the html file to disk based on the htmlString, which in turn is based partially on the svgString
            File.WriteAllText($"{HtmlFilePath}{PlateNumber}.html", htmlString);

        }

    }
}