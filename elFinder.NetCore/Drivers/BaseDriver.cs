using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace elFinder.NetCore.Drivers
{
    /// <summary>
    /// Represents a Base elFinder Driver
    /// </summary>
    public abstract class BaseDriver
    {
        #region Protected

        protected Task<JsonResult> Json(object data)
        {
            return Task.FromResult(new JsonResult(data) { ContentType = "text/html" });
        }

        #endregion Protected

        #region Public

        public string VolumePrefix { get; protected set; }

        public ICollection<RootVolume> Roots { get; protected set; }

        /// <summary>
        /// Adds an object to the end of the roots.
        /// </summary>
        /// <param name="item"></param>
        public void AddRoot(RootVolume item)
        {
            Roots.Add(item);
            item.VolumeId = VolumePrefix + Roots.Count + "_";
        }

        #endregion Public
    }
}