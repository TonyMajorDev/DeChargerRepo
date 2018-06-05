/*   
    Copyright (C) 2008 Webyog Softworks Private Limited

    This file is a part of Visifire Charts.
 
    Visifire is a free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
      
    You should have received a copy of the GNU General Public License
    along with Visifire Charts.  If not, see <http://www.gnu.org/licenses/>.
  
    If GPL is not suitable for your products or company, Webyog provides Visifire 
    under a flexible commercial license designed to meet your specific usage and 
    distribution requirements. If you have already obtained a commercial license 
    from Webyog, you can use this file under those license terms.
    
*/

#if WPF
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
#else
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Collections.Generic;
#endif
using System.Linq;
using Visifire.Commons;
using System.Windows.Shapes;

namespace Visifire.Charts
{
    /// <summary>
    /// Visifire.Charts.StickChart class
    /// </summary>
    internal class StickChart
    {
        #region Public Methods

        #endregion

        #region Public Properties

        #endregion

        #region Public Events And Delegates

        #endregion

        #region Protected Methods

        #endregion

        #region Internal Properties

        #endregion

        #region Private Properties

        #endregion

        #region Private Delegates

        #endregion

        #region Private Methods

        #endregion

        #region Internal Methods

        /// <summary>
        /// Recalculate and apply new brush
        /// </summary>
        /// <param name="shape">Shape reference</param>
        /// <param name="newBrush">New Brush</param>
        /// <param name="isLightingEnabled">Whether lighting is enabled</param>
        /// <returns>New Calculated Brush</returns>
        internal static Brush ReCalculateAndApplyTheNewBrush(Shape shape, Brush newBrush, Boolean isLightingEnabled)
        {
            shape.Stroke = ((Boolean)isLightingEnabled) ? Graphics.GetLightingEnabledBrush(newBrush, "Linear", null) : newBrush;

            return shape.Stroke;
        }

        private static void UpdateYValueAndXValuePosition(DataPoint dataPoint, Double canvasWidth, Double canvasHeight, Double dataPointWidth)
        {
            Canvas dataPointVisual = dataPoint.Faces.Visual as Canvas;
            Faces faces = dataPoint.Faces;
            Line highLowLine = faces.VisualComponents[0] as Line;  // HighLowline
            //Line closeLine = faces.VisualComponents[1] as Line;    // Closeline
            //Line openLine = faces.VisualComponents[2] as Line;     // Openline

            Double highY = 0, lowY = 0; //, openY = 0, closeY = 0;
            PlotGroup plotGroup = dataPoint.Parent.PlotGroup;
            //CandleStick.SetDataPointValues(dataPoint, ref openY, ref lowY, ref highY, ref closeY);

            double InternalAxisMaximum = (Double)plotGroup.AxisY.InternalAxisMaximum;

            if (Double.IsNaN((Double)plotGroup.AxisY.InternalAxisMaximum))
            {
                InternalAxisMaximum = canvasHeight;
            }

            if (dataPoint.Parent != null && dataPoint.Parent is DataSeries && (dataPoint.Parent as DataSeries).MaxForY)
                highY = InternalAxisMaximum;
            else
                highY = dataPoint.InternalYValues == null ? dataPoint.YValue : dataPoint.InternalYValues[0];

            //if (dataPoint.InternalYValues.Count() == 0) highY = dataPoint.YValue;

            // Calculate required pixel positions
            Double xPositionOfDataPoint = Graphics.ValueToPixelPosition(0, canvasWidth, (Double)plotGroup.AxisX.InternalAxisMinimum, (Double)plotGroup.AxisX.InternalAxisMaximum, dataPoint.InternalXValue);

            //openY = Graphics.ValueToPixelPosition(canvasHeight, 0, (Double)plotGroup.AxisY.InternalAxisMinimum, (Double)plotGroup.AxisY.InternalAxisMaximum, openY);
            //closeY = Graphics.ValueToPixelPosition(canvasHeight, 0, (Double)plotGroup.AxisY.InternalAxisMinimum, (Double)plotGroup.AxisY.InternalAxisMaximum, closeY);

            highY = Graphics.ValueToPixelPosition(canvasHeight, 0, (Double)plotGroup.AxisY.InternalAxisMinimum, InternalAxisMaximum, highY);
            lowY = Graphics.ValueToPixelPosition(canvasHeight, 0, (Double)plotGroup.AxisY.InternalAxisMinimum, InternalAxisMaximum, lowY);

            //if (highY == Double.NaN)
            //{
            //    highY = 0.0;
            //}
            //if (lowY == Double.NaN)
            //{
            //    lowY = 0.0;
            //}

            Double dataPointTop = (lowY < highY) ? lowY : highY;
            //openY = openY - dataPointTop;
            //closeY = closeY - dataPointTop;

            dataPointVisual.Width = dataPointWidth;
            dataPointVisual.Height = Math.Abs(lowY - highY);

            // Set DataPoint Visual position
            dataPointVisual.SetValue(Canvas.LeftProperty, xPositionOfDataPoint - dataPointWidth / 2);
            dataPointVisual.SetValue(Canvas.TopProperty, dataPointTop);

            // Set position for high-low line
            highLowLine.X1 = dataPointVisual.Width / 2;
            highLowLine.X2 = dataPointVisual.Width / 2;
            highLowLine.Y1 = 0;
            highLowLine.Y2 = dataPointVisual.Height;

            // Set position for open line
            //openLine.X1 = 0;
            //openLine.X2 = dataPointVisual.Width / 2;
            //openLine.Y1 = openY;
            //openLine.Y2 = openY;

            //// Set position for close line
            //closeLine.X1 = dataPointVisual.Width / 2;
            //closeLine.X2 = dataPointVisual.Width;
            //closeLine.Y1 = closeY;
            //closeLine.Y2 = closeY;

            // Need to apply shadow according to position of DataPoint
            ApplyOrUpdateShadow(dataPoint, dataPointVisual, highLowLine, dataPointWidth);


            // Place label for the DataPoint
            StickChart.CreateAndPositionLabel(dataPoint.Parent.Faces.LabelCanvas, dataPoint);

            //StickChart.CreateAndPositionLabelWithCallouts(dataPoint.Parent.Faces.LabelCanvas, dataPoint);


            if (dataPoint.Parent.ToolTipElement != null)
                dataPoint.Parent.ToolTipElement.Hide();

            (dataPoint.Chart as Chart).ChartArea.DisableIndicators();

            dataPoint._visualPosition = new Point((Double)dataPointVisual.GetValue(Canvas.LeftProperty) + dataPointVisual.Width / 2, (Double)dataPointVisual.GetValue(Canvas.TopProperty));
        }

