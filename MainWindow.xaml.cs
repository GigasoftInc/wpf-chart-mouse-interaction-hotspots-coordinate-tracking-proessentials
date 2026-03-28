using System;
using System.Windows;
using System.Windows.Input;
using Gigasoft.ProEssentials;
using Gigasoft.ProEssentials.Enums;

namespace MouseInteractionHotspots
{
    /// <summary>
    /// ProEssentials WPF — Mouse Interaction: Coordinate Tracking &amp; Hotspots
    ///
    /// Combines two complementary mouse-interaction techniques in a single
    /// PegoWpf dual-Y-axis chart:
    ///
    /// TECHNIQUE 1 — ConvPixelToGraph tooltip (Example 007 technique):
    ///   The PeCustomTrackingDataText event handler converts the mouse pixel
    ///   position to graph coordinates using ConvPixelToGraph — called twice,
    ///   once for the left Y axis and once for the right Y axis. The result
    ///   is a tooltip that shows both axis values at the cursor position,
    ///   interpolated continuously between data points.
    ///   Best for: reading exact values at any position, works between points.
    ///
    /// TECHNIQUE 2 — GetHotSpot status bar (Example 014 technique):
    ///   The MouseMove handler calls GetHotSpot() on every mouse movement.
    ///   When the cursor is over a named chart element (data point, series
    ///   legend, point label), the status bar shows a plain-language
    ///   description. When not directly over an element, SearchSubsetPointIndex
    ///   finds and reports the nearest data point.
    ///   Best for: identifying chart elements by name, implementing click actions.
    ///
    /// Data: 4 subsets × 24 monthly points (2 years of regional sales data)
    ///   Subsets 0–2: North, South, West  — left Y axis  (units sold)
    ///   Subset   3:  Market Share         — right Y axis (percentage)
    ///   RYAxisComparisonSubsets = 1 assigns the last subset to the right Y.
    ///
    /// Controls:
    ///   Mouse move       — status bar updates + tooltip tracks both Y values
    ///   Left-click drag  — zoom box
    ///   Right-click      — context menu (export, print, customize)
    /// </summary>
    public partial class MainWindow : Window
    {
        // Colors taken from the audio waveform dark theme palette
        static readonly System.Windows.Media.Color CyanColor   = System.Windows.Media.Color.FromArgb(255,   0, 229, 229); // #00E5E5
        static readonly System.Windows.Media.Color GreenColor  = System.Windows.Media.Color.FromArgb(255,   0, 255,   0); // #00FF00
        static readonly System.Windows.Media.Color RedColor    = System.Windows.Media.Color.FromArgb(255, 255,  48,  48); // #FF3030
        static readonly System.Windows.Media.Color GoldColor   = System.Windows.Media.Color.FromArgb(255, 255, 210,   0); // #FFD200

        public MainWindow()
        {
            InitializeComponent();
        }

