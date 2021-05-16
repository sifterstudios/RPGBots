using UnityEngine;

namespace MD_Plugin
{
    [AddComponentMenu(MD_Debug.ORGANISATION + MD_Debug.PACKAGENAME + "Tunnel Creator/Tunnel Creator Node UVData")]
    public class MD_TunnelNodeUVData : MonoBehaviour
    {
        public enum _UVMode { uvXY, uvXZ, uvYX, uvYZ, uvZX, uvZY };
        public _UVMode UVMode;
        [Space]
        public Vector2 UvOffset;
        public Vector2 UvTransition;
        [Space]
        public float DebugSize = 0.5f;

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, DebugSize);
        }
    }
}