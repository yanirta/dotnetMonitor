using System;
using System.Collections.Generic;
using System.Data;

namespace Monitor.Interfaces
{
    public interface IUrlProvider
    {
        DataRow nextRow();
        string nextField(int columnNumber);
    }
}