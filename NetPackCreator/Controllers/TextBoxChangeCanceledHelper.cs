#define TEXTBOXCHANGECANCELEDHELPER_SHOW_CHANGE_CANCELED_EVENT_MSG

using System;
using System.Diagnostics;

namespace NetPackCreator.Controllers
{
    /// <summary>В других классах (контроллерах) почему-то не работают события Masking.ChangeCanceled и IntegerRange.ChangeCanceled, используется этот класс.</summary>
    internal sealed class TextBoxChangeCanceledHelper
    {
        /// <summary></summary>
        public event EventHandler ChangeCanceled;

        /// <summary></summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnChangeCanceled(object sender)
        {

#if TEXTBOXCHANGECANCELEDHELPER_SHOW_CHANGE_CANCELED_EVENT_MSG

            Debug.Fail("ColoredTextBoxChangeCanceledHelper.OnChangeChanged");

#endif

            var handler = this.ChangeCanceled;
            if (handler != null) handler(sender, EventArgs.Empty);
        }
    }
}