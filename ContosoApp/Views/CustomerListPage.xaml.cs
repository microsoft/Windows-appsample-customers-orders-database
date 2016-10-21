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

using ContosoApp.ViewModels;
using PropertyChanged;
using System;
using System.Linq;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace ContosoApp.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    [ImplementPropertyChanged]
    public sealed partial class CustomerListPage : Page
    {
        private enum SortOption { ByName, ByCompany };
        private enum GroupOption { ByName, ByCompany };
        public CustomerListPageViewModel ViewModel { get; set; } = new CustomerListPageViewModel();

        /// <summary>
        /// Initializes the page.
        /// </summary>
        public CustomerListPage()
        {
            InitializeComponent();
            DataContext = ViewModel;
            if (ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Controls.CommandBar", "DefaultLabelPosition"))
            {
                Window.Current.SizeChanged += CurrentWindow_SizeChanged;
            }
        }

        private void CurrentWindow_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily != "Windows.Mobile" && e.Size.Width >= (double)App.Current.Resources["MediumWindowSnapPoint"])
            {
                mainCommandBar.DefaultLabelPosition = CommandBarDefaultLabelPosition.Right;
            }
            else
            {
                mainCommandBar.DefaultLabelPosition = CommandBarDefaultLabelPosition.Bottom;
            }
        }

        /// <summary>
        /// Retrieves a list of customers when the user navigates to the page.
        /// </summary>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back
                && ViewModel.SelectedCustomer != null
                && ViewModel.SelectedCustomer.HasChanges == true)
            {
                ViewModel.UpdateCustomerInList(ViewModel.SelectedCustomer);
                ViewModel.SelectedCustomer.HasChanges = false;
            }
        }

        /// <summary>
        /// Navigates to a blank customer details page for the user to fill in.
        /// </summary>
        private void CreateCustomer_Click(object sender, RoutedEventArgs e) =>
            GoToDetailsPage(null);
        private void CustomerSearchBox_Loaded(object sender, RoutedEventArgs e)
        {
            UserControls.CollapsibleSearchBox searchBox = sender as UserControls.CollapsibleSearchBox;

            if (searchBox != null)
            {
                searchBox.AutoSuggestBox.QuerySubmitted += CustomerSearchBox_QuerySubmitted;
                searchBox.AutoSuggestBox.TextChanged += CustomerSearchBox_TextChanged;
                searchBox.AutoSuggestBox.PlaceholderText = "Search customers...";
            }
        }

        /// <summary>
        /// Provide suggestions in the search box as the user types.
        /// </summary>
        private void CustomerSearchBox_TextChanged(AutoSuggestBox sender,
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
                    var cvs = Resources["CustomersCVS"] as CollectionViewSource;
                    if (cvs != null)
                    {
                        if (string.IsNullOrWhiteSpace(sender.Text))
                        {
                            cvs.Source = ViewModel.Customers;
                        }
                    }
                    sender.ItemsSource = null;
                }
                else
                {
                    string[] parameters = sender.Text.Split(new char[] { ' ' },
                        StringSplitOptions.RemoveEmptyEntries);

                    sender.ItemsSource = ViewModel.Customers
                        .Where(x => parameters
                            .Any(y =>
                                x.Address.StartsWith(y, StringComparison.OrdinalIgnoreCase) ||
                                x.FirstName.StartsWith(y, StringComparison.OrdinalIgnoreCase) ||
                                x.LastName.StartsWith(y, StringComparison.OrdinalIgnoreCase) ||
                                x.Company.StartsWith(y, StringComparison.OrdinalIgnoreCase)))
                        .OrderByDescending(x => parameters
                            .Count(y =>
                                x.Address.StartsWith(y, StringComparison.OrdinalIgnoreCase) ||
                                x.FirstName.StartsWith(y, StringComparison.OrdinalIgnoreCase) ||
                                x.LastName.StartsWith(y, StringComparison.OrdinalIgnoreCase) ||
                                x.Company.StartsWith(y, StringComparison.OrdinalIgnoreCase)));
                }
            }
        }

        /// <summary>
        /// Search by customer first or last name, address, or company then display results.
        /// </summary>
        private void CustomerSearchBox_QuerySubmitted(AutoSuggestBox sender,
            AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            var cvs = Resources["CustomersCVS"] as CollectionViewSource;
            if (cvs != null)
            {
                if (string.IsNullOrWhiteSpace(args.QueryText))
                {
                    if (string.IsNullOrEmpty(args.QueryText))
                    {
                        cvs.Source = ViewModel.Customers;
                    }
                }
                else
                {
                    cvs.IsSourceGrouped = false;
                    string[] parameters = sender.Text.Split(new char[] { ' ' },
                        StringSplitOptions.RemoveEmptyEntries);

                    cvs.Source = ViewModel.Customers
                        .Where(x => parameters
                            .Any(y =>
                                x.Address.StartsWith(y, StringComparison.OrdinalIgnoreCase) ||
                                x.FirstName.StartsWith(y, StringComparison.OrdinalIgnoreCase) ||
                                x.LastName.StartsWith(y, StringComparison.OrdinalIgnoreCase) ||
                                x.Company.StartsWith(y, StringComparison.OrdinalIgnoreCase)))
                        .OrderByDescending(x => parameters
                            .Count(y =>
                                x.Address.StartsWith(y, StringComparison.OrdinalIgnoreCase) ||
                                x.FirstName.StartsWith(y, StringComparison.OrdinalIgnoreCase) ||
                                x.LastName.StartsWith(y, StringComparison.OrdinalIgnoreCase) ||
                                x.Company.StartsWith(y, StringComparison.OrdinalIgnoreCase)));
                }
            }
        }


        #region Context Menu Handlers
        private void MenuFlyoutSortByName_Click(object sender, RoutedEventArgs e) =>
            SortList(SortOption.ByName);

        private void MenuFlyoutSortByCompany_Click(object sender, RoutedEventArgs e) =>
            SortList(SortOption.ByCompany);

        private void MenuFlyoutGroupByName_Click(object sender, RoutedEventArgs e) =>
            GroupList(GroupOption.ByName);

        private void MenuFlyoutGroupByCompany_Click(object sender, RoutedEventArgs e) =>
            GroupList(GroupOption.ByCompany);

        #endregion

        /// <summary>
        /// Sorts a list of customers by last name or company.
        /// </summary>
        private void SortList(SortOption option)
        {
            CollectionViewSource cvs = Resources["CustomersCVS"] as CollectionViewSource;
            if (cvs != null)
            {
                cvs.IsSourceGrouped = false;
                switch (option)
                {
                    case SortOption.ByName:
                        cvs.Source = ViewModel.Customers.OrderByDescending(customer => customer.LastName);
                        break;
                    case SortOption.ByCompany:
                        cvs.Source = ViewModel.Customers.OrderByDescending(customer => customer.Company);
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Groups a list of customers by last name or company.
        /// </summary>
        private void GroupList(GroupOption option)
        {
            CollectionViewSource cvs = Resources["CustomersCVS"] as CollectionViewSource;
            if (cvs != null)
            {
                cvs.IsSourceGrouped = true;
                switch (option)
                {
                    case GroupOption.ByName:
                        cvs.Source = ViewModel.Customers.GroupBy(customer => customer.LastName);
                        break;
                    case GroupOption.ByCompany:
                        cvs.Source = ViewModel.Customers.GroupBy(customer => customer.Company);
                        break;
                    default:
                        cvs.IsSourceGrouped = false;
                        break;
                }
            }
        }

        /// <summary>
        /// Workaround to support earlier versions of Windows.
        /// </summary>
        private void CommandBar_Loaded(object sender, RoutedEventArgs e)
        {
            if (ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Controls.CommandBar", "DefaultLabelPosition"))
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
            else
            {
                if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
                {
                    var lastCommand = mainCommandBar.PrimaryCommands.Last();

                    mainCommandBar.PrimaryCommands.Remove(lastCommand);
                    mainCommandBar.SecondaryCommands.Add(lastCommand);
                }
            }
        }

        /// <summary>
        /// Keyboard control for selecting a customer and displaying details
        /// </summary>
        private void ListView_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter || e.Key == Windows.System.VirtualKey.Space)
            {
                GoToDetailsPage(ViewModel.SelectedCustomer);
            }
        }

        /// <summary>
        /// Double click control for selecting a customer and displaying details.
        /// </summary>
        private void ListViewItem_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var customer = (sender as FrameworkElement).DataContext as CustomerViewModel;
            GoToDetailsPage(customer);
        }

        /// <summary>
        /// Menu flyout click control for selecting a customer and displaying details.
        /// </summary>
        private void ViewDetails_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem)
            {
                var customer = (sender as FrameworkElement).DataContext as CustomerViewModel;
                GoToDetailsPage(customer);
            }
            else
            {
                GoToDetailsPage(ViewModel.SelectedCustomer);
            }
        }

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
            Frame.Navigate(typeof(CustomerDetailPage), customer,
                new DrillInNavigationTransitionInfo());

        /// <summary>
        /// Navigates to the order detail page for the provided customer.
        /// </summary>
        private void GoToOrderPage(CustomerViewModel customer) =>
            Frame.Navigate(typeof(OrderDetailPage), customer.Model);
    }
}
