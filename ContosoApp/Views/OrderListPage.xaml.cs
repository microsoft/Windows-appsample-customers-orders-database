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
using Contoso.Models;
using Contoso.App.ViewModels;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Windows.ApplicationModel.Email;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace Contoso.App.Views
{
    /// <summary>
    /// Displays the list of orders.
    /// </summary>
    public sealed partial class OrderListPage : Page
    {
        /// <summary>
        /// We use this object to bind the UI to our data. 
        /// </summary>
        public OrderListPageViewModel ViewModel { get; } = new OrderListPageViewModel();

        /// <summary>
        /// Creates a new OrderListPage.
        /// </summary>
        public OrderListPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Retrieve the list of orders when the user navigates to the page. 
        /// </summary>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (ViewModel.Orders.Count < 1)
            {
                ViewModel.LoadOrders();
            }
        }

        /// <summary>
        /// Opens the order in the order details page for editing. 
        /// </summary>
        private void EditButton_Click(object sender, RoutedEventArgs e) => 
            Frame.Navigate(typeof(OrderDetailPage), ViewModel.SelectedOrder.Id);

        /// <summary>
        /// Deletes the currently selected order.
        /// </summary>
        private async void DeleteOrder_Click(object sender, RoutedEventArgs e)
        {
            try
            { 
                var deleteOrder = ViewModel.SelectedOrder;
                await ViewModel.DeleteOrder(deleteOrder);
            }
            catch(OrderDeletionException ex)
            {
                var dialog = new ContentDialog()
                {
                    Title = "Unable to delete order",
                    Content = $"There was an error when we tried to delete " + 
                        $"invoice #{ViewModel.SelectedOrder.InvoiceNumber}:\n{ex.Message}",
                    PrimaryButtonText = "OK"
                };
                await dialog.ShowAsync();
            }
        }

        /// <summary>
        /// Initializes the AutoSuggestBox portion of the search box.
        /// </summary>
        private void OrderSearchBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is UserControls.CollapsibleSearchBox searchBox)
            {
                searchBox.AutoSuggestBox.QuerySubmitted += OrderSearch_QuerySubmitted;
                searchBox.AutoSuggestBox.TextChanged += OrderSearch_TextChanged;
                searchBox.AutoSuggestBox.PlaceholderText = "Search orders...";
                searchBox.AutoSuggestBox.ItemTemplate = (DataTemplate)Resources["SearchSuggestionItemTemplate"];
                searchBox.AutoSuggestBox.ItemContainerStyle = (Style)Resources["SearchSuggestionItemStyle"];
                searchBox.AutoSuggestBox.ItemsSource = ViewModel.OrderSuggestions;
            }
        }

        /// <summary>
        /// Creates an email about the currently selected invoice. 
        /// </summary>
        private async void EmailButton_Click(object sender, RoutedEventArgs e)
        {

            var emailMessage = new EmailMessage
            {
                Body = $"Dear {ViewModel.SelectedOrder.CustomerName},",
                Subject = "A message from Contoso about order " +
                    $"#{ViewModel.SelectedOrder.InvoiceNumber} placed on " +
                    $"{ViewModel.SelectedOrder.DatePlaced.ToString("MM/dd/yyyy")}"
            };

            if (!string.IsNullOrEmpty(ViewModel.SelectedCustomer.Email))
            {
                var emailRecipient = new EmailRecipient(ViewModel.SelectedCustomer.Email);
                emailMessage.To.Add(emailRecipient);
            }
            await EmailManager.ShowComposeNewEmailAsync(emailMessage);

        }

        /// <summary>
        /// Searches the list of orders.
        /// </summary>
        private void OrderSearch_QuerySubmitted(AutoSuggestBox sender, 
            AutoSuggestBoxQuerySubmittedEventArgs args) => 
                ViewModel.QueryOrders(args.QueryText);

        /// <summary>
        /// Updates the suggestions for the AutoSuggestBox as the user types. 
        /// </summary>
        private void OrderSearch_TextChanged(AutoSuggestBox sender, 
            AutoSuggestBoxTextChangedEventArgs args)
        {
            // We only want to get results when it was a user typing, 
            // otherwise we assume the value got filled in by TextMemberPath 
            // or the handler for SuggestionChosen
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                ViewModel.UpdateOrderSuggestions(sender.Text);
            }
        }

        /// <summary>
        /// Navigates to the order detail page when the user
        /// double-clicks an order. 
        /// </summary>
        private void DataGrid_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e) => 
            Frame.Navigate(typeof(OrderDetailPage), ViewModel.SelectedOrder.Id);

        // Navigates to the details page for the selected customer when the user presses SPACE.
        private void DataGrid_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Space)
            {
                Frame.Navigate(typeof(OrderDetailPage), ViewModel.SelectedOrder.Id);
            }
        }

        /// <summary>
        /// Selects the tapped order. 
        /// </summary>
        private void DataGrid_RightTapped(object sender, RightTappedRoutedEventArgs e) =>
            ViewModel.SelectedOrder = (e.OriginalSource as FrameworkElement).DataContext as Order;

        /// <summary>
        /// Navigates to the order details page.
        /// </summary>
        private void MenuFlyoutViewDetails_Click(object sender, RoutedEventArgs e) =>
            Frame.Navigate(typeof(OrderDetailPage), ViewModel.SelectedOrder.Id, new DrillInNavigationTransitionInfo());

        /// <summary>
        /// Sorts the data in the DataGrid.
        /// </summary>
        private void DataGrid_Sorting(object sender, DataGridColumnEventArgs e) =>
            (sender as DataGrid).Sort(e.Column, ViewModel.Orders.Sort);
    }
}
