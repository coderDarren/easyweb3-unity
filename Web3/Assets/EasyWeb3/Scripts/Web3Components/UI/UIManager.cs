
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public CanvasGroup Menu;
    private bool m_Toggled = true;

    private void Update() {
        if (Input.GetKeyUp(KeyCode.H)) {
            m_Toggled = !m_Toggled;
            Menu.alpha = m_Toggled ? 1 : 0;
        }
    }   
}
