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
using Contoso.App.ViewModels;
using Microsoft.Toolkit.Uwp.Helpers;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Windows.Foundation.Metadata;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace Contoso.App.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CustomerListPage : Page
    {
        /// <summary>
        /// Initializes the page.
        /// </summary>
        public CustomerListPage()
        {
            InitializeComponent();
            DataContext = ViewModel;
            Window.Current.SizeChanged += CurrentWindow_SizeChanged;
        }

        public CustomerListPageViewModel ViewModel { get; set; } = new CustomerListPageViewModel();

        private void CurrentWindow_SizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily != "Windows.Mobile" && 
                e.Size.Width >= (double)App.Current.Resources["MediumWindowSnapPoint"])
            {
                mainCommandBar.DefaultLabelPosition = CommandBarDefaultLabelPosition.Right;
            }
            else
            {
                mainCommandBar.DefaultLabelPosition = CommandBarDefaultLabelPosition.Bottom;
            }
        }

        /// <summary>
        /// Navigates to a blank customer details page for the user to fill in.
        /// </summary>
        private void CreateCustomer_Click(object sender, RoutedEventArgs e) => GoToDetailsPage(null);

        private void CustomerSearchBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (CustomerSearchBox != null)
            {
                CustomerSearchBox.AutoSuggestBox.QuerySubmitted += CustomerSearchBox_QuerySubmitted;
                CustomerSearchBox.AutoSuggestBox.TextChanged += CustomerSearchBox_TextChanged;
                CustomerSearchBox.AutoSuggestBox.PlaceholderText = "Search customers...";
            }
        }

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
                    await DispatcherHelper.ExecuteOnUIThreadAsync(async () =>
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
                            customer.Address.StartsWith(parameter, StringComparison.OrdinalIgnoreCase) ||
                            customer.FirstName.StartsWith(parameter, StringComparison.OrdinalIgnoreCase) ||
                            customer.LastName.StartsWith(parameter, StringComparison.OrdinalIgnoreCase) ||
                            customer.Company.StartsWith(parameter, StringComparison.OrdinalIgnoreCase)))
                        .Select(customer => $"{customer.FirstName} {customer.LastName}"); 
                }
            }
        }

        private async void CustomerSearchBox_QuerySubmitted(AutoSuggestBox sender,
            AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (String.IsNullOrEmpty(args.QueryText))
            {
                await DispatcherHelper.ExecuteOnUIThreadAsync(async () => 
                    await ViewModel.GetCustomerListAsync());
            }
            else
            {
                string[] parameters = sender.Text.Split(new char[] { ' ' },
                    StringSplitOptions.RemoveEmptyEntries);

                var matches = ViewModel.Customers.Where(customer => parameters
                    .Any(parameter =>
                        customer.Address.StartsWith(parameter, StringComparison.OrdinalIgnoreCase) ||
                        customer.FirstName.StartsWith(parameter, StringComparison.OrdinalIgnoreCase) ||
                        customer.LastName.StartsWith(parameter, StringComparison.OrdinalIgnoreCase) ||
                        customer.Company.StartsWith(parameter, StringComparison.OrdinalIgnoreCase)))
                    .OrderByDescending(customer => parameters.Count(parameter =>
                        customer.Address.StartsWith(parameter, StringComparison.OrdinalIgnoreCase) ||
                        customer.FirstName.StartsWith(parameter, StringComparison.OrdinalIgnoreCase) ||
                        customer.LastName.StartsWith(parameter, StringComparison.OrdinalIgnoreCase) ||
                        customer.Company.StartsWith(parameter, StringComparison.OrdinalIgnoreCase)))
                    .ToList(); 

                await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
                {
                    ViewModel.Customers.Clear(); 
                    foreach (var match in matches)
                    {
                        ViewModel.Customers.Add(match); 
                    }
                });
            }
        }

        /// <summary>
        /// Workaround to support earlier versions of Windows.
        /// </summary>
        private void CommandBar_Loaded(object sender, RoutedEventArgs e)
        {
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
            {
                (sender as CommandBar).DefaultLabelPosition = CommandBarDefaultLabelPosition.Bottom;
            }
            else
            {
                (sender as CommandBar).DefaultLabelPosition = CommandBarDefaultLabelPosition.Right;
            }
        }

        /// <summary>
        /// Menu flyout click control for selecting a customer and displaying details.
        /// </summary>
        private void ViewDetails_Click(object sender, RoutedEventArgs e) => GoToDetailsPage(ViewModel.SelectedCustomer);

        /// <summary>
        /// Opens the order detail page for the user to create an order for the selected customer.
        /// </summary>
        private void AddOrder_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem)
            {
                GoToOrderPage((sender as FrameworkElement).DataContext as CustomerViewModel);
            }
            else
            {
                GoToOrderPage(ViewModel.SelectedCustomer);
            }
        }

        /// <summary>
        /// Navigates to the customer detail page for the provided customer.
        /// </summary>
        private void GoToDetailsPage(CustomerViewModel customer) =>
            Frame.Navigate(typeof(CustomerDetailPage), customer, new DrillInNavigationTransitionInfo());

        /// <summary>
        /// Navigates to the order detail page for the provided customer.
        /// </summary>
        private void GoToOrderPage(CustomerViewModel customer) =>
            Frame.Navigate(typeof(OrderDetailPage), customer.Model);

        private void DataGrid_Sorting(object sender, DataGridColumnEventArgs e) =>
            (sender as DataGrid).Sort(e.Column, ViewModel.Customers.Sort);
    }
}
