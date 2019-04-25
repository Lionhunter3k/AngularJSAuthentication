using NHibernate;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace nH.Infrastructure
{
    public interface ISessionWrapper
    {
        ITransaction BeginTransaction();

        ITransaction BeginTransaction(IsolationLevel isolationLevel);

        ITransaction Transaction { get; }

        bool IsConnected { get; }

        bool IsOpen { get; }
    }
}
