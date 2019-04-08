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
using System.Windows.Shapes;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;


namespace Interactivity
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

        private void button_Click(object sender, RoutedEventArgs e)
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

        private async void cboLayerList_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

            lstFields.Items.Clear();
            for (int i = 0; i < fields.Count; i++)
            {
                Field fld = fields[i];
                if (fld.FieldType == FieldType.String)
                    lstFields.Items.Add(fld.Name);
            }
            lstFields.SelectAll();
        }

        private async void btnFind_Click(object sender, RoutedEventArgs e)
        {
            string text = txtQuery.Text;
            if (text == "")
                return;
            if (lstFields.SelectedItems.Count == 0)
                return;

            // get feature class first
            var mv = MapView.Active;
            string lname = cboLayerList.SelectedItem.ToString();
            FeatureLayer fl = mv.Map.FindLayers(lname).FirstOrDefault() as FeatureLayer;

            var fields = lstFields.SelectedItems;
            string query = "";
            for (int i = 0; i < lstFields.SelectedItems.Count; i++)
            {
                query += string.Format("{0} LIKE '{1}'", lstFields.SelectedItems[i].ToString(), text);
                if (i != lstFields.SelectedItems.Count - 1)
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
    }
}
