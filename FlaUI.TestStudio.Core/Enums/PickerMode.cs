using System;
using System.Collections.Generic;
using System.Text;

namespace FlaUI.TestStudio.Core.Enums
{
    public enum PickerMode
    {
        None,
        SingleSelect,   // "Element auswählen": F8 markiert nur, fügt keinen Step hinzu
        Recording       // "Record": F8 markiert UND fügt einen Click-Step hinzu
    }
}