        // -----------------------------------------------------------------------
        // Pego1_Loaded — chart initialization
        //
        // Always initialize ProEssentials in the control's Loaded event.
        // Do NOT initialize in the Window's Loaded event — the window fires
        // before the control is fully initialized.
        // -----------------------------------------------------------------------
        void Pego1_Loaded(object sender, RoutedEventArgs e)
        {
            // =======================================================================
            // Step 1 — Data
            //
            // 4 subsets × 24 monthly points (Jan 2023 – Dec 2024).
            //   Subsets 0–2: regional sales (left Y, units 150–850)
            //   Subset  3:   market share   (right Y, percent 25–75)
            //
            // RYAxisComparisonSubsets = 1 assigns the last subset (index 3) to
            // the right Y axis. MethodII controls its plotting style.
            // =======================================================================
            Pego1.PeData.Subsets = 4;
            Pego1.PeData.Points  = 24;

            var rand = new Random(42);

            for (int p = 0; p < 24; p++)
            {
                // North — strong seasonal peak in summer months
                Pego1.PeData.Y[0, p] = 500f
                    + (float)(Math.Sin((p + 3) * Math.PI / 6.0) * 220f)
                    + (float)(rand.NextDouble() * 60 - 30);

                // South — inverse seasonality (strong in winter)
                Pego1.PeData.Y[1, p] = 420f
                    + (float)(Math.Sin((p + 9) * Math.PI / 6.0) * 160f)
                    + (float)(rand.NextDouble() * 60 - 30);

                // West — gradual upward trend with mild oscillation
                Pego1.PeData.Y[2, p] = 280f + p * 8f
                    + (float)(Math.Sin(p * Math.PI / 4.0) * 80f)
                    + (float)(rand.NextDouble() * 50 - 25);

                // Market Share % — right Y axis, 25–72 range
                Pego1.PeData.Y[3, p] = 48f
                    + (float)(Math.Sin((p + 1) * Math.PI / 5.0) * 18f)
                    + (float)(rand.NextDouble() * 8 - 4);
            }

            // =======================================================================
            // Step 2 — Point labels (month names)
            // =======================================================================
            string[] months = {
                "Jan 23","Feb 23","Mar 23","Apr 23","May 23","Jun 23",
                "Jul 23","Aug 23","Sep 23","Oct 23","Nov 23","Dec 23",
                "Jan 24","Feb 24","Mar 24","Apr 24","May 24","Jun 24",
                "Jul 24","Aug 24","Sep 24","Oct 24","Nov 24","Dec 24"
            };
            for (int p = 0; p < 24; p++)
                Pego1.PeString.PointLabels[p] = months[p];

            // =======================================================================
            // Step 3 — Subset labels
            // =======================================================================
            Pego1.PeString.SubsetLabels[0] = "North";
            Pego1.PeString.SubsetLabels[1] = "South";
            Pego1.PeString.SubsetLabels[2] = "West";
            Pego1.PeString.SubsetLabels[3] = "Market Share";

            // =======================================================================
            // Step 4 — Dual Y-axis
            //
            // RYAxisComparisonSubsets = 1 assigns the last 1 subset (index 3)
            // to the right Y axis. MethodII sets its independent plotting style.
            // PeColor.RYAxis syncs the right Y axis color to the subset color.
            // =======================================================================
            Pego1.PePlot.RYAxisComparisonSubsets = 1;
            Pego1.PePlot.Method                  = GraphPlottingMethod.PointsPlusSpline;
            Pego1.PePlot.MethodII                = GraphPlottingMethodII.Line;

            Pego1.PeString.YAxisLabel  = "Units Sold";
            Pego1.PeString.RYAxisLabel = "Market Share (%)";
            Pego1.PeString.XAxisLabel  = "Month";

            // =======================================================================
            // Step 5 — Colors (audio waveform dark palette)
            // =======================================================================
            Pego1.PeColor.SubsetColors[0] = CyanColor;
            Pego1.PeColor.SubsetColors[1] = GreenColor;
            Pego1.PeColor.SubsetColors[2] = RedColor;
            Pego1.PeColor.SubsetColors[3] = GoldColor;

            // Sync right Y axis label/grid color to the Market Share subset color
            Pego1.PeColor.RYAxis = GoldColor;

            // =======================================================================
            // Step 6 — Hotspot configuration (Example 014 technique)
            //
            // HotSpot.Data    — makes data points hoverable (GetHotSpot returns
            //                   HotSpotType.DataPoint when over a point marker)
            // HotSpot.Subset  — makes series legend entries hoverable
            // HotSpot.Point   — makes X-axis point labels hoverable
            // HotSpot.Size    — Large gives a generous hit area around each element
            //
            // MouseCursorControl = true: clicking a data point snaps the cursor
            //   to that exact point position.
            // MouseCursorControlClosestPoint = true: no click required — the cursor
            //   snaps to the nearest point as the mouse moves near it.
            // =======================================================================
            Pego1.PeUserInterface.HotSpot.Data   = true;
            Pego1.PeUserInterface.HotSpot.Subset  = true;
            Pego1.PeUserInterface.HotSpot.Point   = true;
            Pego1.PeUserInterface.HotSpot.Size    = HotSpotSize.Large;

            Pego1.PeUserInterface.Cursor.Mode                        = CursorMode.DataCross;
            Pego1.PeUserInterface.Cursor.MouseCursorControl           = true;
            Pego1.PeUserInterface.Cursor.MouseCursorControlClosestPoint = true;

            // =======================================================================
            // Step 7 — Tooltip configuration (Example 007 technique)
            //
            // PromptTracking = true enables the tooltip mechanism.
            // PromptLocation = ToolTip places the result in the standard Windows
            //   tooltip popup that follows the cursor.
            // TrackingCustomDataText = true activates the PeCustomTrackingDataText
            //   event — our handler overrides the tooltip string with the dual-Y
            //   coordinate output from ConvPixelToGraph.
            // TrackingTooltipMaxWidth prevents very long strings from wrapping oddly.
            // =======================================================================
            Pego1.PeUserInterface.Cursor.PromptTracking          = true;
            Pego1.PeUserInterface.Cursor.PromptLocation          = CursorPromptLocation.ToolTip;
            Pego1.PeUserInterface.Cursor.PromptStyle             = CursorPromptStyle.XYValues;
            Pego1.PeUserInterface.Cursor.TrackingCustomDataText  = true;
            Pego1.PeUserInterface.Cursor.TrackingTooltipMaxWidth = 260;

            Pego1.PePlot.MarkDataPoints = true;

            // =======================================================================
            // Step 8 — Zoom
            // =======================================================================
            Pego1.PeUserInterface.Allow.Zooming  = AllowZooming.HorzAndVert;
            Pego1.PeUserInterface.Allow.ZoomStyle = ZoomStyle.Ro2Not;

            Pego1.PePlot.Option.MinimumPointSize = MinimumPointSize.Small;
            Pego1.PePlot.Option.MaximumPointSize = MinimumPointSize.Large;

            // =======================================================================
            // Step 9 — Style (audio waveform dark palette)
            // =======================================================================
            Pego1.PeColor.BitmapGradientMode = true;
            Pego1.PeColor.QuickStyle         = QuickStyle.DarkNoBorder;
            Pego1.PeConfigure.BorderTypes    = TABorder.DropShadow;

            Pego1.PeGrid.InFront     = true;
            Pego1.PeGrid.LineControl = GridLineControl.Both;
            Pego1.PeGrid.Style       = GridStyle.Dot;
            Pego1.PePlot.DataShadows = DataShadows.Shadows;

            // =======================================================================
            // Step 10 — Titles
            // =======================================================================
            Pego1.PeString.MainTitle = "Regional Sales Performance";
            Pego1.PeString.SubTitle  = "Tooltip: both Y values at cursor   \u00b7   Status bar: named element under cursor";

            Pego1.PeFont.FontSize      = Gigasoft.ProEssentials.Enums.FontSize.Large;
            Pego1.PeFont.Fixed         = true;
            Pego1.PeFont.MainTitle.Bold = true;

            // =======================================================================
            // Step 11 — Rendering quality
            // =======================================================================
            Pego1.PeConfigure.AntiAliasGraphics = true;
            Pego1.PeConfigure.RenderEngine      = RenderEngine.Direct2D;
            Pego1.PeConfigure.ImageAdjustLeft   = 25;

            // =======================================================================
            // Step 12 — ReinitializeResetImage
            //
            // Always call as the final step. Applies all properties and renders
            // the first image.
            // =======================================================================
            Pego1.PeFunction.ReinitializeResetImage();
            Pego1.Invalidate();
        }