        private static void ApplyBorderProperties(DataPoint dataPoint, Line highLow, Double dataPointWidth)
        {
            highLow.StrokeThickness = (Double)dataPoint.BorderThickness.Left;

            if (highLow.StrokeThickness > dataPointWidth / 2)
                highLow.StrokeThickness = dataPointWidth / 2;
            else if (highLow.StrokeThickness > dataPointWidth)
                highLow.StrokeThickness = dataPointWidth;

            String borderStyle = dataPoint.BorderStyle.ToString();
            highLow.StrokeDashArray = Graphics.LineStyleToStrokeDashArray(borderStyle);

            //highLow.StrokeDashArray = ExtendedGraphics.GetDashArray(BorderStyles.Dotted);


            //// Set style for open line
            //openLine.StrokeThickness = (Double)dataPoint.BorderThickness.Left;
            //openLine.StrokeDashArray = Graphics.LineStyleToStrokeDashArray(borderStyle);

            //// Set style for close line
            //closeLine.StrokeThickness = (Double)dataPoint.BorderThickness.Left;
            //closeLine.StrokeDashArray = Graphics.LineStyleToStrokeDashArray(borderStyle);
        }

        private static void ApplyOrUpdateColorForAStickDp(DataPoint dataPoint, Line highLow)
        {
            // Set style for lighlow line
            ReCalculateAndApplyTheNewBrush(highLow, dataPoint.Color, (Boolean)dataPoint.LightingEnabled);
            //openLine.Stroke = highLow.Stroke;
            //closeLine.Stroke = highLow.Stroke;
        }

        private static void ApplyOrUpdateShadow(DataPoint dataPoint, Canvas dataPointVisual, Line highLow, Double dataPointWidth)
        {
            #region Apply Shadow

            if (!VisifireControl.IsMediaEffectsEnabled)
            {
                Faces dpFaces = dataPoint.Faces;

                dpFaces.ClearList(ref dpFaces.ShadowElements);

                if (dataPoint.ShadowEnabled == true)
                {
                    // Create High and Low Line
                    Line highLowShadow = new Line()
                    {
                        IsHitTestVisible = false,
                        X1 = dataPointVisual.Width / 2 + CandleStick._shadowDepth,
                        X2 = dataPointVisual.Width / 2 + CandleStick._shadowDepth,
                        Y1 = 0 + CandleStick._shadowDepth,
                        Y2 = dataPointVisual.Height + CandleStick._shadowDepth,
                        Stroke = CandleStick._shadowColor,
                        StrokeThickness = (Double)dataPoint.BorderThickness.Left,
                        StrokeDashArray = Graphics.LineStyleToStrokeDashArray(dataPoint.BorderStyle.ToString())
                    };

                    //// Create Open Line
                    //Line openShadowLine = new Line()
                    //{
                    //    IsHitTestVisible = false,
                    //    X1 = openLine.X1 + CandleStick._shadowDepth,
                    //    X2 = openLine.X2 + CandleStick._shadowDepth,
                    //    Y1 = openLine.Y1 + CandleStick._shadowDepth,
                    //    Y2 = openLine.Y2 + CandleStick._shadowDepth,
                    //    Stroke = CandleStick._shadowColor,
                    //    StrokeThickness = (Double)dataPoint.BorderThickness.Left,
                    //    StrokeDashArray = Graphics.LineStyleToStrokeDashArray(dataPoint.BorderStyle.ToString())
                    //};

                    //// Create Close Line
                    //Line closeShadowLine = new Line()
                    //{
                    //    IsHitTestVisible = false,
                    //    X1 = closeLine.X1 + CandleStick._shadowDepth,
                    //    X2 = closeLine.X2 + CandleStick._shadowDepth,
                    //    Y1 = closeLine.Y1 + CandleStick._shadowDepth,
                    //    Y2 = closeLine.Y2 + CandleStick._shadowDepth,

                    //    Stroke = CandleStick._shadowColor,
                    //    StrokeThickness = (Double)dataPoint.BorderThickness.Left,
                    //    StrokeDashArray = Graphics.LineStyleToStrokeDashArray(dataPoint.BorderStyle.ToString())
                    //};

                    // Add shadow elements to list of shadow elements
                    dpFaces.ShadowElements.Add(highLowShadow);
                    //dpFaces.ShadowElements.Add(openShadowLine);
                    //dpFaces.ShadowElements.Add(closeShadowLine);

                    // Add shadows
                    dataPointVisual.Children.Add(highLowShadow);
                    //dataPointVisual.Children.Add(openShadowLine);
                    //dataPointVisual.Children.Add(closeShadowLine);
                }
            }
            else
            {
#if !WP
                if ((Boolean)dataPoint.ShadowEnabled)
                    dataPointVisual.Effect = ExtendedGraphics.GetShadowEffect(315, 4, 0.95);
                else
                    dataPointVisual.Effect = null;
#endif
            }



            #endregion
        }

        public static long callCount = 0;

