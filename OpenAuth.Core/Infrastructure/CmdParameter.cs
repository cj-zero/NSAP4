using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Infrastructure
{
    public class CmdParameter
    {
        public string Sql { get; set; }

        private CommandType _type = CommandType.Text;
        public CommandType Type
        {
            get { return _type; }
            set { _type = value; }
        }

        private IDataParameter[] _params = null;
        public IDataParameter[] Params
        {
            get { return _params; }
            set { _params = value; }
        }

        public bool IsReturn { get; set; }

        public readonly static string ReturnMark = "≮middleware≯";
    }
}
