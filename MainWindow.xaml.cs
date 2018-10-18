﻿using IGUWPF.src.controllers;
using IGUWPF.src.models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using IGUWPF.src.view.Windows;
using IGUWPF.src.controllers.ControllersImpl;
using IGUWPF.src.models.ViewModel;
using System.Collections.Generic;

namespace IGUWPF
{

    public partial class MainWindow : Window
    {
        private double PlotWidth { get => PlotPanel.ActualWidth; }
        private double PlotHeight { get => PlotPanel.ActualHeight; }

        private Label XYMouseCoordinates;

        private IDAO<Function> FunctionDAO;
        private IViewModelImpl<Function> ViewModel;
        private PlotRepresentationSettings PlotSettings;

        private FunctionListWindow FunctionListWindow;

        public MainWindow()
        {
            InitializeComponent();
            //Instance components
            ViewModel = new IViewModelImpl<Function>();
            FunctionDAO = new SerialDAOImpl<Function>();


            //Create the label to know the Cursor position
            XYMouseCoordinates = new Label()
            {
                BorderThickness = new Thickness(1),
                BorderBrush = Brushes.DodgerBlue,
                Foreground = Brushes.DodgerBlue,
                Background = Brushes.AliceBlue,
                Visibility = Visibility.Hidden
            };

            //Give defect values
            PlotSettings.XMin = PlotSettings.YMin = -10;
            PlotSettings.XMax = PlotSettings.YMax = 10;

            //Add event handlers
            //Reload panel if size changes
            PlotPanel.SizeChanged += ViewModelClearEvent; //The clear event is reused
            //Mouse position events
            PlotPanel.MouseEnter += SetMousePositionLabelVissible;
            PlotPanel.MouseLeave += SetMousePositionLabelHidden;
            PlotPanel.MouseMove += CalculateMousePosition;
            //Plot crud events
            ViewModel.CreateElementEvent += ViewModelCreateElementEvent;
            ViewModel.DeleteElementEvent += ViewModelDeleteElementEvent;
            ViewModel.UpdateElementEvent += ViewModelUpdateElementEvent;
            ViewModel.ClearEvent += ViewModelClearEvent;
            //Closing event
            this.Closed += WhenClosed;

            //FunctionListWindow processing
            FunctionListWindow = new FunctionListWindow(ViewModel);
            FunctionListWindow.Closed += WhenClosed;
            FunctionListWindow.Show();
        }


        private void ViewModelCreateElementEvent(object sender, ViewModelEventArgs e) {
            Polyline Polyline = null;
            PointCollection [] Segments = null;
            Function Function = (Function)e.Element;

            Segments = PlotServices.CalculatePlot(Function.Calculator, this.PlotWidth, this.PlotHeight, PlotSettings);

            for (int i = 0; i < Segments.Length; i++)
            {
                Polyline = new Polyline();

                Polyline.Points = Segments[i];
                Polyline.Name = PlotServices.GetPlotName(Function.ID) + i;
                Polyline.Stroke = new SolidColorBrush(Function.Color);
                PlotPanel.Children.Add(Polyline);
            }
        }

        private void ViewModelDeleteElementEvent(object sender, ViewModelEventArgs e)
        {
            string PlotName = null;
            Polyline Polyline = null;
            Function Function = (Function)e.Element;
            List<Polyline> PolylineList = new List<Polyline>();

            PlotName = PlotServices.GetPlotName(Function.ID);

            foreach (UIElement Element in PlotPanel.Children) {
                if (Element is Polyline)
                {
                    Polyline = (Polyline)Element;
                    if (Polyline.Name.Contains(PlotName))
                        PolylineList.Add((Polyline)Element);
                }
            }

            foreach (Polyline Element in PolylineList)
                PlotPanel.Children.Remove(Element);
        }

        private void ViewModelUpdateElementEvent(object sender, ViewModelEventArgs e)
        {
            string PlotName = null;
            Polyline Polyline = null;
            PointCollection[] Segments = null;
            Function Function = (Function)e.Element;
            List<Polyline> PolylineList = new List<Polyline>();

            //Delete older plot
            PlotName = PlotServices.GetPlotName(Function.ID);

            foreach (UIElement Element in PlotPanel.Children)
            {
                if (Element is Polyline)
                {
                    Polyline = (Polyline)Element;
                    if (Polyline.Name.Contains(PlotName))
                        PolylineList.Add((Polyline)Element);
                }
            }

            foreach (Polyline Element in PolylineList)
                PlotPanel.Children.Remove(Element);

            //Get new plot
            Segments = PlotServices.CalculatePlot(Function.Calculator, this.PlotWidth, this.PlotHeight, PlotSettings);

            for (int i = 0; i < Segments.Length; i++)
            {
                Polyline = new Polyline();

                Polyline.Points = Segments[i];
                Polyline.Name = PlotServices.GetPlotName(Function.ID) + i;
                Polyline.Stroke = new SolidColorBrush(Function.Color);
                PlotPanel.Children.Add(Polyline);
            }
        }

        //A eventArgs is used instead a ViewModelEventArgs because is used this event handler for two events
        private void ViewModelClearEvent(object sender, EventArgs e)
        {
            //Clear the panel
            PlotPanel.Children.Clear();

            //Add the Label to know the plot position
            PlotPanel.Children.Add(XYMouseCoordinates);
            Canvas.SetRight(XYMouseCoordinates, 0);
            Canvas.SetBottom(XYMouseCoordinates, 0);

            //Add axys
            Line[] Axys = PlotServices.GetAxys(this.PlotWidth, this.PlotHeight, PlotSettings);
            PlotPanel.Children.Add(Axys[0]);
            PlotPanel.Children.Add(Axys[1]);
        }

        private void SetMousePositionLabelVissible(object sender, MouseEventArgs e)
        {
            XYMouseCoordinates.Visibility = Visibility.Visible;
        }
        private void SetMousePositionLabelHidden(object sender, MouseEventArgs e)
        {
            XYMouseCoordinates.Visibility = Visibility.Hidden;
        }
        private void CalculateMousePosition(object sender, MouseEventArgs e)
        {
            double realX, realY;

            //Obtain the mouse pointer coordinates
            Panel MousePanel = (Panel)sender;
            Point p = e.GetPosition(MousePanel);

            //Calculate real points
            realX = Math.Truncate(PlotServices.ParseXScreenPointToRealPoint(p.X, MousePanel.ActualWidth, PlotSettings));
            realY = Math.Truncate(PlotServices.ParseYScreenPointToRealPoint(p.Y, MousePanel.ActualHeight, PlotSettings));

            //Update label
            if (null != XYMouseCoordinates)
                XYMouseCoordinates.Content = "X: " + realX + " Y: " + realY;
        }

        private void WhenClosed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
