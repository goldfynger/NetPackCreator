using System;
using System.Net;

using NetPackCreator.Controls;

namespace NetPackCreator.Controllers
{
    /// <summary></summary>
    internal sealed class DestinationIpAddressController : ColoredTextBoxController<IPAddress>, IDestinationIpAddressController
    {
        /// <summary></summary>
        /// <param name="textBox"></param>
        /// <param name="helper"></param>
        /// <param name="parseFunc"></param>
        /// <param name="validationFunc"></param>
        /// <param name="toStringFunc"></param>
        public DestinationIpAddressController(ColoredTextBox textBox, TextBoxChangeCanceledHelper helper, Func<string, IPAddress> parseFunc, Func<string, bool> validationFunc, Func<IPAddress, string> toStringFunc) :
            base(textBox, helper, parseFunc, validationFunc, toStringFunc) { }

        /// <summary></summary>
        /// <param name="address"></param>
        public void SetAddress(IPAddress address)
        {
            base.SetValue(address);
        }

        /// <summary></summary>
        public void ClearAddress()
        {
            base.ClearValue();
        }

        /// <summary></summary>
        public IPAddress CurrentValue
        {
            get { return base.Value; }
        }

        /// <summary></summary>
        public bool HasValue
        {
            get { return base.Value != null; }
        }
    }
}