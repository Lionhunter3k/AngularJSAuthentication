using NHibernate;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace nH.Infrastructure
{
    public class StatelessSessionWrapper : ISessionWrapper
    {
        private readonly IStatelessSession _session;

        public StatelessSessionWrapper(IStatelessSession session)
        {
            this._session = session;
        }

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
