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

using Newtonsoft.Json;
using System.Collections.Generic;
using System;

namespace ContosoModels
{
    /// <summary>
    /// Represents a customer.
    /// </summary>
    public class Customer : DbObject, IEquatable<Customer>
    {
        /// <summary>
        /// Gets or sets the customer's first name.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the customer's last name.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the customer's company.
        /// </summary>
        public string Company { get; set; }

        /// <summary>
        /// Gets or sets the customer's email.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the customer's phone number.
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// Gets or sets the customer's address. 
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// A list of the customer's orders. 
        /// </summary>
        [JsonIgnore]
        public virtual List<Order> Orders { get; set; }

        /// <summary>
        /// Returns the customer's name.
        /// </summary>
        public override string ToString() => $"{FirstName} {LastName}";

        public bool Equals(Customer other)
        {
            return 
                this.FirstName == other.FirstName &&
                this.LastName == other.LastName &&
                this.Company == other.Company &&
                this.Email == other.Email &&
                this.Phone == other.Phone &&
                this.Address == other.Address;
        }

        public Customer Clone() => (Customer)MemberwiseClone(); 
    }
}
