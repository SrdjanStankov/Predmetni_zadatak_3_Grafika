using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
        private const double SIZE = 10;
        private Point start = new Point();
        private Point diffOffset = new Point();
        private Point mousePosition = new Point();
        private int zoomMax = 7;
        private int zoomCurent = 1;
        private bool rotate = false;
        private double xScale;
        private double yScale;
        private List<SubstationEntity> substationEntities = new List<SubstationEntity>();
        private List<NodeEntity> nodeEntities = new List<NodeEntity>();
        private List<SwitchEntity> switchEntities = new List<SwitchEntity>();
        private List<LineEntity> lineEntities = new List<LineEntity>();
        private CancellationTokenSource cts = new CancellationTokenSource();
        private AxisAngleRotation3D axisAngleRotation = new AxisAngleRotation3D();

        public MainWindow()
        {
            InitializeComponent();

            yScale = 775 / (Utils.LAT_MAX - Utils.LAT_MIN);
            xScale = 1175 / (Utils.LON_MAX - Utils.LON_MIN);

            var doc = new XmlDocument();
            doc.Load("Geographic.xml");

            Utils.AddEntities(substationEntities, doc.DocumentElement.SelectNodes("/NetworkModel/Substations/SubstationEntity"));
            Utils.AddEntities(nodeEntities, doc.DocumentElement.SelectNodes("/NetworkModel/Nodes/NodeEntity"));
            Utils.AddEntities(switchEntities, doc.DocumentElement.SelectNodes("/NetworkModel/Switches/SwitchEntity"));
            Utils.AddLineEntities(lineEntities, doc.DocumentElement.SelectNodes("/NetworkModel/Lines/LineEntity"));

            substationEntities.ForEach(item =>
            {
                var newY = Utils.Convert(item.X, Utils.LAT_MIN, yScale);
                var newX = Utils.Convert(item.Y, Utils.LON_MIN, xScale);
                item.Y = newY;
                item.X = newX;
            });
            nodeEntities.ForEach(item =>
            {
                var newY = Utils.Convert(item.X, Utils.LAT_MIN, yScale);
                var newX = Utils.Convert(item.Y, Utils.LON_MIN, xScale);
                item.Y = newY;
                item.X = newX;
            });
            switchEntities.ForEach(item =>
            {
                var newY = Utils.Convert(item.X, Utils.LAT_MIN, yScale);
                var newX = Utils.Convert(item.Y, Utils.LON_MIN, xScale);
                item.Y = newY;
                item.X = newX;
            });
            lineEntities.ForEach(item =>
            {
                for (var j = 0; j < item.Vertices.Count; j++)
                {
                    item.Vertices[j] = new Point3D(Utils.Convert(item.Vertices[j].Y, Utils.LON_MIN, xScale), Utils.Convert(item.Vertices[j].X, Utils.LAT_MIN, yScale), item.Vertices[j].Z);
                }
            });

            substationEntities.ForEach(item => MakeCube(item, Brushes.Red));
            nodeEntities.ForEach(item => MakeCube(item, Brushes.Green));
            switchEntities.ForEach(item => MakeCube(item, Brushes.Blue));

            foreach (var item in lineEntities)
            {
                for (var i = 0; i < item.Vertices.Count - 1; i++)
                {
                    MakeLine(0.5, item.Vertices[i], item.Vertices[i + 1], item);
                }
            }
        }

        private void MakeLine(double size, Point3D start, Point3D end, LineEntity entity)
        {
            var v = end - start;
            var v1 = new Vector3D(v.X, -v.Y, v.Z);
            var v2 = new Vector3D(-v.X, v.Y, v.Z);
            v1.Normalize();
            v2.Normalize();

            v1 *= size;
            v2 *= size;
            var p1 = end;
            var p2 = start;
            var pointB = p1 + v1;
            var pointC = p1 + v2;
            var pointA = p2 + v1;
            var pointD = p2 + v2;

            var mesh = new MeshGeometry3D();
            mesh.Positions.Add(pointB);
            mesh.Positions.Add(pointC);
            mesh.Positions.Add(pointA);
            mesh.Positions.Add(pointD);

            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(2);

            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(3);
            mesh.TriangleIndices.Add(2);

            Brush color;
            if (entity.R < 1)
            {
                color = Brushes.Red;
            }
            else if (entity.R < 2)
            {
                color = Brushes.Orange;
            }
            else
            {
                color = Brushes.Yellow;
            }

            var material = new DiffuseMaterial(color);
            var model = new GeometryModel3D(mesh, material);
            model.SetValue(TagProperty, entity);
            modelGroup.Children.Add(model);
        }

        private void MakeCube(PowerEntity entity, Brush brush)
        {
            const double HALF_SIZE = SIZE / 2;
            const double HEIGHT = 10;

            var mesh = new MeshGeometry3D();

            mesh.Positions.Add(new Point3D(entity.X - HALF_SIZE, entity.Y - HALF_SIZE, HEIGHT - HALF_SIZE)); // dole levo
            mesh.Positions.Add(new Point3D(entity.X + HALF_SIZE, entity.Y - HALF_SIZE, HEIGHT - HALF_SIZE)); // dole desno
            mesh.Positions.Add(new Point3D(entity.X - HALF_SIZE, entity.Y + HALF_SIZE, HEIGHT - HALF_SIZE)); // gore levo
            mesh.Positions.Add(new Point3D(entity.X + HALF_SIZE, entity.Y + HALF_SIZE, HEIGHT - HALF_SIZE)); // gore desno
            mesh.Positions.Add(new Point3D(entity.X - HALF_SIZE, entity.Y - HALF_SIZE, HEIGHT + HALF_SIZE)); // dole levo dalje
            mesh.Positions.Add(new Point3D(entity.X + HALF_SIZE, entity.Y - HALF_SIZE, HEIGHT + HALF_SIZE)); // dole desno dalje
            mesh.Positions.Add(new Point3D(entity.X - HALF_SIZE, entity.Y + HALF_SIZE, HEIGHT + HALF_SIZE)); // gore levo dalje
            mesh.Positions.Add(new Point3D(entity.X + HALF_SIZE, entity.Y + HALF_SIZE, HEIGHT + HALF_SIZE)); // gore desno dalje

            foreach (var item in modelGroup.Children)
            {
                while (mesh.Bounds.IntersectsWith(item.Bounds))
                {
                    for (int i = 0; i < mesh.Positions.Count; i++)
                    {
                        mesh.Positions[i] = new Point3D(mesh.Positions[i].X, mesh.Positions[i].Y, mesh.Positions[i].Z + HEIGHT);
                    }
                }
            }

            #region Triangles

            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(3);
            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(3);
            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(5);
            mesh.TriangleIndices.Add(7);
            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(7);
            mesh.TriangleIndices.Add(3);
            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(3);
            mesh.TriangleIndices.Add(6);
            mesh.TriangleIndices.Add(3);
            mesh.TriangleIndices.Add(7);
            mesh.TriangleIndices.Add(6);
            mesh.TriangleIndices.Add(5);
            mesh.TriangleIndices.Add(6);
            mesh.TriangleIndices.Add(7);
            mesh.TriangleIndices.Add(5);
            mesh.TriangleIndices.Add(4);
            mesh.TriangleIndices.Add(6);
            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(6);
            mesh.TriangleIndices.Add(4);
            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(6);
            mesh.TriangleIndices.Add(4);
            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(4);
            mesh.TriangleIndices.Add(5);
            mesh.TriangleIndices.Add(1);
            #endregion


            var material = new DiffuseMaterial(brush);

            var model = new GeometryModel3D(mesh, material);
            model.SetValue(TagProperty, entity);
            modelGroup.Children.Add(model);
        }

        private void Viewport3D_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            viewPort.CaptureMouse();
            start = e.GetPosition(this);
            diffOffset.X = translacija.OffsetX;
            diffOffset.Y = translacija.OffsetY;
        }

        private void viewPort_MouseMove(object sender, MouseEventArgs e)
        {
            if (viewPort.IsMouseCaptured)
            {
                var end = e.GetPosition(this);
                var offsetX = start.X - end.X;
                var offsetY = start.Y - end.Y;
                var translateX = (offsetX * 100) / Width;
                var translateY = -(offsetY * 100) / Height;
                translacija.OffsetX = diffOffset.X + (translateX / (100 * skaliranje.ScaleX)) * 1000;
                translacija.OffsetY = diffOffset.Y + (translateY / (100 * skaliranje.ScaleY)) * 1000;
            }
            if (rotate)
            {
                var end = e.GetPosition(this);
                var axis2D = end - start;
                var axis3D = new Vector3D(-axis2D.Y, -axis2D.X, 0);
                axisAngleRotation.Axis = axis3D;
                axis2D.Normalize();
                axisAngleRotation.Angle += (axis2D.X < 0 || axis2D.Y < 0) ? -0.05 : 0.05;
            }
        }

        private void viewPort_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var p = PointFromScreen(e.GetPosition(win));
            skaliranje.CenterX = p.X;
            skaliranje.CenterY = p.Y;
            skaliranje.CenterZ = 0;
            if (e.Delta > 0 && zoomCurent < zoomMax)
            {
                zoomCurent++;
                skaliranje.ScaleX += 0.1;
                skaliranje.ScaleY += 0.1;
                skaliranje.ScaleZ += 0.1;
            }
            else if (e.Delta <= 0 && zoomCurent > -zoomMax)
            {
                zoomCurent--;
                skaliranje.ScaleX -= 0.1;
                skaliranje.ScaleY -= 0.1;
                skaliranje.ScaleZ -= 0.1;
            }
        }

        private void viewPort_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle && e.ButtonState == MouseButtonState.Pressed)
            {
                rotate = true;
                start = e.GetPosition(this);
                rotiranje.Rotation = axisAngleRotation;
            }

            if (e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Pressed)
            {
                var mouseposition = e.GetPosition(viewPort);
                var testpoint3D = new Point3D(mouseposition.X, mouseposition.Y, 0);
                var testdirection = new Vector3D(mouseposition.X, mouseposition.Y, 10);

                var pointparams = new PointHitTestParameters(mouseposition);
                var rayparams = new RayHitTestParameters(testpoint3D, testdirection);

                mousePosition = e.GetPosition(win);
                VisualTreeHelper.HitTest(viewPort, null, HitResult, pointparams);
            }
        }

        private HitTestResultBehavior HitResult(HitTestResult result)
        {
            var rayResult = result as RayHitTestResult;
            var tag = rayResult?.ModelHit.GetValue(TagProperty);
            if (tag is object)
            {
                CreateLabel(mousePosition, tag);
            }

            return HitTestResultBehavior.Stop;
        }

        private void CreateLabel(Point mousePosition, object entity)
        {
            var label = new Label
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };
            if (entity is BaseEntity)
            {
                label.Content = $"Id: {(entity as BaseEntity).Id}, Name: {(entity as BaseEntity).Name}, Type: {(entity as BaseEntity).GetType().Name}";
            }
            label.Background = Brushes.White;
            label.Margin = new Thickness(mousePosition.X, mousePosition.Y, 0, 0);

            label.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            label.Arrange(new Rect(label.DesiredSize));

            if (mousePosition.X + label.ActualWidth > win.Width)
            {
                label.Margin = new Thickness(mousePosition.X - label.ActualWidth, mousePosition.Y, 0, 0);
            }
            if (mousePosition.Y + label.ActualHeight > win.Height)
            {
                label.Margin = new Thickness(mousePosition.X, mousePosition.Y - label.ActualHeight, 0, 0);
            }

            groupBox.Content = label;
            cts.Cancel();
            cts = new CancellationTokenSource();
            Task.Run(() => RemoveLabelAsync(2), cts.Token);
        }

        private async Task RemoveLabelAsync(int seconds)
        {
            await Task.Delay(seconds * 1000, cts.Token);
            Dispatcher.Invoke(() => { groupBox.Content = null; });
        }

        private void viewPort_MouseUp(object sender, MouseButtonEventArgs e)
        {
            rotate = false;
            viewPort.ReleaseMouseCapture();
        }
    }
}