        internal static void CreateOrUpdateAStickDataPoint(DataPoint dataPoint, Canvas stickChartCanvas, Canvas labelCanvas, Double canvasWidth, Double canvasHeight, Double dataPointWidth)
        {
            callCount++;

            if (dataPoint.XValue == null || dataPoint.Chart.CancelPending) return;

            Faces dpFaces = dataPoint.Faces;

            // Remove preexisting dataPoint visual and label visual
            if (dpFaces != null && dpFaces.Visual != null && stickChartCanvas == dpFaces.Visual.Parent)
            {
                stickChartCanvas.Children.Remove(dataPoint.Faces.Visual);
            }

            // Remove preexisting label visual
            if (dataPoint.LabelVisual != null && dataPoint.LabelVisual.Parent == labelCanvas)
            {
                labelCanvas.Children.Remove(dataPoint.LabelVisual);
            }

            dataPoint.Faces = null;

            if ((dataPoint.InternalYValue == null && dataPoint.InternalYValues == null) || dataPoint.Enabled == false)
                return;

            // Initialize DataPoint faces
            dataPoint.Faces = new Faces();

            // Creating ElementData for Tag
            ElementData tagElement = new ElementData() { Element = dataPoint };

            // Create DataPoint Visual
            Canvas dataPointVisual = new Canvas();          // Create DataPoint Visual
            Line highLow = new Line() { Tag = tagElement };  // Create High and Low Line
            highLow.StrokeDashArray = ExtendedGraphics.GetDashArray(BorderStyles.Dotted);
            //highLow.StrokeDashArray = ExtendedGraphics.GetDashArray(BorderStyles.Dashed);

            //Line closeLine = new Line(){ Tag = tagElement };    // Create Close Line
            //Line openLine = new Line() { Tag = tagElement };    // Create Close Line

            //if (true) //(pieParams.LabelLineEnabled)
            //{
            //    Path labelLine = new Path() { Tag = tagElement };
            //    Double meanAngle = 0.23;
            //    labelLine.SetValue(Canvas.ZIndexProperty, -100000);
            //    Point piePoint = new Point();
            //    piePoint.X = center.X + pieParams.OuterRadius * Math.Cos(meanAngle);
            //    piePoint.Y = center.Y + pieParams.OuterRadius * Math.Sin(meanAngle);

            //    Point labelPoint = new Point();
            //    labelPoint.X = center.X + pieParams.LabelPoint.X - pieParams.Width / 2;
            //    labelPoint.Y = center.Y + pieParams.LabelPoint.Y - pieParams.Height / 2;

            //    Point midPoint = new Point();
            //    // midPoint.X = (labelPoint.X < center.X) ? labelPoint.X + 10 : labelPoint.X - 10;
            //    if (pieParams.LabelLineTargetToRight)
            //        midPoint.X = labelPoint.X + 10;
            //    else
            //        midPoint.X = labelPoint.X - 10;

            //    midPoint.Y = labelPoint.Y;


            //    List<PathGeometryParams> labelLinePathGeometry = new List<PathGeometryParams>();
            //    labelLinePathGeometry.Add(new LineSegmentParams(pieParams.AnimationEnabled ? piePoint : midPoint));
            //    labelLinePathGeometry.Add(new LineSegmentParams(pieParams.AnimationEnabled ? piePoint : labelPoint));
            //    labelLine.Data = GetPathGeometryFromList(FillRule.Nonzero, piePoint, labelLinePathGeometry, true);
            //    PathFigure figure = (labelLine.Data as PathGeometry).Figures[0];
            //    PathSegmentCollection segments = figure.Segments;
            //    figure.IsClosed = false;
            //    figure.IsFilled = false;

            //    // animate the label lines of the individual pie slices
            //    if (pieParams.AnimationEnabled)
            //    {
            //        pieParams.Storyboard = CreateLabelLineAnimation(currentDataSeries, pieParams.DataPoint, pieParams.Storyboard, segments[0], piePoint, midPoint);
            //        pieParams.Storyboard = CreateLabelLineAnimation(currentDataSeries, pieParams.DataPoint, pieParams.Storyboard, segments[1], piePoint, midPoint, labelPoint);
            //    }

            //    labelLine.Stroke = pieParams.LabelLineColor;
            //    labelLine.StrokeDashArray = pieParams.LabelLineStyle;
            //    labelLine.StrokeThickness = pieParams.LabelLineThickness;

            //    labelLinePath = labelLine;

            //    visual.Children.Add(labelLine);
            //}



            dataPoint.Faces.Visual = dataPointVisual;

            // Add VisualComponents
            dataPoint.Faces.VisualComponents.Add(highLow);
            //dataPoint.Faces.VisualComponents.Add(openLine);
            //dataPoint.Faces.VisualComponents.Add(closeLine);

            // Add Border elements
            dataPoint.Faces.BorderElements.Add(highLow);
            //dataPoint.Faces.BorderElements.Add(openLine);
            //dataPoint.Faces.BorderElements.Add(closeLine);

            dataPoint.Faces.Visual = dataPointVisual;
            stickChartCanvas.Children.Add(dataPointVisual);

            UpdateYValueAndXValuePosition(dataPoint, canvasWidth, canvasHeight, dataPointWidth);
            ApplyBorderProperties(dataPoint, highLow, dataPointWidth);
            ApplyOrUpdateColorForAStickDp(dataPoint, highLow);

            // Add VisualComponents to visual
            dataPointVisual.Children.Add(highLow);
            //dataPointVisual.Children.Add(openLine);
            //dataPointVisual.Children.Add(closeLine);

            // Attach tooltip, events, href etc
            dataPointVisual.Opacity = (Double)dataPoint.Parent.Opacity * (Double)dataPoint.Opacity;

            Chart chart = dataPoint.Chart as Chart;
            dataPoint.SetCursor2DataPointVisualFaces();
            dataPoint.AttachEvent2DataPointVisualFaces(dataPoint);
            dataPoint.AttachEvent2DataPointVisualFaces(dataPoint.Parent);
            dataPoint._parsedToolTipText = dataPoint.TextParser(dataPoint.ToolTipText);
            if (!chart.IndicatorEnabled)
            {
                dataPoint.AttachToolTip(chart, dataPoint, dataPoint.Faces.VisualComponents);

                if (dataPoint.LabelVisual != null)
                    dataPoint.AttachToolTip(chart, dataPoint, dataPoint.LabelVisual);
            }
            dataPoint.AttachHref(chart, dataPoint.Faces.VisualComponents, dataPoint.Href, (HrefTargets)dataPoint.HrefTarget);
        }

