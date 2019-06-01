using UnityEngine;
using UnityEngine.UI;

public class Logger {
    public Logger(Text guiText) { logText = guiText; }

    public void Clear() {
        if (logText != null)
            logText.text = "";
    }

    public static Logger operator +(Logger logger, string msg) {
        logger.ShowGuiMsg(msg);
        Debug.Log(msg);
        return logger;
    }

    public void Warning(string msg) {
        ShowGuiMsg(msg);
        Debug.LogWarning(msg);
    }

    public void Error(string msg) {
        ShowGuiMsg(msg);
        Debug.LogError(msg);
    }

    private void ShowGuiMsg(string msg) {
        if (logText != null) {
            if (logText.text.Length == 0)
                logText.text += "# ";
            logText.text += msg + "\n# ";
        }
    }


    private Text logText = null;
}