        // -----------------------------------------------------------------------
        // Pego1_MouseMove — GetHotSpot status bar (Example 014 technique)
        //
        // Fires on every mouse movement over the chart control. Calls
        // GetHotSpot() to identify which named element is under the cursor,
        // then writes a plain-language description to the status TextBlock.
        //
        // When not directly over a named element, SearchSubsetPointIndex
        // performs a proximity search and reports the nearest data point.
        //
        // HotSpotData struct fields:
        //   ds.Type  — HotSpotType enum (DataPoint, Subset, Point, None, ...)
        //   ds.Data1 — primary index   (subset index for DataPoint/Subset,
        //                               point index for Point)
        //   ds.Data2 — secondary index (point index for DataPoint)
        // -----------------------------------------------------------------------
        private void Pego1_MouseMove(object sender, MouseEventArgs e)
        {
            Gigasoft.ProEssentials.Structs.HotSpotData ds = Pego1.PeFunction.GetHotSpot();

            if (ds.Type == HotSpotType.DataPoint)
            {
                // Directly over a plotted data point
                float val    = Pego1.PeData.Y[ds.Data1, ds.Data2];
                string label = Pego1.PeString.SubsetLabels[ds.Data1];
                string month = Pego1.PeString.PointLabels[ds.Data2];
                string axis  = ds.Data1 == 3 ? "RY" : "LY";
                StatusText.Text = $"Data point  ·  {label}  ·  {month}  ·  {axis}: {val:0.0}";
            }
            else if (ds.Type == HotSpotType.Subset)
            {
                // Over a series legend entry
                StatusText.Text = $"Series legend  ·  {Pego1.PeString.SubsetLabels[ds.Data1]}";
            }
            else if (ds.Type == HotSpotType.Point)
            {
                // Over an X-axis point label
                StatusText.Text = $"Point label  ·  {Pego1.PeString.PointLabels[ds.Data1]}";
            }
            else
            {
                // Not over a named element — find the nearest data point
                // LastMouseMove returns System.Windows.Point in WPF; cast to int for the API call
                System.Windows.Point pt = Pego1.PeUserInterface.Cursor.LastMouseMove;
                // SearchSubsetPointIndex returns Structs.Point; X >= 0 means a match was found
                Gigasoft.ProEssentials.Structs.Point nResult =
                    Pego1.PeFunction.SearchSubsetPointIndex((int)pt.X, (int)pt.Y);

                if (nResult.X >= 0)
                {
                    int s     = Pego1.PeFunction.ClosestSubsetIndex;
                    int p     = Pego1.PeFunction.ClosestPointIndex;
                    float val = Pego1.PeData.Y[s, p];
                    StatusText.Text = $"Nearest  ·  {Pego1.PeString.SubsetLabels[s]}"
                                    + $"  ·  {Pego1.PeString.PointLabels[p]}  ·  {val:0.0}";
                }
                else
                {
                    StatusText.Text = "Move mouse over the chart — data points, series legends, and axis labels are all hot-spot enabled";
                }
            }
        }