        /// <summary>
        /// Place label for DataPoint
        /// </summary>
        /// <param name="visual">Visual</param>
        /// <param name="labelCanvas">Canvas for label</param>
        /// <param name="dataPoint">DataPoint</param>
        internal static void CreateAndPositionLabel(Canvas labelCanvas, DataPoint dataPoint)
        {
            if (dataPoint.LabelVisual != null)
            {
                Panel parent = dataPoint.LabelVisual.Parent as Panel;

                if (parent != null)
                    parent.Children.Remove(dataPoint.LabelVisual);
            }

            if ((Boolean)dataPoint.LabelEnabled && !String.IsNullOrEmpty(dataPoint.LabelText))
            {
                //Canvas dataPointVisual = dataPoint.Faces.Visual as Canvas;

                Title tb = new Title()
                {
                    Text = dataPoint.TextParser(dataPoint.LabelText),
                    FontFamily = dataPoint.LabelFontFamily,
                    FontSize = dataPoint.LabelFontSize.Value,
                    FontWeight = (FontWeight)dataPoint.LabelFontWeight,
                    FontStyle = (FontStyle)dataPoint.LabelFontStyle,
                    Background = dataPoint.LabelBackground,
                    FontColor = Chart.CalculateDataPointLabelFontColor(dataPoint.Chart as Chart, dataPoint, dataPoint.LabelFontColor, LabelStyles.OutSide)
                };

                tb.CreateVisualObject(new ElementData() { Element = dataPoint });
                tb.Visual.Height = tb.Height;
                tb.Visual.Width = tb.Width;
                dataPoint.LabelVisual = tb.Visual;

                // Double labelTop = (Double)dataPointVisual.GetValue(Canvas.TopProperty) - tb.Height;
                // Double labelLeft = (Double)dataPointVisual.GetValue(Canvas.LeftProperty) + (dataPointVisual.Width - tb.Width) / 2;

                // if (labelTop < 0) labelTop = (Double)dataPointVisual.GetValue(Canvas.TopProperty);
                // if (labelLeft < 0) labelLeft = 1;
                // if (labelLeft + tb.ActualWidth > labelCanvas.Width)
                //    labelLeft = labelCanvas.Width - tb.ActualWidth - 2;

                // tb.Visual.SetValue(Canvas.LeftProperty, labelLeft);
                // tb.Visual.SetValue(Canvas.TopProperty, labelTop);

                SetLabelPosition(dataPoint, labelCanvas.Width, labelCanvas.Height);

                labelCanvas.Children.Add(tb.Visual);
            }
        }

        /// <summary>
        /// Place label for DataPoint
        /// </summary>
        /// <param name="visual">Visual</param>
        /// <param name="labelCanvas">Canvas for label</param>
        /// <param name="dataPoint">DataPoint</param>
        internal static void CreateAndPositionLabelWithCallouts(Canvas labelCanvas, DataPoint dataPoint)
        {
            var stickChartCanvas = dataPoint.Parent.Faces.Visual as Canvas;

            if (dataPoint.LabelVisual != null)
            {
                Panel parent = dataPoint.LabelVisual.Parent as Panel;

                if (parent != null)
                    parent.Children.Remove(dataPoint.LabelVisual);
            }

            if ((Boolean)dataPoint.LabelEnabled && !String.IsNullOrEmpty(dataPoint.LabelText))
            {
                //Canvas dataPointVisual = dataPoint.Faces.Visual as Canvas;

                Title tb = new Title()
                {
                    Text = dataPoint.TextParser(dataPoint.LabelText),
                    FontFamily = dataPoint.LabelFontFamily,
                    FontSize = dataPoint.LabelFontSize.Value,
                    FontWeight = (FontWeight)dataPoint.LabelFontWeight,
                    FontStyle = (FontStyle)dataPoint.LabelFontStyle,
                    Background = dataPoint.LabelBackground,
                    FontColor = Chart.CalculateDataPointLabelFontColor(dataPoint.Chart as Chart, dataPoint, dataPoint.LabelFontColor, LabelStyles.OutSide)
                };

                tb.CreateVisualObject(new ElementData() { Element = dataPoint });
                tb.Visual.Height = tb.Height;
                tb.Visual.Width = tb.Width;
                dataPoint.LabelVisual = tb.Visual;

                // Double labelTop = (Double)dataPointVisual.GetValue(Canvas.TopProperty) - tb.Height;
                // Double labelLeft = (Double)dataPointVisual.GetValue(Canvas.LeftProperty) + (dataPointVisual.Width - tb.Width) / 2;

                // if (labelTop < 0) labelTop = (Double)dataPointVisual.GetValue(Canvas.TopProperty);
                // if (labelLeft < 0) labelLeft = 1;
                // if (labelLeft + tb.ActualWidth > labelCanvas.Width)
                //    labelLeft = labelCanvas.Width - tb.ActualWidth - 2;

                // tb.Visual.SetValue(Canvas.LeftProperty, labelLeft);
                // tb.Visual.SetValue(Canvas.TopProperty, labelTop);

                SetLabelPositionWithAvoidance(labelCanvas, stickChartCanvas, dataPoint);

                labelCanvas.Children.Add(tb.Visual);

                var line = CreateLabelLine(dataPoint, labelCanvas.Width, labelCanvas.Height);

                labelCanvas.Children.Add(line);

            }
        }

