using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace RoadChangerAddIn
{
    /// <summary>
    /// Add-in module. Required entry point for the .esriAddinX; the framework
    /// instantiates it from the &lt;insertModule&gt; entry in Config.daml.
    /// </summary>
    internal class TransportationModule : Module
    {
        private static TransportationModule _this = null;

        /// <summary>Singleton accessor for this module.</summary>
        public static TransportationModule Current =>
            _this ??= (TransportationModule)FrameworkApplication.FindModule("RoadChangerAddIn_Module");

        #region Overrides
        /// <summary>
        /// Called by the framework when the module is asked to unload. Return true
        /// to allow unload (no unsaved in-memory state to protect here).
        /// </summary>
        protected override bool CanUnload() => true;
        #endregion Overrides
    }
}
