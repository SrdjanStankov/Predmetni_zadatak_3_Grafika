using System.Collections.Generic;

namespace Predmetni_zadatak_3_Grafika.Model
{
    public class LineEntity
    {
        public LineEntity()
        {

        }

        public long Id { get; set; }

        public string Name { get; set; }

        public bool IsUnderground { get; set; }

        public float R { get; set; }

        public string ConductorMaterial { get; set; }

        public string LineType { get; set; }

        public long ThermalConstantHeat { get; set; }

        public long FirstEnd { get; set; }

        public long SecondEnd { get; set; }

        public List<Point> Vertices { get; set; }
    }
}