        private static Path CreateLabelLine(DataPoint dataPoint, Double canvasWidth, Double canvasHeight)
        {
            Canvas dataPointVisual = dataPoint.Faces.Visual as Canvas;

            double labelTop = (Double)dataPointVisual.GetValue(Canvas.TopProperty) - dataPoint.LabelVisual.Height;
            double labelBottom = (Double)dataPointVisual.GetValue(Canvas.TopProperty) - canvasHeight;
            double labelLeft = (Double)dataPointVisual.GetValue(Canvas.LeftProperty) + (dataPointVisual.Width - dataPoint.LabelVisual.Width) / 2;
            double labelMiddle = (Double)dataPointVisual.GetValue(Canvas.LeftProperty) + (dataPointVisual.Width - dataPoint.LabelVisual.Width) / 2;

            double pointTop = (Double)dataPointVisual.GetValue(Canvas.TopProperty);
            double pointLeft = (Double)dataPointVisual.GetValue(Canvas.LeftProperty) + (dataPointVisual.Width / 2);
            double pointRight = (Double)dataPointVisual.GetValue(Canvas.LeftProperty) + dataPointVisual.Width;

            if (labelTop < 0) labelTop = (Double)dataPointVisual.GetValue(Canvas.TopProperty);
            if (labelLeft < 0) labelLeft = 1;
            if (labelLeft + dataPoint.LabelVisual.Width > canvasWidth)
                labelLeft = canvasWidth - dataPoint.LabelVisual.Width - 2;

            //dataPoint.LabelVisual.SetValue(Canvas.LeftProperty, labelLeft);
            //dataPoint.LabelVisual.SetValue(Canvas.TopProperty, labelTop);





            Path labelLine = new Path() { Tag = new ElementData() { Element = dataPoint } };
            Double meanAngle = 0.23;
            labelLine.SetValue(Canvas.ZIndexProperty, -100000);
            Point piePoint = new Point();
            piePoint.X = pointLeft;
            piePoint.Y = pointTop;

            Point labelPoint = new Point();
            labelPoint.X = labelLeft;
            labelPoint.Y = labelTop;

            Point midPoint = new Point();
            // midPoint.X = (labelPoint.X < center.X) ? labelPoint.X + 10 : labelPoint.X - 10;
            //if (pieParams.LabelLineTargetToRight)
            midPoint.X = labelPoint.X + 10;
            //else
            //   midPoint.X = labelPoint.X - 10;

            midPoint.Y = labelPoint.Y;

            var labelLinePathGeometry = new List<Visifire.Charts.PieChart.PathGeometryParams>();
            labelLinePathGeometry.Add(new Visifire.Charts.PieChart.LineSegmentParams(midPoint));
            labelLinePathGeometry.Add(new Visifire.Charts.PieChart.LineSegmentParams(labelPoint));
            labelLine.Data = Visifire.Charts.PieChart.GetPathGeometryFromList(FillRule.Nonzero, piePoint, labelLinePathGeometry, true);
            PathFigure figure = (labelLine.Data as PathGeometry).Figures[0];
            PathSegmentCollection segments = figure.Segments;
            figure.IsClosed = false;
            figure.IsFilled = false;

            //// animate the label lines of the individual pie slices
            //if (pieParams.AnimationEnabled)
            //{
            //    pieParams.Storyboard = CreateLabelLineAnimation(currentDataSeries, pieParams.DataPoint, pieParams.Storyboard, segments[0], piePoint, midPoint);
            //    pieParams.Storyboard = CreateLabelLineAnimation(currentDataSeries, pieParams.DataPoint, pieParams.Storyboard, segments[1], piePoint, midPoint, labelPoint);
            //}

            labelLine.Stroke = new SolidColorBrush() { Color = Colors.Black };
            labelLine.StrokeDashArray = null;
            labelLine.StrokeThickness = 1;

            //labelLinePath = labelLine;

            //if ((pieParams.TagReference as DataPoint).InternalYValue == 0)
            //{
            //    Line zeroLine = new Line();
            //    zeroLine.X1 = center.X;
            //    zeroLine.Y1 = center.Y;

            //    zeroLine.X2 = piePoint.X;
            //    zeroLine.Y2 = piePoint.Y;
            //    zeroLine.Stroke = pieParams.LabelLineColor;
            //    zeroLine.StrokeThickness = 0.25;
            //    zeroLine.IsHitTestVisible = false;
            //    visual.Children.Add(zeroLine);

            //    if (pieParams.AnimationEnabled)
            //    {
            //        pieParams.Storyboard = CreateOpacityAnimation(currentDataSeries, pieParams.DataPoint, pieParams.Storyboard, zeroLine, 2, zeroLine.Opacity, 0.5);
            //        zeroLine.Opacity = 0;
            //    }
            //}

            //visual.Children.Add(labelLine);

            //// set the un exploded points for interactivity
            //unExplodedPoints.LabelLineEndPoint = labelPoint;
            //unExplodedPoints.LabelLineMidPoint = midPoint;
            //unExplodedPoints.LabelLineStartPoint = piePoint;

            //// set the exploded points for interactivity
            //explodedPoints.LabelLineEndPoint = new Point(labelPoint.X, labelPoint.Y - yOffset);
            //explodedPoints.LabelLineMidPoint = new Point(midPoint.X, midPoint.Y - yOffset);
            //explodedPoints.LabelLineStartPoint = new Point(piePoint.X + xOffset, piePoint.Y + yOffset);

            //if ((pieParams.TagReference as DataPoint).InternalYValue == 0)
            //    labelLine.IsHitTestVisible = false;

            return labelLine;
        }

        private static void SetLabelPositionWithAvoidance(Canvas LabelCanvas, Canvas stickChartCanvas, DataPoint dataPoint)
        {
            Canvas dataPointLabelVisual = dataPoint.LabelVisual as Canvas;
            Canvas dataPointVisual = dataPoint.Faces.Visual as Canvas;

            Double labelTop;
            Double labelLeft;

            // Propose label in 5 different positions (centered, right, left, far left, far right) and if none collide, set opacity to 0 to hide the label

            if (Double.IsNaN(dataPoint.LabelAngle) || dataPoint.LabelAngle == 0)
            {
                labelTop = (Double)dataPointVisual.GetValue(Canvas.TopProperty) - dataPoint.LabelVisual.Height;
                labelLeft = (Double)dataPointVisual.GetValue(Canvas.LeftProperty) + (dataPointVisual.Width - dataPoint.LabelVisual.Width) / 2;

                if (labelTop < 0) labelTop = (Double)dataPointVisual.GetValue(Canvas.TopProperty);
                if (labelLeft < 0) labelLeft = 1;
                //if (labelLeft + dataPoint.LabelVisual.Width > canvasWidth)
                //    labelLeft = canvasWidth - dataPoint.LabelVisual.Width - 2;

                dataPoint.LabelVisual.SetValue(Canvas.LeftProperty, labelLeft);
                dataPoint.LabelVisual.SetValue(Canvas.TopProperty, labelTop);
            }
            else
            {
                Point centerOfRotation = new Point((Double)dataPointVisual.GetValue(Canvas.LeftProperty) + dataPointVisual.Width / 2,
                    (Double)dataPointVisual.GetValue(Canvas.TopProperty));

                Double radius = 4;
                Double angle = 0;
                Double angleInRadian = 0;

                if (dataPoint.LabelAngle > 0 && dataPoint.LabelAngle <= 90)
                {
                    angle = dataPoint.LabelAngle - 180;
                    angleInRadian = (Math.PI / 180) * angle;
                    radius += dataPoint.LabelVisual.Width;
                    angle = (angleInRadian - Math.PI) * (180 / Math.PI);
                }
                else if (dataPoint.LabelAngle >= -90 && dataPoint.LabelAngle < 0)
                {
                    angle = dataPoint.LabelAngle;
                    angleInRadian = (Math.PI / 180) * angle;
                }

                labelLeft = centerOfRotation.X + radius * Math.Cos(angleInRadian);
                labelTop = centerOfRotation.Y + radius * Math.Sin(angleInRadian);

                labelTop -= dataPoint.LabelVisual.Height / 2;

                dataPoint.LabelVisual.SetValue(Canvas.LeftProperty, labelLeft);
                dataPoint.LabelVisual.SetValue(Canvas.TopProperty, labelTop);

                dataPoint.LabelVisual.RenderTransformOrigin = new Point(0, 0.5);
                dataPoint.LabelVisual.RenderTransform = new RotateTransform()
                {
                    CenterX = 0,
                    CenterY = 0,
                    Angle = angle
                };
            }
        }

