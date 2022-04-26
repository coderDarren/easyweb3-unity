using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public Vector3 OpenPos;
    private bool m_Open;

    private void Update() {
        if (m_Open) {
            transform.localPosition = Vector3.Lerp(transform.localPosition, OpenPos, 5 * Time.deltaTime);
        }
    }

    public void Open() {
        m_Open = true;
    }
}
