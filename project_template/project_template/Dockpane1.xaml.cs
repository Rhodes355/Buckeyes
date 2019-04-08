using System;
using System.Collections.Generic;
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
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System.Windows.Shapes;


namespace project_template
{
    /// <summary>
    /// Interaction logic for Dockpane1View.xaml
    /// </summary>
    public partial class Dockpane1View : UserControl
    {
        public Dockpane1View()
        {
            InitializeComponent();
            var mv = MapView.Active;
            if (mv == null)
                return;

            var layers = mv.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>();
            foreach (var l in layers)
            {
                cboLayerList.Items.Add(l.Name);
                cboLayerList.SelectedIndex = 0;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            cboLayerList.Items.Clear();
            var mv = MapView.Active;
            if (mv == null)
                return;

            var layers = mv.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>();
            foreach (var l in layers)
            {
                cboLayerList.Items.Add(l.Name);
                cboLayerList.SelectedIndex = 0;
            }

        }

        private async void CboLayerList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string lname = cboLayerList.SelectedItem.ToString();
            var mv = MapView.Active;
            FeatureLayer fl = mv.Map.FindLayers(lname).FirstOrDefault() as FeatureLayer;

            var fields = await QueuedTask.Run(() =>
            {
                FeatureClass fc = fl.GetFeatureClass();
                FeatureClassDefinition fcdef = fc.GetDefinition();
                return fcdef.GetFields();
            });

            LstFields.Items.Clear();
            for (int i = 0; i < fields.Count; i++)
            {
                Field fld = fields[i];
                if (fld.FieldType == FieldType.String)
                    LstFields.Items.Add(fld.Name);
            }
            LstFields.SelectAll();

        }

        private async void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            string text = txtQuery.Text;
            if (text == "")
                return;
            if (LstFields.SelectedItems.Count == 0)
                return;

            // get feature class first
            var mv = MapView.Active;
            string lname = cboLayerList.SelectedItem.ToString();
            FeatureLayer fl = mv.Map.FindLayers(lname).FirstOrDefault() as FeatureLayer;

            var fields = LstFields.SelectedItems;
            string query = "";
            for (int i = 0; i < LstFields.SelectedItems.Count; i++)
            {
                query += string.Format("{0} LIKE '{1}'", LstFields.SelectedItems[i].ToString(), text);
                if (i != LstFields.SelectedItems.Count - 1)
                    query += " OR ";
            }

            System.Windows.MessageBox.Show(query);

            QueryFilter filter = new QueryFilter
            {
                WhereClause = query
            };

            // MessageBox.Show(query);

            await QueuedTask.Run(() =>
            {
                int c = 0;
                using (Selection sel = fl.Select(filter))
                {
                    c = sel.GetCount();
                }
                // MessageBox.Show(c.ToString());
            });

        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