        private static void SetLabelPosition(DataPoint dataPoint, Double canvasWidth, Double canvasHeight)
        {
            Canvas dataPointVisual = dataPoint.Faces.Visual as Canvas;

            Double labelTop;
            Double labelLeft;

            if (Double.IsNaN(dataPoint.LabelAngle) || dataPoint.LabelAngle == 0)
            {
                labelTop = (Double)dataPointVisual.GetValue(Canvas.TopProperty) - dataPoint.LabelVisual.Height;
                labelLeft = (Double)dataPointVisual.GetValue(Canvas.LeftProperty) + (dataPointVisual.Width - dataPoint.LabelVisual.Width) / 2;

                if (labelTop < 0) labelTop = (Double)dataPointVisual.GetValue(Canvas.TopProperty);
                if (labelLeft < 0) labelLeft = 1;
                if (labelLeft + dataPoint.LabelVisual.Width > canvasWidth)
                    labelLeft = canvasWidth - dataPoint.LabelVisual.Width - 2;

                dataPoint.LabelVisual.SetValue(Canvas.LeftProperty, labelLeft);
                dataPoint.LabelVisual.SetValue(Canvas.TopProperty, labelTop);
            }
            else
            {
                Point centerOfRotation = new Point((Double)dataPointVisual.GetValue(Canvas.LeftProperty) + dataPointVisual.Width / 2,
                    (Double)dataPointVisual.GetValue(Canvas.TopProperty));

                Double radius = 4;
                Double angle = 0;
                Double angleInRadian = 0;

                if (dataPoint.LabelAngle > 0 && dataPoint.LabelAngle <= 90)
                {
                    angle = dataPoint.LabelAngle - 180;
                    angleInRadian = (Math.PI / 180) * angle;
                    radius += dataPoint.LabelVisual.Width;
                    angle = (angleInRadian - Math.PI) * (180 / Math.PI);
                }
                else if (dataPoint.LabelAngle >= -90 && dataPoint.LabelAngle < 0)
                {
                    angle = dataPoint.LabelAngle;
                    angleInRadian = (Math.PI / 180) * angle;
                }

                labelLeft = centerOfRotation.X + radius * Math.Cos(angleInRadian);
                labelTop = centerOfRotation.Y + radius * Math.Sin(angleInRadian);

                labelTop -= dataPoint.LabelVisual.Height / 2;

                dataPoint.LabelVisual.SetValue(Canvas.LeftProperty, labelLeft);
                dataPoint.LabelVisual.SetValue(Canvas.TopProperty, labelTop);

                dataPoint.LabelVisual.RenderTransformOrigin = new Point(0, 0.5);
                dataPoint.LabelVisual.RenderTransform = new RotateTransform()
                {
                    CenterX = 0,
                    CenterY = 0,
                    Angle = angle
                };
            }
        }


        /// <summary>
        /// Calculate DataPoint width
        /// </summary>
        /// <param name="width">PlotCanvas width</param>
        /// <param name="height">PlotCanvas height</param>
        /// <param name="chart">Chart reference</param>
        /// <returns>DataPointWidth as Double</returns>
        internal static Double CalculateDataPointWidth(Double width, Double height, Chart chart)
        {
            Double dataPointWidth;

            Double minDiffValue = chart.PlotDetails.GetMinOfMinDifferencesForXValue(RenderAs.Column, RenderAs.StackedColumn, RenderAs.StackedColumn100, RenderAs.Stock, RenderAs.CandleStick, RenderAs.Stick);

            if (double.IsPositiveInfinity(minDiffValue))
                minDiffValue = 0;

            if (Double.IsNaN(chart.DataPointWidth) || chart.DataPointWidth < 0)
            {
                if (minDiffValue != 0)
                {
                    dataPointWidth = Graphics.ValueToPixelPosition(0, width, (Double)chart.AxesX[0].InternalAxisMinimum, (Double)chart.AxesX[0].InternalAxisMaximum, minDiffValue + (Double)chart.AxesX[0].InternalAxisMinimum);
                    dataPointWidth *= .9;

                    if (dataPointWidth < 5)
                        dataPointWidth = 5;
                }
                else
                {
                    dataPointWidth = width * .3;
                }
            }
            else
            {
                dataPointWidth = chart.PlotArea.Width / 100 * chart.DataPointWidth;
            }

            if (dataPointWidth < 2)
                dataPointWidth = 2;

            return dataPointWidth;
        }

