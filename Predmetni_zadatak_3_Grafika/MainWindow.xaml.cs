using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Xml;
using Predmetni_zadatak_3_Grafika.Model;
using Predmetni_zadatak_3_Grafika.Services;
using Point = System.Windows.Point;

namespace Predmetni_zadatak_3_Grafika
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const double SIZE = 10;
        private Point start = new Point();
        private Point diffOffset = new Point();
        private int zoomMax = 7;
        private int zoomCurent = 1;
        private bool rotate = false;
        private double xScale;
        private double yScale;
        private GeometryModel3D hitgeo;
        private List<SubstationEntity> substationEntities = new List<SubstationEntity>();
        private List<NodeEntity> nodeEntities = new List<NodeEntity>();
        private List<SwitchEntity> switchEntities = new List<SwitchEntity>();
        private List<LineEntity> lineEntities = new List<LineEntity>();

        public MainWindow()
        {
            InitializeComponent();

            xScale = 1175 / (Utils.LAT_MAX - Utils.LAT_MIN);
            yScale = 775 / (Utils.LON_MAX - Utils.LON_MIN);

            var doc = new XmlDocument();
            doc.Load("Geographic.xml");

            Utils.AddEntities(substationEntities, doc.DocumentElement.SelectNodes("/NetworkModel/Substations/SubstationEntity"));
            Utils.AddEntities(nodeEntities, doc.DocumentElement.SelectNodes("/NetworkModel/Nodes/NodeEntity"));
            Utils.AddEntities(switchEntities, doc.DocumentElement.SelectNodes("/NetworkModel/Switches/SwitchEntity"));
            Utils.AddLineEntities(lineEntities, doc.DocumentElement.SelectNodes("/NetworkModel/Lines/LineEntity"));

            substationEntities.ForEach(item => { item.X = Utils.Convert(item.X, xScale, Utils.LAT_MIN, SIZE, 1175); item.Y = Utils.Convert(item.Y, yScale, Utils.LON_MIN, SIZE, 775); });
            nodeEntities.ForEach(item => { item.X = Utils.Convert(item.X, xScale, Utils.LAT_MIN, SIZE, 1175); item.Y = Utils.Convert(item.Y, yScale, Utils.LON_MIN, SIZE, 775); });
            switchEntities.ForEach(item => { item.X = Utils.Convert(item.X, xScale, Utils.LAT_MIN, SIZE, 1175); item.Y = Utils.Convert(item.Y, yScale, Utils.LON_MIN, SIZE, 775); });
            lineEntities.ForEach(item => item.Vertices.ForEach(vert => { vert.X = Utils.Convert(vert.X, xScale, Utils.LAT_MIN, SIZE, 1175); vert.Y = Utils.Convert(vert.Y, yScale, Utils.LON_MIN, SIZE, 775); }));

            substationEntities.ForEach(item => MakeCube(item, Brushes.Red));
            nodeEntities.ForEach(item => MakeCube(item, Brushes.Green));
            switchEntities.ForEach(item => MakeCube(item, Brushes.Blue));
        }

        private void MakeCube(PowerEntity entity, Brush brush)
        {
            const double HALF_SIZE = SIZE / 2;

            var mesh = new MeshGeometry3D();

            mesh.Positions.Add(new Point3D(entity.X - HALF_SIZE, entity.Y - HALF_SIZE, 0 - HALF_SIZE)); // dole levo
            mesh.Positions.Add(new Point3D(entity.X + HALF_SIZE, entity.Y - HALF_SIZE, 0 - HALF_SIZE)); // dole desno
            mesh.Positions.Add(new Point3D(entity.X - HALF_SIZE, entity.Y + HALF_SIZE, 0 - HALF_SIZE)); // gore levo
            mesh.Positions.Add(new Point3D(entity.X + HALF_SIZE, entity.Y + HALF_SIZE, 0 - HALF_SIZE)); // gore desno
            mesh.Positions.Add(new Point3D(entity.X - HALF_SIZE, entity.Y - HALF_SIZE, 0 + HALF_SIZE)); // dole levo dalje
            mesh.Positions.Add(new Point3D(entity.X + HALF_SIZE, entity.Y - HALF_SIZE, 0 + HALF_SIZE)); // dole desno dalje
            mesh.Positions.Add(new Point3D(entity.X - HALF_SIZE, entity.Y + HALF_SIZE, 0 + HALF_SIZE)); // gore levo dalje
            mesh.Positions.Add(new Point3D(entity.X + HALF_SIZE, entity.Y + HALF_SIZE, 0 + HALF_SIZE)); // gore desno dalje
            #region Triangles

            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(3);
            mesh.TriangleIndices.Add(1);

            mesh.TriangleIndices.Add(3);
            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(0);

            mesh.TriangleIndices.Add(7);
            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(3);

            mesh.TriangleIndices.Add(7);
            mesh.TriangleIndices.Add(5);
            mesh.TriangleIndices.Add(1);

            mesh.TriangleIndices.Add(6);
            mesh.TriangleIndices.Add(5);
            mesh.TriangleIndices.Add(7);

            mesh.TriangleIndices.Add(6);
            mesh.TriangleIndices.Add(4);
            mesh.TriangleIndices.Add(5);

            mesh.TriangleIndices.Add(6);
            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(0);

            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(4);

            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(7);
            mesh.TriangleIndices.Add(3);

            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(6);
            mesh.TriangleIndices.Add(7);

            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(5);

            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(5);
            mesh.TriangleIndices.Add(4);
            #endregion


            var material = new DiffuseMaterial(brush);

            modelGroup.Children.Add(new GeometryModel3D(mesh, material));
        }

        private void Viewport3D_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            viewPort.CaptureMouse();
            start = e.GetPosition(this);
            diffOffset.X = translacija.OffsetX;
            diffOffset.Y = translacija.OffsetY;
        }

        private void viewPort_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e) => viewPort.ReleaseMouseCapture();

        private void viewPort_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (viewPort.IsMouseCaptured)
            {
                var end = e.GetPosition(this);
                var offsetX = end.X - start.X;
                var offsetY = end.Y - start.Y;
                var w = Width;
                var h = Height;
                var translateX = (offsetX * 100) / w;
                var translateY = -(offsetY * 100) / h;
                translacija.OffsetX = diffOffset.X + (translateX / (100 * skaliranje.ScaleX));
                translacija.OffsetY = diffOffset.Y + (translateY / (100 * skaliranje.ScaleX));
            }
            if (rotate)
            {

            }
        }

        private void viewPort_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            var p = e.MouseDevice.GetPosition(this);
            double scaleY;
            double scaleX;
            if (e.Delta > 0 && zoomCurent < zoomMax)
            {
                scaleX = skaliranje.ScaleX + 0.1;
                scaleY = skaliranje.ScaleY + 0.1;
                zoomCurent++;
                skaliranje.ScaleX = scaleX;
                skaliranje.ScaleY = scaleY;
            }
            else if (e.Delta <= 0 && zoomCurent > -zoomMax)
            {
                scaleX = skaliranje.ScaleX - 0.1;
                scaleY = skaliranje.ScaleY - 0.1;
                zoomCurent--;
                skaliranje.ScaleX = scaleX;
                skaliranje.ScaleY = scaleY;
            }
        }

        private void viewPort_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Middle && e.ButtonState == System.Windows.Input.MouseButtonState.Pressed)
            {
                rotate = true;
            }

            if (e.ChangedButton == System.Windows.Input.MouseButton.Left && e.ButtonState == System.Windows.Input.MouseButtonState.Pressed)
            {
                var mouseposition = e.GetPosition(viewPort);
                Point3D testpoint3D = new Point3D(mouseposition.X, mouseposition.Y, 0);
                Vector3D testdirection = new Vector3D(mouseposition.X, mouseposition.Y, 10);

                PointHitTestParameters pointparams = new PointHitTestParameters(mouseposition);
                RayHitTestParameters rayparams = new RayHitTestParameters(testpoint3D, testdirection);

                //test for a result in the Viewport3D     
                hitgeo = null;
                VisualTreeHelper.HitTest(viewPort, null, HitResult, pointparams);
            }
        }

        private HitTestResultBehavior HitResult(HitTestResult result)
        {
            var rayResult = result as RayHitTestResult;
            if (rayResult is null)
            {
                return HitTestResultBehavior.Stop;
            }

            return HitTestResultBehavior.Stop;
        }

        private void viewPort_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Middle && e.ButtonState == System.Windows.Input.MouseButtonState.Released)
            {
                rotate = false;
            }
        }
    }
}
