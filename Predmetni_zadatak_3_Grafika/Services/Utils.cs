﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using Predmetni_zadatak_3_Grafika.Model;

namespace Predmetni_zadatak_3_Grafika.Services
{
    public static class Utils
    {
        private const double LAT_MIN = 45.2325;
        private const double LAT_MAX = 45.277031;
        private const double LON_MIN = 19.793909;
        private const double LON_MAX = 19.894459;

        public static void AddEntities<T>(List<T> entities, XmlNodeList nodeList) where T : PowerEntity, new()
        {
            foreach (XmlNode item in nodeList)
            {
                ToLatLon(double.Parse(item.SelectSingleNode("X").InnerText, CultureInfo.InvariantCulture), double.Parse(item.SelectSingleNode("Y").InnerText, CultureInfo.InvariantCulture), 34, out var x, out var y);

                if (!(LAT_MIN <= x && x <= LAT_MAX) || !(LON_MIN <= y && y <= LON_MAX))
                {
                    continue;
                }

                var entity = new T()
                {
                    Id = long.Parse(item.SelectSingleNode("Id").InnerText, CultureInfo.InvariantCulture),
                    Name = item.SelectSingleNode("Name").InnerText,
                    X = x,
                    Y = y
                };
                if (typeof(T) == typeof(SwitchEntity))
                {
                    (entity as SwitchEntity).Status = item.SelectSingleNode("Status").InnerText;
                }
                entities.Add(entity);
            }
        }

        public static void AddLineEntities(List<LineEntity> entites, XmlNodeList nodeList)
        {
            foreach (XmlNode item in nodeList)
            {
                var line = new LineEntity()
                {
                    Id = long.Parse(item.SelectSingleNode("Id").InnerText, CultureInfo.InvariantCulture),
                    Name = item.SelectSingleNode("Name").InnerText,
                    ConductorMaterial = item.SelectSingleNode("ConductorMaterial").InnerText,
                    LineType = item.SelectSingleNode("LineType").InnerText,
                    IsUnderground = bool.Parse(item.SelectSingleNode("IsUnderground").InnerText),
                    R = float.Parse(item.SelectSingleNode("R").InnerText, CultureInfo.InvariantCulture),
                    ThermalConstantHeat = long.Parse(item.SelectSingleNode("ThermalConstantHeat").InnerText, CultureInfo.InvariantCulture),
                    FirstEnd = long.Parse(item.SelectSingleNode("FirstEnd").InnerText, CultureInfo.InvariantCulture),
                    SecondEnd = long.Parse(item.SelectSingleNode("SecondEnd").InnerText, CultureInfo.InvariantCulture)
                };
                if (entites.Any((ent) => ent.FirstEnd == line.SecondEnd && ent.SecondEnd == line.FirstEnd))
                {
                    continue;
                }
                entites.Add(line);
            }
        }

        public static void ToLatLon(double utmX, double utmY, int zoneUTM, out double latitude, out double longitude)
        {
            var isNorthHemisphere = true;

            var diflat = -0.00066286966871111111111111111111111111;
            var diflon = -0.0003868060578;

            var zone = zoneUTM;
            var c_sa = 6378137.000000;
            var c_sb = 6356752.314245;
            var e2 = Math.Pow(Math.Pow(c_sa, 2) - Math.Pow(c_sb, 2), 0.5) / c_sb;
            var e2cuadrada = Math.Pow(e2, 2);
            var c = Math.Pow(c_sa, 2) / c_sb;
            var x = utmX - 500000;
            var y = isNorthHemisphere ? utmY : utmY - 10000000;

            var s = (zone * 6.0) - 183.0;
            var lat = y / (c_sa * 0.9996);
            var v = c / Math.Pow(1 + (e2cuadrada * Math.Pow(Math.Cos(lat), 2)), 0.5) * 0.9996;
            var a = x / v;
            var a1 = Math.Sin(2 * lat);
            var a2 = a1 * Math.Pow(Math.Cos(lat), 2);
            var j2 = lat + (a1 / 2.0);
            var j4 = ((3 * j2) + a2) / 4.0;
            var j6 = ((5 * j4) + Math.Pow(a2 * Math.Cos(lat), 2)) / 3.0;
            var alfa = 3.0 / 4.0 * e2cuadrada;
            var beta = 5.0 / 3.0 * Math.Pow(alfa, 2);
            var gama = 35.0 / 27.0 * Math.Pow(alfa, 3);
            var bm = 0.9996 * c * (lat - (alfa * j2) + (beta * j4) - (gama * j6));
            var b = (y - bm) / v;
            var epsi = e2cuadrada * Math.Pow(a, 2) / 2.0 * Math.Pow(Math.Cos(lat), 2);
            var eps = a * (1 - (epsi / 3.0));
            var nab = (b * (1 - epsi)) + lat;
            var senoheps = (Math.Exp(eps) - Math.Exp(-eps)) / 2.0;
            var delt = Math.Atan(senoheps / Math.Cos(nab));
            var tao = Math.Atan(Math.Cos(delt) * Math.Tan(nab));

            longitude = (delt * (180.0 / Math.PI)) + s + diflon;
            latitude = ((lat + ((1 + (e2cuadrada * Math.Pow(Math.Cos(lat), 2)) - (3.0 / 2.0 * e2cuadrada * Math.Sin(lat) * Math.Cos(lat) * (tao - lat))) * (tao - lat))) * (180.0 / Math.PI)) + diflat;
        }
    }
}