        /// <summary>
        /// Get visual object for point chart
        /// </summary>
        /// <param name="width">Width of the charat</param>
        /// <param name="height">Height of the charat</param>
        /// <param name="plotDetails">plotDetails</param>
        /// <param name="seriesList">List of DataSeries</param>
        /// <param name="chart">Chart</param>
        /// <param name="plankDepth">Plank depth</param>
        /// <param name="animationEnabled">Whether animation is enabled</param>
        /// <returns>Point chart canvas</returns>
        internal static Canvas GetVisualObjectForStickChart(Panel preExistingPanel, Double width, Double height, PlotDetails plotDetails, List<DataSeries> seriesList, Chart chart, Double plankDepth, bool animationEnabled)
        {
            if (Double.IsNaN(width) || Double.IsNaN(height) || width <= 0 || height <= 0) return null;

            Canvas visual, labelCanvas, stickChartCanvas;

            RenderHelper.RepareCanvas4Drawing(preExistingPanel as Canvas, out visual, out labelCanvas, out stickChartCanvas, width, height);

            Double depth3d = plankDepth / (plotDetails.Layer3DCount == 0 ? 1 : plotDetails.Layer3DCount) * (chart.View3D ? 1 : 0);
            Double visualOffset = depth3d * (plotDetails.SeriesDrawingIndex[seriesList[0]] + 1 - (plotDetails.Layer3DCount == 0 ? 0 : 1));

            visual.SetValue(Canvas.TopProperty, visualOffset);
            visual.SetValue(Canvas.LeftProperty, -visualOffset);

            Double animationBeginTime = 0;
            DataSeries _tempDataSeries = null;

            // Calculate width of a DataPoint 
            Double dataPointWidth = StickChart.CalculateDataPointWidth(width, height, chart);

            foreach (DataSeries series in seriesList)
            {
                if (series.Enabled == false)
                    continue;

                Faces dsFaces = new Faces() { Visual = stickChartCanvas, LabelCanvas = labelCanvas };
                series.Faces = dsFaces;

                PlotGroup plotGroup = series.PlotGroup;
                _tempDataSeries = series;

                List<DataPoint> viewPortDataPoints = RenderHelper.GetDataPointsUnderViewPort(series, false);

                // sorting by descending Y Value makes for nicer looking callouts...
                foreach (DataPoint dataPoint in viewPortDataPoints.OrderByDescending(p => p.YValue))
                    CreateOrUpdateAStickDataPoint(dataPoint, stickChartCanvas, labelCanvas, width, height, dataPointWidth);
            }

            // Apply animation to series
            if (animationEnabled)
            {
                if (_tempDataSeries.Storyboard == null)
                    _tempDataSeries.Storyboard = new Storyboard();

                _tempDataSeries.Storyboard = AnimationHelper.ApplyOpacityAnimation(stickChartCanvas, _tempDataSeries, _tempDataSeries.Storyboard, animationBeginTime, 1, 0, 1);
                animationBeginTime += 0.5;
            }

            // Label animation
            if (animationEnabled && _tempDataSeries != null)
                _tempDataSeries.Storyboard = AnimationHelper.ApplyOpacityAnimation(labelCanvas, _tempDataSeries, _tempDataSeries.Storyboard, animationBeginTime, 1, 0, 1);

            stickChartCanvas.Tag = null;

            // ColumnChart.CreateOrUpdatePlank(chart, seriesList[0].PlotGroup.AxisY, stickChartCanvas, depth3d, Orientation.Horizontal);

            // Remove old visual and add new visual in to the existing panel
            if (preExistingPanel != null)
            {
                visual.Children.RemoveAt(1);
                visual.Children.Add(stickChartCanvas);
            }
            else
            {
                labelCanvas.SetValue(Canvas.ZIndexProperty, 1);
                visual.Children.Add(labelCanvas);
                visual.Children.Add(stickChartCanvas);
            }

            RectangleGeometry clipRectangle = new RectangleGeometry();
            clipRectangle.Rect = new Rect(0, -chart.ChartArea.PLANK_DEPTH, width + chart.ChartArea.PLANK_OFFSET, height + chart.ChartArea.PLANK_DEPTH);
            visual.Clip = clipRectangle;

            return visual;
        }

        public static void Update(ObservableObject sender, VcProperties property, object newValue, Boolean isAxisChanged)
        {
            Boolean isDataPoint = sender.GetType().Equals(typeof(DataPoint));

            if (isDataPoint)
                UpdateDataPoint(sender as DataPoint, property, newValue, isAxisChanged);
            else
                UpdateDataSeries(sender as DataSeries, property, newValue, isAxisChanged);
        }

        private static void UpdateDataPoint(DataPoint dataPoint, VcProperties property, object newValue, Boolean isAxisChanged)
        {
            Chart chart = dataPoint.Chart as Chart;

            if (chart == null)
                return;

            DataSeries dataSeries = dataPoint.Parent;
            PlotGroup plotGroup = dataSeries.PlotGroup;
            Faces dsFaces = dataSeries.Faces;
            Faces dpFaces = dataPoint.Faces;

            Double dataPointWidth;

            if (dsFaces != null)
                ColumnChart.UpdateParentVisualCanvasSize(chart, dsFaces.Visual as Canvas);

            if (dpFaces != null && dpFaces.Visual != null)
                dataPointWidth = dpFaces.Visual.Width;
            else if (dsFaces == null)
                return;
            else
                dataPointWidth = CandleStick.CalculateDataPointWidth(dsFaces.Visual.Width, dsFaces.Visual.Height, chart);

            if (property == VcProperties.Enabled || (dpFaces == null && (property == VcProperties.XValue || property == VcProperties.YValues)))
            {
                CreateOrUpdateAStickDataPoint(dataPoint, dsFaces.Visual as Canvas, dsFaces.LabelCanvas, dsFaces.Visual.Width, dsFaces.Visual.Height, dataPointWidth);
                return;
            }

            if (dpFaces == null)
                return;

            Canvas dataPointVisual = dpFaces.Visual as Canvas;       // DataPoint visual canvas
            Line highLowLine = dpFaces.VisualComponents[0] as Line;  // HighLowline
            //Line closeLine = dpFaces.VisualComponents[1] as Line;    // Closeline
            //Line openLine = dpFaces.VisualComponents[2] as Line;     // Openline

            switch (property)
            {
                case VcProperties.BorderThickness:
                case VcProperties.BorderStyle:
                    ApplyBorderProperties(dataPoint, highLowLine, dataPointWidth);
                    break;

                case VcProperties.Color:
                    ApplyOrUpdateColorForAStickDp(dataPoint, highLowLine);
                    break;

                case VcProperties.Cursor:
                    dataPoint.SetCursor2DataPointVisualFaces();
                    break;

                case VcProperties.Href:
                    dataPoint.SetHref2DataPointVisualFaces();
                    break;

                case VcProperties.HrefTarget:
                    dataPoint.SetHref2DataPointVisualFaces();
                    break;

                case VcProperties.LabelBackground:
                case VcProperties.LabelEnabled:
                case VcProperties.LabelFontColor:
                case VcProperties.LabelFontFamily:
                case VcProperties.LabelFontStyle:
                case VcProperties.LabelFontSize:
                case VcProperties.LabelFontWeight:
                case VcProperties.LabelStyle:
                case VcProperties.LabelText:
                    StickChart.CreateAndPositionLabel(dsFaces.LabelCanvas, dataPoint);
                    break;

                case VcProperties.LegendText:
                    chart.InvokeRender();
                    break;

                case VcProperties.LightingEnabled:
                    ApplyOrUpdateColorForAStickDp(dataPoint, highLowLine);
                    break;

                //case VcProperties.MarkerBorderColor:
                //case VcProperties.MarkerBorderThickness:
                //case VcProperties.MarkerColor:
                //case VcProperties.MarkerEnabled:
                //case VcProperties.MarkerScale:
                //case VcProperties.MarkerSize:
                //case VcProperties.MarkerType:
                case VcProperties.ShadowEnabled:
                    ApplyOrUpdateShadow(dataPoint, dataPointVisual, highLowLine, dataPointWidth);
                    break;

                case VcProperties.Opacity:
                    dpFaces.Visual.Opacity = (Double)dataSeries.Opacity * (Double)dataPoint.Opacity;
                    break;

                case VcProperties.ShowInLegend:
                    chart.InvokeRender();
                    break;

                case VcProperties.ToolTipText:
                    dataPoint._parsedToolTipText = dataPoint.TextParser(dataPoint.ToolTipText);
                    break;

                case VcProperties.XValueFormatString:
                case VcProperties.YValueFormatString:
                    dataPoint._parsedToolTipText = dataPoint.TextParser(dataPoint.ToolTipText);
                    StickChart.CreateAndPositionLabel(dsFaces.LabelCanvas, dataPoint);
                    break;

                case VcProperties.XValueType:
                    chart.InvokeRender();
                    break;

                case VcProperties.Enabled:
                    CreateOrUpdateAStickDataPoint(dataPoint, dsFaces.Visual as Canvas, dsFaces.LabelCanvas, dsFaces.Visual.Width, dsFaces.Visual.Height, dataPointWidth);
                    break;

                case VcProperties.XValue:
                case VcProperties.YValue:
                case VcProperties.YValues:
                    if (isAxisChanged || dataPoint.InternalYValues == null)
                        UpdateDataSeries(dataSeries, property, newValue, isAxisChanged);
                    else
                    {
                        dataPoint._parsedToolTipText = dataPoint.TextParser(dataPoint.ToolTipText);
                        UpdateYValueAndXValuePosition(dataPoint, dsFaces.Visual.Width, dsFaces.Visual.Height, dpFaces.Visual.Width);

                        if ((Boolean)dataPoint.LabelEnabled)
                            StickChart.CreateAndPositionLabel(dsFaces.LabelCanvas, dataPoint);
                    }

                    if (dataPoint.Parent.SelectionEnabled && dataPoint.Selected)
                        dataPoint.Select(true);

                    break;
            }
        }

