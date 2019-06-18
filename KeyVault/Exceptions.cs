using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyVault
{
    [Serializable]
    class DBInsertServiceException : Exception
    {
        public DBInsertServiceException()
        {
        }

        public DBInsertServiceException(string msg)
            : base(String.Format("Services might already exist.\n{0}", msg))
        {
        }
    }

    [Serializable]
    class DBInsertKeypairException : Exception
    {
        public DBInsertKeypairException()
        {
        }

        public DBInsertKeypairException(string msg)
            : base(String.Format("Failed to add key.\n{0}", msg))
        {
        }
    }
}
