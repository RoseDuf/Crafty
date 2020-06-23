using UnityEngine;
using UnityEngine.UI;

namespace Tenacious.UI
{
    public class ConfirmDialog : Window
    {
        [SerializeField] public RectTransform rootPanelTransform;
        [SerializeField] public Text txtMessage;
        [SerializeField] public Text txtConfirm;
        [SerializeField] public Text txtCancel;

        public const float DEFAULT_WIDTH = 600f;
        public const string DEFAULT_CONFIRM_STRING = "CONFIRM";
        public const string DEFAULT_CANCEL_STRING = "CANCEL";

        public delegate void Callback(ConfirmDialog self, bool result);
        private Callback callback;

        public void SetData(string message, Callback callback, float width = DEFAULT_WIDTH, 
            string strConfirm = DEFAULT_CONFIRM_STRING, string strCancel = DEFAULT_CANCEL_STRING)
        {
            txtMessage.text = message;
            this.callback = callback != null ? callback : (ConfirmDialog s, bool r) => { };
            txtConfirm.text = strConfirm;
            txtCancel.text = strCancel;

            // not working
            rootPanelTransform.rect.Set(rootPanelTransform.rect.x, rootPanelTransform.rect.y, width, rootPanelTransform.rect.height);
        }

        public void ConfirmClick()
        {
            callback(this, true);
        }

        public void CancelClick()
        {
            callback(this, false);
        }
    }
}
