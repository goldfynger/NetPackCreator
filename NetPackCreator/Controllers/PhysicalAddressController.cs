using System;
using System.Net.NetworkInformation;

using NetPackCreator.Controls;

namespace NetPackCreator.Controllers
{
    /// <summary></summary>
    internal sealed class PhysicalAddressController : ColoredTextBoxController<PhysicalAddress>, IPhysicalAddressController
    {
        /// <summary></summary>
        /// <param name="textBox"></param>
        /// <param name="helper"></param>
        /// <param name="parseFunc"></param>
        /// <param name="validationFunc"></param>
        /// <param name="toStringFunc"></param>
        public PhysicalAddressController(ColoredTextBox textBox, TextBoxChangeCanceledHelper helper, Func<string, PhysicalAddress> parseFunc, Func<string, bool> validationFunc, Func<PhysicalAddress, string> toStringFunc) :
            base(textBox, helper, parseFunc, validationFunc, toStringFunc) { }

        /// <summary></summary>
        /// <param name="address"></param>
        public void SetAddress(PhysicalAddress address)
        {
            base.SetValue(address);
        }

        /// <summary></summary>
        public void ClearAddress()
        {
            base.ClearValue();
        }

        /// <summary></summary>
        public PhysicalAddress CurrentValue
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