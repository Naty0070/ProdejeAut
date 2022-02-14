using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;

namespace WpfAppTS {
    public partial class XmlReader : Window {
        private List<Sale> Sales { get; set; }
        private string Path { get; set; }
        private XmlDocument Doc { get; set; }
        private SeriesCollection Series { get; set; }
        private double Price { get; set; }
        private double Dph { get; set; }
        private DateTime Date { get; set; }
        private string Model { get; set; }
        private const string CAUT = "Pozor!";
        private const string MESSAGE = "Prosím načtěte nejdřív soubor!";
        private const Visibility SHOW = Visibility.Visible;
        private const string FILE = "Prodeje_aut.xml";

        public XmlReader() {
            InitializeComponent();
            Sales = null;
            Path = "";
            Doc = new();
            Series = null;
            Price = 0;
            Dph = 0;
            Date = DateTime.MinValue;
            Model = "";
        }

        private void OpenFileButton_Click(object sender, RoutedEventArgs e) {
            do {
                OpenFileDialog openFileDialog = new();
                openFileDialog.Filter = "Xml files (*.xml)|*.xml";
                openFileDialog.Title = "Vyber soubor s názvem: Prodeje_aut.xml";
                if (openFileDialog.ShowDialog(this) == true) {
                    Path = openFileDialog.FileName;
                    if (Path.Contains(FILE)) {
                        Doc.Load(Path);
                        ReadXML();
                        dgSold.Visibility = SHOW;
                        spMenu.Visibility = SHOW;
                        lbSales.Visibility = SHOW;
                    } else {
                        MessageBox.Show("Tento program je určen pro soubor Prodeje_aut! Prosím načtěte tento soubor!", "Pozor!");
                    }
                } else { break; }
            } while (!Path.Contains(FILE));
        }
        private void CountDph_Click(object sender, RoutedEventArgs e) {
            if (Sales != null) {
                ShowDPH();
            } else {
                ShowMge();
            }
        }
        private void btWeekend_Click(object sender, RoutedEventArgs e) {
            if (Sales != null) {
                VikendProdeje();
            } else {
                ShowMge();
            }
        }
        private void btnAdd_Click(object sender, RoutedEventArgs e) {
            if (Sales != null) {
                AddRow();
                UpdateDG();
            } else {
                ShowMge();
            }
        }
        private void btnRem_Click(object sender, RoutedEventArgs e) {
            if (Sales != null) {
                DeleteRowAt();
                UpdateDG();
            } else {
                ShowMge();
            }
        }
        private void btPieChart_Click(object sender, RoutedEventArgs e) {
            if (Sales != null) {
                SetPieCartSales();
                
            } else {
                ShowMge();
            }
        }
        private static void ShowMge() {
            MessageBox.Show(MESSAGE, CAUT);
        }
        private void ReadXML() {
            Sales = new();
            foreach (XmlNode node in Doc.DocumentElement) {
                Model = node["Model"].InnerText;
                Date = DateTime.Parse(node["Datum"].InnerText);
                Price = double.Parse(node["Cena"].InnerText);
                Dph = double.Parse(node["DPH"].InnerText);
                Sales.Add(new Sale(Model, Date, Price, Dph));
            }
            UpdateDG();
        }
        private void AddRow() {
            bool ok = true;
            ok = double.TryParse(txCena.Text, out double price);
            ok = double.TryParse(txDPH.Text, out double dph);
            if (ok) {
                Price = price;
                Dph = dph;
                Date = DateTime.Now;
                Model = cbModel.Text;
                XmlElement elem = Doc.CreateElement("Prodej");
                XmlNode nodeModel = elem.AppendChild(Doc.CreateElement("Model"));
                XmlNode nodeDatum = elem.AppendChild(Doc.CreateElement("Datum"));
                XmlNode nodeCena = elem.AppendChild(Doc.CreateElement("Cena"));
                XmlNode nodeDPH = elem.AppendChild(Doc.CreateElement("DPH"));
                nodeModel.InnerText = Model;
                nodeDatum.InnerText = Date.ToString();
                nodeCena.InnerText = Price.ToString();
                nodeDPH.InnerText = Dph.ToString();
                Doc.DocumentElement.AppendChild(elem);
                Doc.Save(Path);
                Sales.Add(new Sale(Model, Date, Price, Dph));
            } else { MessageBox.Show("Špatně zadaný formát ceny nebo daně!", CAUT); }
        }
        private void DeleteRowAt() {
            int selected = dgSold.SelectedIndex;
            if (selected != -1) {
                XmlElement root = Doc.DocumentElement;
                XmlNode sale = root.ChildNodes.Item(selected);
                root.RemoveChild(sale);
                Doc.Save(Path);
                Sales.RemoveAt(selected);
            } else { MessageBox.Show("Prosím označte řádek!", CAUT); }
        }
        private void UpdateDG() {
            var load = Sales.Select(x => new {
                Model = x.Model,
                Datum = x.Date.ToLocalTime().ToString("dd/MM/yyyy"),
                Cena = $"{x.Price:N} Kč",
                DPH = $"{x.Dph} %"
            });
            dgSold.ItemsSource = load;
        }
        private void VikendProdeje() {
            var weekends = Sales.Select(x => new {
                model = x.Model,
                bezDPH = x.Price,
                Dph=x.Dph,
                sDPH = x.Price * (1 + x.Dph / 100),
                datum = x.Date
            }).Where(x => x.datum.DayOfWeek is DayOfWeek.Sunday or DayOfWeek.Saturday);

            var grouped = weekends.GroupBy(x => new { x.model,x.Dph })
                .OrderBy(g => g.Key.model).ThenBy( g=>g.Key.Dph)
               .Select(g => new {
                   _Model_Vikend_Sum = $"{g.Key.model}\n{g.Sum(x => x.bezDPH):N} Kč",
                   _S_DPH = $"{g.Key.Dph} %\n{g.Sum(x => x.sDPH):N} Kč",
               });
            dgWeekend.ItemsSource = grouped;
            dgWeekend.Visibility = SHOW;
            lbWeekend.Visibility = SHOW;
        }
        private void ShowDPH() {
            var sDph = Sales.Select(x => new {
                bezDPH = $"{x.Model}\n{x.Price:N} Kč",
                sDPH = $"{x.Dph} %\n{x.Price * (1 + x.Dph / 100):N} Kč"
            });
            var groped = sDph.GroupBy(x => new { x.bezDPH, x.sDPH })
                .OrderBy(g => g.Key.bezDPH).ThenBy(g => g.Key.sDPH).
                Select(g => new {
                    _Model_Bez_DPH = g.Key.bezDPH,
                    _S_DPH = g.Key.sDPH,
                });
            dgDPH.ItemsSource = groped;
            dgDPH.Visibility = SHOW;
            lbDph.Visibility = SHOW;
        }

        private void SetPieCartSales() {
            Dictionary<string, int> counts = new() { };
            foreach (var sale in Sales) {
                if (counts.ContainsKey(sale.Model))
                    counts[sale.Model]++;
                else {
                    counts.Add(sale.Model, 1);
                }
            }
            Series = new() { };
            foreach (var group in counts) {
                Series.Add(new PieSeries {
                    Title = group.Key,
                    Values = new ChartValues<ObservableValue> { new ObservableValue(counts[group.Key]) },
                    DataLabels = true
                });
            }
            pchSales.Series = Series;
            lbPieChSales.Visibility = SHOW;
        }
    }
}
