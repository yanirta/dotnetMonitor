using System;
using System.Collections.Generic;
using System.Data;

namespace Monitor.Interfaces
{
    public interface ISiteDataProvider
    {
        DataRow nextRow();
        string nextField(int columnNumber);
    }
}