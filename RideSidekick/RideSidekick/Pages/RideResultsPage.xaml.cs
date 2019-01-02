using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Rideshare.Uber.Sdk.Models;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Xaml;
using static RideSidekick.Pages.RideSearchPage;

namespace RideSidekick.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RideResultsPage : ContentPage
    {
        public ObservableCollection<KeyValuePair<Route, PriceEstimate>> Items { get; set; }

        public RideResultsPage(Dictionary<Route, PriceEstimate> prices)
        {
            InitializeComponent();

            var geocoder = new Geocoder();
            var orderedPrices = prices.OrderBy(p => p.Value.HighEstimate)
                                      .ThenBy(p => p.Key)
                                      .ToList();

            this.Items = new ObservableCollection<KeyValuePair<Route, PriceEstimate>>(orderedPrices);
			RideResultsList.ItemsSource = orderedPrices;
        }

        async void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item == null)
                return;

            await DisplayAlert("Item Tapped", "An item was tapped.", "OK");

            //Deselect Item
            ((ListView)sender).SelectedItem = null;
        }
    }
}
