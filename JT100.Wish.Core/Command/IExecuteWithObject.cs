using System;
using System.Collections.Generic;
using System.Text;

namespace JT100.Wish.Core
{
    public interface IExecuteWithObject
    {
        object Target
        {
            get;
        }

        void ExecuteWithObject(object parameter);

        void MarkForDeletion();
    }
}
