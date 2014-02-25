using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using SearchCharms.Data;

// La plantilla de elemento Contrato de búsqueda está documentada en http://go.microsoft.com/fwlink/?LinkId=234240

namespace SearchCharms
{
    // TODO: Editar el manifiesto para habilitar la búsqueda   
    /// <summary>
    /// En esta página se muestran resultados de la búsqueda cuando se dirige una búsqueda global a esta aplicación.
    /// </summary>
    public sealed partial class SearchResultsPage1 : SearchCharms.Common.LayoutAwarePage
    {

        public SearchResultsPage1()
        {
            this.InitializeComponent();
        }

        public Dictionary<String, IEnumerable<SampleDataItem>> SearchResult { get; set; }
        /// <summary>
        /// Rellena la página con el contenido pasado durante la navegación.  Cualquier estado guardado se
        /// proporciona también al crear de nuevo una página a partir de una sesión anterior.
        /// </summary>
        /// <param name="navigationParameter">Valor de parámetro pasado a
        /// <see cref="Frame.Navigate(Type, Object)"/> cuando se solicitó inicialmente esta página.
        /// </param>
        /// <param name="pageState">Diccionario del estado mantenido por esta página durante una sesión
        /// anterior.  Será null la primera vez que se visite una página.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            var queryText = navigationParameter as String;
            var fltList = new List<Filter>();
            var TotMatchItem = 0;
            SearchResult = new Dictionary<string, IEnumerable<SampleDataItem>>();
            var groups = SampleDataSource.GetGroups("AllGroups");
            foreach (var grp in groups)
            {
                var matchItems = grp.Items.Where(item => item.Title.ToLower().Contains(queryText));
                int NoOfMatchItems = matchItems.Count();
                if (NoOfMatchItems > 0)
                {
                    SearchResult.Add(grp.Title, matchItems);
                    fltList.Add(new Filter(grp.Title, NoOfMatchItems));
                }
                TotMatchItem += NoOfMatchItems;
            }
            fltList.Insert(0, new Filter("All", TotMatchItem, true));
            this.DefaultViewModel["QueryText"] = '\u201c' + queryText + '\u201d';
            this.DefaultViewModel["Filters"] = fltList;
            this.DefaultViewModel["ShowFilters"] = fltList.Count > 1;
        }

        /// <summary>
        /// Se invoca al seleccionar un filtro con el objeto ComboBox en el estado de vista Snapped.
        /// </summary>
        /// <param name="sender">Instancia de ComboBox.</param>
        /// <param name="e">Datos de evento que describen cómo se cambió el filtro seleccionado.</param>
        void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedFilter = e.AddedItems.FirstOrDefault() as Filter;
            if (selectedFilter != null)
            {
                selectedFilter.Active = true;
                if (selectedFilter.Name.Equals("All"))
                {
                    var tempResults = new List<SampleDataItem>();
                    foreach (var group in SearchResult)
                        tempResults.AddRange(group.Value);
                    this.DefaultViewModel["Results"] = tempResults;
                }
                else if (SearchResult.ContainsKey(selectedFilter.Name))
                    this.DefaultViewModel["Results"] =
                        new List<SampleDataItem>(SearchResult[selectedFilter.Name]);

                object results;
                ICollection resultsCollection;
                if (this.DefaultViewModel.TryGetValue("Results", out results) &&
                    (resultsCollection = results as ICollection) != null &&
                    resultsCollection.Count != 0)
                {
                    VisualStateManager.GoToState(this, "ResultsFound", true);
                    return;
                }
            }
            VisualStateManager.GoToState(this, "NoResultsFound", true);
        }

        /// <summary>
        /// Se invoca al seleccionar un filtro con un objeto RadioButton en un estado distinto de Snapped.
        /// </summary>
        /// <param name="sender">Instancia de RadioButton seleccionada.</param>
        /// <param name="e">Datos de evento que describen cómo se seleccionó el objeto RadioButton.</param>
        void Filter_Checked(object sender, RoutedEventArgs e)
        {
            // Reflejar el cambio en el objeto CollectionViewSource usado por el ComboBox
            // correspondiente para asegurarse de que el cambio se refleja en estado Snapped
            if (filtersViewSource.View != null)
            {
                var filter = (sender as FrameworkElement).DataContext;
                filtersViewSource.View.MoveCurrentTo(filter);
            }
        }

        /// <summary>
        /// Modelo de vista que describe uno de los filtros disponibles para ver los resultados de la búsqueda.
        /// </summary>
        private sealed class Filter : SearchCharms.Common.BindableBase
        {
            private String _name;
            private int _count;
            private bool _active;

            public Filter(String name, int count, bool active = false)
            {
                this.Name = name;
                this.Count = count;
                this.Active = active;
            }
            public override String ToString()
            {
                return Description;
            }
            public String Name
            {
                get { return _name; }
                set { if (this.SetProperty(ref _name, value)) this.OnPropertyChanged("Description"); }
            }
            public int Count
            {
                get { return _count; }
                set { if (this.SetProperty(ref _count, value)) this.OnPropertyChanged("Description"); }
            }
            public bool Active
            {
                get { return _active; }
                set { this.SetProperty(ref _active, value); }
            }
            public String Description
            {
                get { return String.Format("{0} ({1})", _name, _count); }
            }
        }

        private void resultsGridView_ItemClick_1(object sender, ItemClickEventArgs e)
        {
            var itemId = ((SampleDataItem)e.ClickedItem).UniqueId;
            this.Frame.Navigate(typeof(ItemDetailPage), itemId);
        }
    }
}
