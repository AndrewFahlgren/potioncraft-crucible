// <copyright file="SpriteAssetRequestEventArgs.cs" company="RoboPhredDev">
// This file is part of the Crucible Modding Framework.
//
// Crucible is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// Crucible is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
// You should have received a copy of the GNU Lesser General Public License
// along with Crucible; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
// </copyright>

namespace RoboPhredDev.PotionCraft.Crucible.GameAPI.GameHooks
{
    using System;
    using TMPro;

    /// <summary>
    /// Arguments for events that request a <see cref="TMP_SpriteAsset"/> by name.
    /// </summary>
    public class SpriteAssetRequestEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpriteAssetRequestEventArgs"/> class.
        /// </summary>
        /// <param name="hashCode">The hash code of the asset name.</param>
        /// <param name="assetName">The asset name.</param>
        public SpriteAssetRequestEventArgs(int hashCode, string assetName)
        {
            this.AssetHashCode = hashCode;
            this.AssetName = assetName;
        }

        /// <summary>
        /// Gets the hash code of the asset name.
        /// </summary>
        public int AssetHashCode { get; }

        /// <summary>
        /// Gets the asset name being requested.
        /// </summary>
        public string AssetName { get; }

        /// <summary>
        /// Gets or sets the sprite asset used to respond to this request.
        /// </summary>
        public TMP_SpriteAsset SpriteAsset { get; set; }
    }
}
