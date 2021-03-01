using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OD_Finesse_Bridge.model
{
    public class Transaction
    {
        public string nor { get; set; }
        public string tct { get; set; }
        public string fec { get; set; }
        public string nau { get; set; }
        public string pro { get; set; }
        public string mnt { get; set; }
        public string hor { get; set; }
        public string trx { get; set; }
        public MensajeNegocio mne { get; set; }
    }

    public class MensajeNegocio {
        public string msg { get; set; }
        public int ERR_CODE { get; set; }
        public string ERR_DESC { get; set; }
    }

}
