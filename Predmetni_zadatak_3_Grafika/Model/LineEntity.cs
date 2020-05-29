using System.Collections.Generic;
using System.Windows.Media.Media3D;

namespace Predmetni_zadatak_3_Grafika.Model
{
    public class LineEntity : BaseEntity
    {
        public LineEntity()
        {

        }

        public bool IsUnderground { get; set; }

        public float R { get; set; }

        public string ConductorMaterial { get; set; }

        public string LineType { get; set; }

        public long ThermalConstantHeat { get; set; }

        public long FirstEnd { get; set; }

        public long SecondEnd { get; set; }

        public List<Point3D> Vertices { get; set; }
    }
}
