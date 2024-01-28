using UnityEngine;

namespace Valve.VR
{
    /// <summary>
    /// Helper for OpenVR/SteamVR-related operations.
    /// </summary>
    public class OpenVRHelper
    {
        /// <summary>
        /// Initialize OpenVR as an overlay. This allows direct communication with SteamVR while maintaining the active OpenXR session. This method is idempotent.
        /// </summary>
        /// <returns>Whether or not initialization was successful.</returns>
        public static bool Initialize()
        {
            if (OpenVR.System != null)
            {
                return true;
            }

            Debug.Log("Initializing OpenVR in Overlay mode");

            EVRInitError error = EVRInitError.None;
            CVRSystem system = OpenVR.Init(ref error, EVRApplicationType.VRApplication_Overlay);

            if (error != EVRInitError.None)
            {
                Debug.LogError("Failed to initialize OpenVR in Overlay mode: " + error);
                return false;
            }

            if (system == null)
            {
                Debug.LogError("Failed to initialize OpenVR in Overlay mode: System is null");
                return false;
            }

            return true;
        }
    }
}
