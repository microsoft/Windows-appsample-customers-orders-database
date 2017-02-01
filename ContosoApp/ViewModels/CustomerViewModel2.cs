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
    public class CustomerViewModel2 : ValidateViewModelBase
    {
        // Set as internal so our code can see/modify, but Telerik RadDataGrid
        // won't auto-generate a column for it. 

        internal Customer _model; 
        internal bool _isModified; 

        public CustomerViewModel2()
        { }

        public CustomerViewModel2(Customer model)
        {
            _model = model; 
        }

        public string FirstName
        {
            get { return _model.FirstName; }
            set
            {
                if (value != _model.FirstName)
                {
                    _model.FirstName = value;
                    _isModified = true; 
                }
            }
        }

        public string LastName
        {
            get { return _model.LastName; }
            set
            {
                if (value != _model.LastName)
                {
                    _model.LastName = value;
                    _isModified = true;
                }
            }
        }

        public string Address
        {
            get { return _model.Address; }
            set
            {
                if (value != _model.Address)
                {
                    _model.Address = value;
                    _isModified = true;
                }
            }
        }

        public string Company
        {
            get { return _model.Company; }
            set
            {
                if (value != _model.Company)
                {
                    _model.Company = value;
                    _isModified = true;
                }
            }
        }

        public string Phone
        {
            get { return _model.Phone; }
            set
            {
                if (value != _model.Phone)
                {
                    var validPhone = new Regex(@"^\(?([0-9]{3})\)?[-. ]?" +
                        @"([0-9]{3})[-. ]?([0-9]{4})$");
                    if (validPhone.IsMatch(value))
                    {
                        RemoveErrors(nameof(Phone));
                        _model.Phone = value;
                    }
                    else
                    {
                        AddError(nameof(Phone), "Phone number is not valud.");
                    }
                    _isModified = true; 
                }
            }
        }

        public string Email
        {
            get { return _model.Email; }
            set
            {
                if (value != _model.Email)
                {

                    _model.Email = value;
                    var validEmail = new Regex(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
                        @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
                        @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");

                    if (validEmail.IsMatch(value))
                    {
                        RemoveErrors(nameof(Email));
                        _model.Email = value;
                    }
                    else
                    {
                        AddError(nameof(Email), "Email is not valid.");
                    }
                    _isModified = true; 
                }
            }
        }
    }
}