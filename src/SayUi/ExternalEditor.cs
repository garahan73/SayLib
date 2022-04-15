using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace System.Windows.Forms
{
    public class ExternalEditor
    {
        public static async Task ShowAsync(string? path, string? errorMessage = null, bool includePathInContents = false)
        {
            if (path == null) return;

            try
            {
                if (SayUI.UseExternalEditor)
                {
                    try
                    {
                        var startInfo = new ProcessStartInfo
                        {
                            FileName = SayUI.ExternalEditorPath,
                            Arguments = $"\"{path}\"",
                            WindowStyle = ProcessWindowStyle.Hidden,
                            UseShellExecute = true,
                        };
                        Process.Start(startInfo);
                        //Process.Start(SayUI.ExternalEditorPath, $"\"{path}\"");
                    }
                    catch
                    {
                        MessageBox.Show($"Editor path: \"{SayUI.ExternalEditorPath}\"", "External editor Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        await Notepad.ShowFile(path, includePathInContents);
                    }
                }
                else
                {
                    await Notepad.ShowFile(path, includePathInContents);
                }
            }
            catch (Exception ex)
            {
                errorMessage ??= "Failed to launch external editor";
                Popup.Error(errorMessage, ex);
            }
        }
    }
}
