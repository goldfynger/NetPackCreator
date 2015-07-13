using System;

using NetPackCreator.Controls;

namespace NetPackCreator.Controllers
{
    /// <summary></summary>
    internal sealed class PortController : ColoredTextBoxController<ushort?>, IPortController
    {
        /// <summary></summary>
        /// <param name="textBox"></param>
        /// <param name="helper"></param>
        /// <param name="parseFunc"></param>
        /// <param name="validationFunc"></param>
        /// <param name="toStringFunc"></param>
        public PortController(ColoredTextBox textBox, TextBoxChangeCanceledHelper helper, Func<string, ushort?> parseFunc, Func<string, bool> validationFunc, Func<ushort?, string> toStringFunc) :
            base(textBox, helper, parseFunc, validationFunc, toStringFunc) { }

        /// <summary></summary>
        /// <param name="address"></param>
        public void SetPort(ushort address)
        {
            base.SetValue(address);
        }

        /// <summary></summary>
        public void ClearPort()
        {
            base.ClearValue();
        }

        /// <summary></summary>
        public ushort? CurrentValue
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