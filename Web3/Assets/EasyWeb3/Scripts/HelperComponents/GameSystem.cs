
using UnityEngine;

/*
 * GameSystem facilities commonalities and generic behaviors..
 * ..we expect all game systems to share
 *
 *  Logging/Debugging
 *  Performance Metrics
 */
public class GameSystem : MonoBehaviour
{
    public bool debug;
    
#region Protected Functions
    protected void Log(string _msg) {
        if (!debug) return;
        Debug.Log("["+this.GetType()+"]: "+_msg);
    }

    protected void LogWarning(string _msg) {
        if (!debug) return;
        Debug.LogWarning("["+this.GetType()+"]: "+_msg);
    }

    protected void LogError(string _msg) {
        if (!debug) return;
        Debug.LogError("["+this.GetType()+"]: "+_msg);
    }
#endregion
}