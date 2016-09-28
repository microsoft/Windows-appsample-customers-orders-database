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
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace ContosoService
{
    /// <summary>
    /// Entity Framework DbContext for Contoso.
    /// </summary>
    public class ContosoContext : DbContext
    {
        private const string CsDev = @"<TODO: Insert connection string>";

        private const string CsProd = @"<TODO: Insert connection string>"; 

        /// <summary>
        /// Creates a new Contoso DbContext.
        /// </summary>
        public ContosoContext() : base(CsProd)
        {
            AppDomain.CurrentDomain.SetData("DataDirectory",
                System.IO.Directory.GetCurrentDirectory());
            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<ContosoContext>());
            Database.CommandTimeout = 90;
        }

        /// <summary>
        /// Gets the customers DbSet.
        /// </summary>
        public DbSet<Customer> Customers { get; set; }

        /// <summary>
        /// Gets the orders DbSet.
        /// </summary>
        public DbSet<Order> Orders { get; set; }

        /// <summary>
        /// Gets the products DbSet.
        /// </summary>
        public DbSet<Product> Products { get; set; }
    }
}