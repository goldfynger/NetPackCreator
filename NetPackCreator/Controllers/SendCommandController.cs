#define SENDCOMMANDCONTROLLER_SHOW_COMMAND_SERVED_EVENT_MSG

using System;
using System.Diagnostics;
using System.Windows.Controls;

namespace NetPackCreator.Controllers
{
    /// <summary></summary>
    public sealed class SendCommandController : ICommandController
    {
        /// <summary></summary>
        public event EventHandler CommandServed;

                /// <summary></summary>
        /// <param name="comboBox"></param>
        /// <param name="helper"></param>
        public SendCommandController(Button button)
        {
            if (button == null) throw new ArgumentNullException("button");

            button.Click += (sender, e) => this.OnCommandServed();
        }

        /// <summary></summary>
        private void OnCommandServed()
        {

#if SENDCOMMANDCONTROLLER_SHOW_COMMAND_SERVED_EVENT_MSG

            Debug.Fail("SendCommandController.OnCommandServed");

#endif

            var handler = this.CommandServed;
            if (handler != null) handler(this, EventArgs.Empty);
        }
    }
}