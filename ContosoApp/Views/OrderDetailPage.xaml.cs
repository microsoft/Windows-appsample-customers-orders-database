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

using Contoso.Models;
using Contoso.App.ViewModels;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.Email;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Contoso.App.Views
{
    /// <summary>
    /// Displays and edits an order.
    /// </summary>
    public sealed partial class OrderDetailPage : Page, INotifyPropertyChanged
    {
        /// <summary>
        /// Initializes the page.
        /// </summary>
        public OrderDetailPage() => InitializeComponent();

        /// <summary>
        /// Stores the view model. 
        /// </summary>
        private OrderViewModel _viewModel;

        /// <summary>
        /// We use this object to bind the UI to our data.
        /// </summary>
        public OrderViewModel ViewModel
        {
            get => _viewModel;
            set
            {
                if (_viewModel != value)
                {
                    _viewModel = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Loads the specified order, a cached order, or creates a new order.
        /// </summary>
        /// <param name="e">Info about the event.</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            var guid = (Guid)e.Parameter;
            var customer = App.ViewModel.Customers.Where(cust => cust.Model.Id == guid).FirstOrDefault();

            if (customer != null)
            {
                // Order is a new order
                ViewModel = new OrderViewModel(new Order(customer.Model));
            }
            else
            {
                // Order is an existing order.
                var order = await App.Repository.Orders.GetAsync(guid);
                ViewModel = new OrderViewModel(order);
            }

            base.OnNavigatedTo(e);
        }

        /// <summary>
        /// Check whether there are unsaved changes and warn the user.
        /// </summary>
        protected async override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (ViewModel.IsModified)
            {
                var saveDialog = new SaveChangesDialog()
                {
                    Title = $"Save changes to Invoice # {ViewModel.InvoiceNumber.ToString()}?",
                    Content = $"Invoice # {ViewModel.InvoiceNumber.ToString()} " + 
                        "has unsaved changes that will be lost. Do you want to save your changes?"
                };

                await saveDialog.ShowAsync();
                SaveChangesDialogResult result = saveDialog.Result;

                switch (result)
                {
                    case SaveChangesDialogResult.Save:
                        await ViewModel.SaveOrderAsync();
                        break;
                    case SaveChangesDialogResult.DontSave:
                        break;
                    case SaveChangesDialogResult.Cancel:
                        if (e.NavigationMode == NavigationMode.Back)
                        {
                            Frame.GoForward();
                        }
                        else
                        {
                            Frame.GoBack();
                        }
                        e.Cancel = true;

                        // This flag gets cleared on navigation, so restore it. 
                        ViewModel.IsModified = true; 
                        break;
                }
            }

            base.OnNavigatingFrom(e);
        }

        /// <summary>
        /// Creates an email to the current customer.
        /// </summary>
        private async void emailButton_Click(object sender, RoutedEventArgs e)
        {
            var emailMessage = new EmailMessage
            {
                Body = $"Dear {ViewModel.CustomerName},",
                Subject = "A message from Contoso about order " +
                    $"#{ViewModel.InvoiceNumber} placed on {ViewModel.DatePlaced.ToString("MM/dd/yyyy")} "
            };

            if (!string.IsNullOrEmpty(ViewModel.Customer.Email))
            {
                var emailRecipient = new EmailRecipient(ViewModel.Customer.Email);
                emailMessage.To.Add(emailRecipient);
            }

            await EmailManager.ShowComposeNewEmailAsync(emailMessage);
        }

        /// <summary>
        /// Reloads the order.
        /// </summary>
        private async void RefreshButton_Click(object sender, RoutedEventArgs e) => 
            ViewModel = await OrderViewModel.CreateFromGuid(ViewModel.Id);

        /// <summary>
        /// Reverts the page.
        /// </summary>
        private async void RevertButton_Click(object sender, RoutedEventArgs e)
        {
            var saveDialog = new SaveChangesDialog()
            {
                Title = $"Save changes to Invoice # {ViewModel.InvoiceNumber.ToString()}?",
                Content = $"Invoice # {ViewModel.InvoiceNumber.ToString()} " + 
                    "has unsaved changes that will be lost. Do you want to save your changes?"
            };
            await saveDialog.ShowAsync();
            SaveChangesDialogResult result = saveDialog.Result;

            switch (result)
            {
                case SaveChangesDialogResult.Save:
                    await ViewModel.SaveOrderAsync();
                    ViewModel = await OrderViewModel.CreateFromGuid(ViewModel.Id);
                    break;
                case SaveChangesDialogResult.DontSave:
                    ViewModel = await OrderViewModel.CreateFromGuid(ViewModel.Id);
                    break;
                case SaveChangesDialogResult.Cancel:
                    break;
            }         
        }

        /// <summary>
        /// Saves the current order.
        /// </summary>
        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            { 
                await ViewModel.SaveOrderAsync();
                Bindings.Update();
            }
            catch (OrderSavingException ex)
            {
                var dialog = new ContentDialog()
                {
                    Title = "Unable to save",
                    Content = $"There was an error saving your order:\n{ex.Message}", 
                    PrimaryButtonText = "OK"                 
                };

                await dialog.ShowAsync();
            }
        }

        /// <summary>
        /// Queries for products.
        /// </summary>
        private void ProductSearchBox_TextChanged(AutoSuggestBox sender, 
            AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                ViewModel.UpdateProductSuggestions(sender.Text);
            }
        }

        /// <summary>
        /// Notifies the page that a new item was chosen.
        /// </summary>
        private void ProductSearchBox_SuggestionChosen(AutoSuggestBox sender, 
            AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            if (args.SelectedItem != null)
            {
                var selectedProduct = args.SelectedItem as Product;
                ViewModel.NewLineItem.Product = selectedProduct;
            }
        }

        /// <summary>
        /// Adds the new line item to the list of line items.
        /// </summary>
        private void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.LineItems.Add(ViewModel.NewLineItem.Model);
            ClearCandidateProduct();
        }

        /// <summary>
        /// Clears the new line item without adding it to the list of line items.
        /// </summary>
        private void CancelProductButton_Click(object sender, RoutedEventArgs e)
        {
            ClearCandidateProduct();
        }

        /// <summary>
        /// Cleears the new line item entry area.
        /// </summary>
        private void ClearCandidateProduct()
        {
            ProductSearchBox.Text = string.Empty;
            ViewModel.NewLineItem = new LineItemViewModel();
        }

        /// <summary>
        /// Removes a line item from the order.
        /// </summary>
        private void RemoveProduct_Click(object sender, RoutedEventArgs e) =>
            ViewModel.LineItems.Remove((sender as FrameworkElement).DataContext as LineItem);

        /// <summary>
        /// Fired when a property value changes. 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notifies listeners that a property value changed. 
        /// </summary>
        private void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
