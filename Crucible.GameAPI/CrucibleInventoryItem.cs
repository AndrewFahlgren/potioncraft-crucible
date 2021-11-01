// <copyright file="CrucibleInventoryItem.cs" company="RoboPhredDev">
// This file is part of the Crucible Modding Framework.
//
// Foobar is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// Foobar is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
// You should have received a copy of the GNU Lesser General Public License
// along with Foobar; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
// </copyright>

namespace RoboPhredDev.PotionCraft.Crucible.GameAPI
{
    using HarmonyLib;
    using UnityEngine;

    /// <summary>
    /// Wraps a <see cref="InventoryItem"/> to provide an api for mod use.
    /// </summary>
    public abstract class CrucibleInventoryItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CrucibleInventoryItem"/> class.
        /// </summary>
        /// <param name="item">The inventory item to wrap.</param>
        internal CrucibleInventoryItem(InventoryItem item)
        {
            this.InventoryItem = item;
        }

        /// <summary>
        /// Gets the ID (internal name) of this inventory item.
        /// </summary>
        public string ID
        {
            get
            {
                return this.InventoryItem.name;
            }
        }

        /// <summary>
        /// Gets or sets the sprite to use for this item in the inventory.
        /// </summary>
        public Sprite InventoryIcon
        {
            get
            {
                return this.InventoryItem.inventoryIconObject;
            }

            set
            {
                this.InventoryItem.inventoryIconObject = value;
            }
        }

        /// <summary>
        /// Gets or sets the base price of this item for buying or selling.
        /// </summary>
        /// <value>The price of the item.</value>
        public float Price
        {
            get
            {
                return Traverse.Create(this.InventoryItem).Field<float>("price").Value;
            }

            set
            {
                Traverse.Create(this.InventoryItem).Field<float>("price").Value = value;
            }
        }

        /// <summary>
        /// Gets the game item being controlled by this api wrapper.
        /// </summary>
        internal InventoryItem InventoryItem { get; }

        /// <summary>
        /// Gives the item to the player.
        /// </summary>
        /// <param name="count">The number of items to give.</param>
        public void GiveToPlayer(int count)
        {
            Managers.Player.inventory.AddItem(this.InventoryItem, count, true, true);
        }
    }
}
