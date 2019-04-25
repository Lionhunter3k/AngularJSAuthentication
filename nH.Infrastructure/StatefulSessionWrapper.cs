using NHibernate;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace nH.Infrastructure
{
    public class StatefulSessionWrapper : ISessionWrapper
    {
        public StatefulSessionWrapper(ISession session)
        {
            this._session = session;
        }

        private readonly ISession _session;

        #region ISessionWrapper Members

        public ITransaction BeginTransaction()
        {
            return _session.BeginTransaction();
        }

        public ITransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            return _session.BeginTransaction(isolationLevel);
        }

        public ITransaction Transaction { get { return _session.Transaction; } }

        public bool IsConnected
        {
            get { return _session.IsConnected; }
        }

        public bool IsOpen
        {
            get { return _session.IsOpen; }
        }

        #endregion
    }
}