        private static void UpdateDataSeries(DataSeries dataSeries, VcProperties property, object newValue, Boolean isAxisChanged)
        {
            Chart chart = dataSeries.Chart as Chart;

            if (chart == null)
                return;

            switch (property)
            {
                case VcProperties.DataPoints:
                case VcProperties.YValues:
                case VcProperties.XValue:
                    chart.ChartArea.RenderSeries();
                    //Canvas ChartVisualCanvas = chart.ChartArea.ChartVisualCanvas;

                    //Double width = chart.ChartArea.ChartVisualCanvas.Width;
                    //Double height = chart.ChartArea.ChartVisualCanvas.Height;

                    //PlotDetails plotDetails = chart.PlotDetails;
                    //PlotGroup plotGroup = dataSeries.PlotGroup;

                    //List<DataSeries> dataSeriesListInDrawingOrder = plotDetails.SeriesDrawingIndex.Keys.ToList();

                    //List<DataSeries> selectedDataSeries4Rendering = new List<DataSeries>();

                    //RenderAs currentRenderAs = dataSeries.RenderAs;

                    //Int32 currentDrawingIndex = plotDetails.SeriesDrawingIndex[dataSeries];

                    //for (Int32 i = 0; i < chart.InternalSeries.Count; i++)
                    //{
                    //    if (currentRenderAs == dataSeriesListInDrawingOrder[i].RenderAs && currentDrawingIndex == plotDetails.SeriesDrawingIndex[dataSeriesListInDrawingOrder[i]])
                    //        selectedDataSeries4Rendering.Add(dataSeriesListInDrawingOrder[i]);
                    //}

                    //if (selectedDataSeries4Rendering.Count == 0)
                    //    return;

                    //Panel oldPanel = null;
                    //Dictionary<RenderAs, Panel> RenderedCanvasList = chart.ChartArea.RenderedCanvasList;

                    //if (chart.ChartArea.RenderedCanvasList.ContainsKey(currentRenderAs))
                    //{
                    //    oldPanel = RenderedCanvasList[currentRenderAs];
                    //}

                    //Panel renderedChart = chart.ChartArea.RenderSeriesFromList(oldPanel, selectedDataSeries4Rendering);

                    //if (oldPanel == null)
                    //{
                    //    chart.ChartArea.RenderedCanvasList.Add(currentRenderAs, renderedChart);
                    //    renderedChart.SetValue(Canvas.ZIndexProperty, currentDrawingIndex);
                    //    ChartVisualCanvas.Children.Add(renderedChart);
                    //}
                    //else
                    //    chart.ChartArea.RenderedCanvasList[currentRenderAs] = renderedChart;

                    //Visifire.Charts.Chart.SelectDataPoints(chart);
                    break;

                default:
                    // case VcProperties.Enabled:
                    foreach (DataPoint dataPoint in dataSeries.InternalDataPoints.OrderByDescending(p => p.YValue))
                        UpdateDataPoint(dataPoint, property, newValue, isAxisChanged);
                    break;
            }
        }

        //internal static void Update(Chart chart, RenderAs currentRenderAs, List<DataSeries> selectedDataSeries4Rendering, VcProperties property, object newValue)
        //{
        //    Boolean is3D = chart.View3D;
        //    ChartArea chartArea = chart.ChartArea;
        //    Canvas ChartVisualCanvas = chart.ChartArea.ChartVisualCanvas;

        //    // Double width = chart.ChartArea.ChartVisualCanvas.Width;
        //    // Double height = chart.ChartArea.ChartVisualCanvas.Height;

        //    Panel preExistingPanel = null;
        //    Dictionary<RenderAs, Panel> RenderedCanvasList = chart.ChartArea.RenderedCanvasList;

        //    if (chartArea.RenderedCanvasList.ContainsKey(currentRenderAs))
        //    {
        //        preExistingPanel = RenderedCanvasList[currentRenderAs];
        //    }

        //    Panel renderedChart = chartArea.RenderSeriesFromList(preExistingPanel, selectedDataSeries4Rendering);

        //    if (preExistingPanel == null)
        //    {
        //        chartArea.RenderedCanvasList.Add(currentRenderAs, renderedChart);
        //        ChartVisualCanvas.Children.Add(renderedChart);
        //    }
        //}

        #endregion

        #region Internal Events And Delegates

        #endregion

        #region Data

        #endregion
    }
}