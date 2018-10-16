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

namespace Contoso.App.ViewModels
{
    /// <summary>
    /// Provides a bindable wrapper for the LineItem model class.
    /// </summary>
    public class LineItemViewModel : BindableBase
    {
        /// <summary>
        /// Initializes a new instance of the LineItemWrapper class that wraps a LineItem object.
        /// </summary>
        public LineItemViewModel(LineItem model = null) => Model = model ?? new LineItem();

        /// <summary>
        /// Gets the underlying LineItem object.
        /// </summary>
        public LineItem Model { get; }

        /// <summary>
        /// Gets or sets the product for the line item.
        /// </summary>
        public Product Product
        {
            get => Model.Product;
            set
            {
                if (Model.Product != value)
                {
                    Model.Product = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the product quantity for the line item.
        /// </summary>
        public int Quantity
        {
            get => Model.Quantity;
            set
            {
                if (Model.Quantity != value)
                {
                    Model.Quantity = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}
