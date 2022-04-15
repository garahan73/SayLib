using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace System.Windows.Forms
{
    public static class SayUI
    {        
        public static Form? MainWindow { get; set; }

        public static bool UseExternalEditor { get; set; } = false;
        public static string? ExternalEditorPath { get; set; }

    }
}
