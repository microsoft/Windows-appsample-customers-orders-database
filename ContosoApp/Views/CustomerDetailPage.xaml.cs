//  ---------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// 
//  The MIT License (MIT)
// 
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
// 
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
// 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
//  ---------------------------------------------------------------------------------

using ContosoModels;
using ContosoApp.ViewModels;
using Windows.Foundation.Metadata;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace ContosoApp.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CustomerDetailPage : Page
    {
        /// <summary>
        /// Used to bind the UI to the data.
        /// </summary>
        public CustomerDetailPageViewModel ViewModel { get; set; } = new CustomerDetailPageViewModel();

        /// <summary>
        /// Initializes the page.
        /// </summary>
        public CustomerDetailPage()
        {
            InitializeComponent();
            DataContext = ViewModel;
        }

        /// <summary>
        /// Displays the selected customer data.
        /// </summary>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            CustomerViewModel customer = e.Parameter as CustomerViewModel;
            if (customer == null)
            {
                ViewModel.Customer = new CustomerViewModel();
                Bindings.Update();
                PageHeaderText.Text = "New customer";
            }
            else if (ViewModel.Customer != customer)
            {
                ViewModel.Customer = customer;
                Bindings.Update();
            }
            base.OnNavigatedTo(e);
        }

        /// <summary>
        /// Navigates from the current page.
        /// </summary>
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            ViewModel.IsInEdit = false;

            base.OnNavigatingFrom(e);
        }

        /// <summary>
        /// Queries the database for a customer result matching the search text entered.
        /// </summary>
        private async void CustomerSearchBox_TextChanged(AutoSuggestBox sender,
            AutoSuggestBoxTextChangedEventArgs args)
        {
            // We only want to get results when it was a user typing,
            // otherwise we assume the value got filled in by TextMemberPath
            // or the handler for SuggestionChosen
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                // If no search query is entered, refresh the complete list.
                if (string.IsNullOrEmpty(sender.Text))
                {
                    sender.ItemsSource = null;
                }
                else
                {
                    ContosoDataSource data = new ContosoDataSource();
                    sender.ItemsSource = await data.Customers.GetAsync(sender.Text);
                }
            }
        }

        /// <summary>
        /// Search by customer name, email, or phone number, then display results.
        /// </summary>
        private void CustomerSearchBox_QuerySubmitted(AutoSuggestBox sender,
            AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            Customer customer = args.ChosenSuggestion as Customer;
            if (customer != null)
            {
                ViewModel.Customer = new CustomerViewModel(customer);
            }
            Bindings.Update();
        }

        /// <summary>
        /// A workaround for earlier versions of Windows 10.
        /// </summary>
        private void CommandBar_Loaded(object sender, RoutedEventArgs e)
        {
            if (ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Controls.CommandBar",
                "DefaultLabelPosition"))
            {
                ((CommandBar)sender).DefaultLabelPosition = CommandBarDefaultLabelPosition.Right;
            }
        }

        /// <summary>
        /// Navigates to the order page for the customer.
        /// </summary>
        private void ViewOrderButton_Click(object sender, RoutedEventArgs e) =>
            Frame.Navigate(typeof(OrderDetailPage), ((FrameworkElement)sender).DataContext,
                new DrillInNavigationTransitionInfo());

        /// <summary>
        /// Adds a new order for the customer.
        /// </summary>
        private void AddOrder_Click(object sender, RoutedEventArgs e) =>
            Frame.Navigate(typeof(OrderDetailPage), ViewModel.Customer.Model);

        private void CancelEditButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.Customer.IsNewCustomer == true
                && Frame.CanGoBack == true)
            {
                Frame.GoBack();
            }
        }
    }
}
