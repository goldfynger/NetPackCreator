using System;
using System.Collections.Generic;
using System.Net;
using System.Windows.Controls;

namespace NetPackCreator.Controllers
{
    /// <summary></summary>
    internal sealed class SourceIpAddressController : ComboBoxController<IPAddress>, ISourceIpAddressController
    {
        /// <summary></summary>
        /// <param name="comboBox"></param>
        /// <param name="helper"></param>
        /// <param name="values"></param>
        /// <param name="toStringFunc"></param>
        public SourceIpAddressController(ComboBox comboBox, ComboBoxSelectionChangedHelper helper, List<IPAddress> values, Func<IPAddress, string> toStringFunc): base(comboBox, helper, values, toStringFunc) { }

        /// <summary></summary>
        /// <param name="list"></param>
        /// <param name="currentIpAddress"></param>
        public void SetAddressList(List<IPAddress> list, IPAddress currentIpAddress)
        {
            base.FillValues(list, currentIpAddress);
        }

        /// <summary></summary>
        public void ClearAddressList()
        {
            base.ClearValues();
        }

        /// <summary></summary>
        public bool HasValue
        {
            get { return base.CurrentValue != null; }
        }
    }
}