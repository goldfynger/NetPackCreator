#define COMBOBOXSELECTIONCHANGEDHELPER_SHOW_SELECTION_CHANGED_EVENT_MSG

using System.Diagnostics;
using System.Windows.Controls;

namespace NetPackCreator.Controllers
{
    /// <summary>В других классах (контроллерах) почему-то не работает событие ComboBox.SelectionChanged, используется этот класс.</summary>
    internal sealed class ComboBoxSelectionChangedHelper
    {
        /// <summary></summary>
        public event SelectionChangedEventHandler SelectionChanged;

        /// <summary></summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

#if COMBOBOXSELECTIONCHANGEDHELPER_SHOW_SELECTION_CHANGED_EVENT_MSG

            Debug.Fail("ComboBoxSelectionChangedHelper.OnSelectionChanged");

#endif

            var handler = this.SelectionChanged;
            if (handler != null) handler(sender, e);
        }
    }
}