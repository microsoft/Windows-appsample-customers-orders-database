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

using System;
using System.Linq;
using System.Threading.Tasks;
using Contoso.App.ViewModels;
using Microsoft.Toolkit.Uwp;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace Contoso.App.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CustomerListPage : Page
    {
        private DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        /// <summary>
        /// Initializes the page.
        /// </summary>
        public CustomerListPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets the app-wide ViewModel instance.
        /// </summary>
        public MainViewModel ViewModel => App.ViewModel;

        /// <summary>
        /// Initializes the AutoSuggestBox portion of the search box.
        /// </summary>
        private void CustomerSearchBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (CustomerSearchBox != null)
            {
                CustomerSearchBox.AutoSuggestBox.QuerySubmitted += CustomerSearchBox_QuerySubmitted;
                CustomerSearchBox.AutoSuggestBox.TextChanged += CustomerSearchBox_TextChanged;
                CustomerSearchBox.AutoSuggestBox.PlaceholderText = "Search customers...";
            }
        }

        /// <summary>
        /// Updates the search box items source when the user changes the search text.
        /// </summary>
        private async void CustomerSearchBox_TextChanged(AutoSuggestBox sender,
            AutoSuggestBoxTextChangedEventArgs args)
        {
            // We only want to get results when it was a user typing,
            // otherwise we assume the value got filled in by TextMemberPath
            // or the handler for SuggestionChosen.
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                // If no search query is entered, refresh the complete list.
                if (String.IsNullOrEmpty(sender.Text))
                {
                    await dispatcherQueue.EnqueueAsync(async () =>
                        await ViewModel.GetCustomerListAsync());
                    sender.ItemsSource = null;
                }
                else
                {
                    string[] parameters = sender.Text.Split(new char[] { ' ' },
                        StringSplitOptions.RemoveEmptyEntries);
                    sender.ItemsSource = ViewModel.Customers
                        .Where(customer => parameters.Any(parameter =>
                            customer.Address.StartsWith(parameter, StringComparison.OrdinalIgnoreCase) ||
                            customer.FirstName.StartsWith(parameter, StringComparison.OrdinalIgnoreCase) ||
                            customer.LastName.StartsWith(parameter, StringComparison.OrdinalIgnoreCase) ||
                            customer.Company.StartsWith(parameter, StringComparison.OrdinalIgnoreCase)))
                        .OrderByDescending(customer => parameters.Count(parameter =>
                            customer.Address.StartsWith(parameter) ||
                            customer.FirstName.StartsWith(parameter) ||
                            customer.LastName.StartsWith(parameter) ||
                            customer.Company.StartsWith(parameter)))
                        .Select(customer => $"{customer.FirstName} {customer.LastName}"); 
                }
            }
        }

        /// Filters or resets the customer list based on the search text.
        /// </summary>
        private async void CustomerSearchBox_QuerySubmitted(AutoSuggestBox sender,
            AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (String.IsNullOrEmpty(args.QueryText))
            {
                await ResetCustomerList();
            }
            else
            {
                await FilterCustomerList(args.QueryText);
            }
        }

        /// <summary>
        /// Resets the customer list.
        /// </summary>
        private async Task ResetCustomerList()
        {
            await dispatcherQueue.EnqueueAsync(async () =>
                await ViewModel.GetCustomerListAsync());
        }

        /// <summary>
        /// Filters the customer list based on the search text.
        /// </summary>
        private async Task FilterCustomerList(string text)
        {
            string[] parameters = text.Split(new char[] { ' ' },
                StringSplitOptions.RemoveEmptyEntries);

            var matches = ViewModel.Customers.Where(customer => parameters
                .Any(parameter =>
                    customer.Address.StartsWith(parameter, StringComparison.OrdinalIgnoreCase) ||
                    customer.FirstName.StartsWith(parameter, StringComparison.OrdinalIgnoreCase) ||
                    customer.LastName.StartsWith(parameter, StringComparison.OrdinalIgnoreCase) ||
                    customer.Company.StartsWith(parameter, StringComparison.OrdinalIgnoreCase)))
                .OrderByDescending(customer => parameters.Count(parameter =>
                    customer.Address.StartsWith(parameter) ||
                    customer.FirstName.StartsWith(parameter) ||
                    customer.LastName.StartsWith(parameter) ||
                    customer.Company.StartsWith(parameter)))
                .ToList();

            await dispatcherQueue.EnqueueAsync(() =>
            {
                ViewModel.Customers.Clear();
                foreach (var match in matches)
                {
                    ViewModel.Customers.Add(match);
                }
            });
        }

        /// <summary>
        /// Resets the customer list when leaving the page.
        /// </summary>
        protected async override void OnNavigatedFrom(NavigationEventArgs e)
        {
            await ResetCustomerList();
        }

        /// <summary>
        /// Applies any existing filter when navigating to the page.
        /// </summary>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(CustomerSearchBox.AutoSuggestBox.Text))
            {
                await FilterCustomerList(CustomerSearchBox.AutoSuggestBox.Text);
            }
        }

        /// <summary>
        /// Menu flyout click control for selecting a customer and displaying details.
        /// </summary>
        private void ViewDetails_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedCustomer != null)
            {
                Frame.Navigate(typeof(CustomerDetailPage), ViewModel.SelectedCustomer.Model.Id,
                    new DrillInNavigationTransitionInfo());
            }
        }

        private void DataGrid_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e) =>
            Frame.Navigate(typeof(CustomerDetailPage), ViewModel.SelectedCustomer.Model.Id,
                    new DrillInNavigationTransitionInfo());

        /// <summary>
        /// Navigates to a blank customer details page for the user to fill in.
        /// </summary>
        private void CreateCustomer_Click(object sender, RoutedEventArgs e) =>
            Frame.Navigate(typeof(CustomerDetailPage), null, new DrillInNavigationTransitionInfo());

        /// <summary>
        /// Reverts all changes to the row if the row has changes but a cell is not currently in edit mode.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGrid_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Escape &&
                ViewModel.SelectedCustomer != null &&
                ViewModel.SelectedCustomer.IsModified &&
                !ViewModel.SelectedCustomer.IsInEdit)
            {
                (sender as DataGrid).CancelEdit(DataGridEditingUnit.Row);
            }
        }

        /// <summary>
        /// Selects the tapped customer. 
        /// </summary>
        private void DataGrid_RightTapped(object sender, RightTappedRoutedEventArgs e) =>
            ViewModel.SelectedCustomer = (e.OriginalSource as FrameworkElement).DataContext as CustomerViewModel;

        /// <summary>
        /// Opens the order detail page for the user to create an order for the selected customer.
        /// </summary>
        private void AddOrder_Click(object sender, RoutedEventArgs e) =>
            Frame.Navigate(typeof(OrderDetailPage), ViewModel.SelectedCustomer.Model.Id);

        /// <summary>
        /// Sorts the data in the DataGrid.
        /// </summary>
        private void DataGrid_Sorting(object sender, DataGridColumnEventArgs e) =>
            (sender as DataGrid).Sort(e.Column, ViewModel.Customers.Sort);
    }
}