        // -----------------------------------------------------------------------
        // Pego1_PeCustomTrackingDataText — ConvPixelToGraph tooltip
        //                                  (Example 007 technique)
        //
        // Fires during tooltip construction whenever the mouse is over the chart.
        // Converts the current mouse pixel position to graph coordinates using
        // ConvPixelToGraph — called twice to read both Y-axis scales:
        //   rightAxis = false → left Y  (units sold, subsets 0–2)
        //   rightAxis = true  → right Y (market share %, subset 3)
        //
        // Setting e.TrackingText replaces the default tooltip string.
        //
        // When TrackingPromptTrigger is CursorMove (keyboard arrow key), the
        // cursor is snapped to an exact data point so we read Y data directly.
        //
        // ConvPixelToGraph signature:
        //   ConvPixelToGraph(ref axisIndex, ref pixelX, ref pixelY,
        //                    ref graphX, ref graphY,
        //                    rightAxis, topAxis, viceVersa)
        //   Initialise axisIndex/pixelX/pixelY before calling.
        //   graphX/graphY receive the converted data-unit coordinates.
        // -----------------------------------------------------------------------
        private void Pego1_PeCustomTrackingDataText(object sender,
            Gigasoft.ProEssentials.EventArg.CustomTrackingDataTextEventArgs e)
        {
            if (Pego1.PeUserInterface.Cursor.TrackingPromptTrigger == TrackingTrigger.MouseMove)
            {
                // Mouse hover — convert pixel to graph coordinates
                // In WPF: LastMouseMove returns System.Windows.Point,
                //          GetRectGraph()  returns System.Windows.Rect
                System.Windows.Point pt = Pego1.PeUserInterface.Cursor.LastMouseMove;
                System.Windows.Rect  r  = Pego1.PeFunction.GetRectGraph();

                if (!r.Contains(pt))
                    return;

                int    nA  = 0;
                int    nX  = (int)pt.X;
                int    nY  = (int)pt.Y;
                double fX  = 0, fLY = 0, fRY = 0;

                // Left Y axis value at cursor position
                Pego1.PeFunction.ConvPixelToGraph(ref nA, ref nX, ref nY, ref fX, ref fLY,
                                                   false, false, false);

                // Right Y axis value — reset pixel coords then call again with rightAxis=true
                nX = (int)pt.X;
                nY = (int)pt.Y;
                nA = 0;
                Pego1.PeFunction.ConvPixelToGraph(ref nA, ref nX, ref nY, ref fX, ref fRY,
                                                   true, false, false);

                e.TrackingText = $"Left Y   \u2190  {fLY:0.0}  (units)\n"
                               + $"{fRY:0.0}  (%)  \u2192  Right Y";
            }
            else
            {
                // Keyboard cursor move — snapped to exact data point
                int s   = Pego1.PeUserInterface.Cursor.Subset;
                int p   = Pego1.PeUserInterface.Cursor.Point;
                float v = Pego1.PeData.Y[s, p];
                string axis = s == 3 ? "Right Y" : "Left Y";
                e.TrackingText = $"{Pego1.PeString.SubsetLabels[s]}\n"
                               + $"{Pego1.PeString.PointLabels[p]}  ·  {axis}: {v:0.0}";
            }
        }

        // -----------------------------------------------------------------------
        // Window_Closing
        // -----------------------------------------------------------------------
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }
    }
}
