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

//-----------------------------------------------------------------------------------
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
using System.Windows.Input;
using Windows.UI.Xaml.Controls;

namespace ContosoApp.Commands
{
    /// <summary>
    /// A generic relay command that allows binding commands from the UI.
    /// </summary>
    /// <typeparam name="T">The target type.</typeparam>
    public class RelayCommand<T> : BaseCommand, ICommand
    {
        private readonly Action<T> _action;

        /// <summary>
        /// Creates a new command that can always execute.
        /// </summary>
        /// <param name="action">The execution logic.</param>
        public RelayCommand(Action<T> action)
            : this(action, null)
        {
        }

        /// <summary>
        /// Creates a new command.
        /// </summary>
        /// <param name="action">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        public RelayCommand(Action<T> action, Func<bool> canExecute)
            : base(canExecute)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            _action = action;
        }

        /// <summary>
        /// Executes the <see cref="RelayCommand" /> on the current command target.
        /// </summary>
        /// <param name="parameter">
        /// Data used by the command. If the command does not require data to be passed, this object can be set to null.
        /// </param>
        public void Execute(object parameter)
        {
            if (parameter is ItemClickEventArgs)
            {
                parameter = ((ItemClickEventArgs)parameter).ClickedItem;
            }

            if (parameter is T)
            {
                _action((T)parameter);
            }
        }
    }

    /// <summary>
    /// A command whose sole purpose is to relay its functionality
    /// to other objects by invoking delegates.
    /// The default return value for the CanExecute method is 'true'.
    /// </summary>
    public class RelayCommand : BaseCommand, ICommand
    {
        private readonly Action _action;

        /// <summary>
        /// Creates a new command that can always execute.
        /// </summary>
        /// <param name="action">The execution logic.</param>
        public RelayCommand(Action action)
            : this(action, null)
        {
        }

        /// <summary>
        /// Creates a new command.
        /// </summary>
        /// <param name="action">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        public RelayCommand(Action action, Func<bool> canExecute)
            : base(canExecute)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            _action = action;
        }

        /// <summary>
        /// Executes the <see cref="RelayCommand" /> on the current command target.
        /// </summary>
        /// <param name="parameter">
        /// Data used by the command. If the command does not require data to be passed, this object can be set to null.
        /// </param>
        public void Execute(object parameter)
        {
            _action();
        }
    }
}