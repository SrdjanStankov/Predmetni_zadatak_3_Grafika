using System.Collections.Generic;
using System.Windows;
using System.Xml;
using Predmetni_zadatak_3_Grafika.Model;
using Predmetni_zadatak_3_Grafika.Services;

namespace Predmetni_zadatak_3_Grafika
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var doc = new XmlDocument();
            doc.Load("Geographic.xml");

            var substationEntities = new List<SubstationEntity>();
            var nodeEntities = new List<NodeEntity>();
            var switchEntities = new List<SwitchEntity>();

            Utils.AddEntities(substationEntities, doc.DocumentElement.SelectNodes("/NetworkModel/Substations/SubstationEntity"));
            Utils.AddEntities(nodeEntities, doc.DocumentElement.SelectNodes("/NetworkModel/Nodes/NodeEntity"));
            Utils.AddEntities(switchEntities, doc.DocumentElement.SelectNodes("/NetworkModel/Switches/SwitchEntity"));
            //Utils.AddLineEntities(lineEntities, doc.DocumentElement.SelectNodes("/NetworkModel/Lines/LineEntity"));
        }
    }
}
