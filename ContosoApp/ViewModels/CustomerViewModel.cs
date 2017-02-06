using ContosoModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telerik.Core;

namespace ContosoApp.ViewModels
{
    public class CustomerViewModel : ValidateViewModelBase
    {
        public CustomerViewModel(Customer model)
        {
            Model = model ?? new Customer();
        }

        internal Customer Model { get; set; }

        internal bool IsModified { get; set; }

        public string FirstName
        {
            get { return Model.FirstName; }
            set
            {
                if (value != Model.FirstName)
                {
                    Model.FirstName = value;
                    IsModified = true; 
                }
            }
        }

        public string LastName
        {
            get { return Model.LastName; }
            set
            {
                if (value != Model.LastName)
                {
                    Model.LastName = value;
                    IsModified = true;
                }
            }
        }

        public string Address
        {
            get { return Model.Address; }
            set
            {
                if (value != Model.Address)
                {
                    Model.Address = value;
                    IsModified = true;
                }
            }
        }

        public string Company
        {
            get { return Model.Company; }
            set
            {
                if (value != Model.Company)
                {
                    Model.Company = value;
                    IsModified = true;
                }
            }
        }

        public string Phone
        {
            get { return Model.Phone; }
            set
            {
                if (value != Model.Phone)
                {
                    var validPhone = new Regex(@"^\(?([0-9]{3})\)?[-. ]?" +
                        @"([0-9]{3})[-. ]?([0-9]{4})$");
                    if (validPhone.IsMatch(value))
                    {
                        RemoveErrors(nameof(Phone));
                        Model.Phone = value;
                    }
                    else
                    {
                        AddError(nameof(Phone), "Phone number is not valid.");
                    }
                    IsModified = true; 
                }
            }
        }

        public string Email
        {
            get { return Model.Email; }
            set
            {
                if (value != Model.Email)
                {
                    Model.Email = value;
                    var validEmail = new Regex(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
                        @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
                        @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");

                    if (validEmail.IsMatch(value))
                    {
                        RemoveErrors(nameof(Email));
                        Model.Email = value;
                    }
                    else
                    {
                        AddError(nameof(Email), "Email is not valid.");
                    }
                    IsModified = true; 
                }
            }
        }
    }
}