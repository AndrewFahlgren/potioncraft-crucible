// <copyright file="IngredientsListResolveAtlasEvent.cs" company="RoboPhredDev">
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

namespace RoboPhredDev.PotionCraft.Crucible.GameAPI.GameHooks
{
    using System;
    using System.Collections.Generic;
    using System.Reflection.Emit;
    using Books.RecipeBook;
    using HarmonyLib;
    using UnityEngine;

    /// <summary>
    /// Provides an event to override the atlas name of items in the ingredients list.
    /// </summary>
    public static class IngredientsListResolveAtlasEvent
    {
        private static bool patchApplied = false;
        private static EventHandler<ScriptableObjectAtlasRequestEventArgs> onAtlasRequest;

        /// <summary>
        /// Raised when the atlas of a given item is being resolved.
        /// </summary>
        public static event EventHandler<ScriptableObjectAtlasRequestEventArgs> OnAtlasRequest
        {
            add
            {
                EnsurePatch();
                onAtlasRequest += value;
            }

            remove
            {
                onAtlasRequest -= value;
            }
        }

        private static void EnsurePatch()
        {
            if (patchApplied)
            {
                return;
            }

            var panelUpdateIngredientsMethod = AccessTools.Method(typeof(PotionCraftPanel.PotionCraftPanel), "UpdateIngredientsList");
            if (panelUpdateIngredientsMethod == null)
            {
                Debug.Log("[RoboPhredDev.PotionCraft.Crucible] Failed to locate potion craft panel ingredients list update function!");
            }
            else
            {
                var transpiler = AccessTools.Method(typeof(IngredientsListResolveAtlasEvent), nameof(TranspilePotionCraftPanelUpdateIngredientsList));
                HarmonyInstance.Instance.Patch(panelUpdateIngredientsMethod, transpiler: new HarmonyMethod(transpiler));
            }

            var recipeUpdateIngredientsMethod = AccessTools.Method(typeof(RecipeBookLeftPageContent), "UpdateIngredientsList");
            if (recipeUpdateIngredientsMethod == null)
            {
                Debug.Log("[RoboPhredDev.PotionCraft.Crucible] Failed to locate recipe book ingredients list update function!");
            }
            else
            {
                var transpiler = AccessTools.Method(typeof(IngredientsListResolveAtlasEvent), nameof(TranspileRecipeBookLeftPageContentUpdateIngredientsList));
                HarmonyInstance.Instance.Patch(recipeUpdateIngredientsMethod, transpiler: new HarmonyMethod(transpiler));
            }

            patchApplied = true;
        }

        private static string GetAtlasForUsedComponentIndex(int usedComponentIndex)
        {
            var component = Managers.Potion.usedComponents[usedComponentIndex];
            return GetAtlasForUsedComponent(component);
        }

        private static string GetAtlasForUsedComponent(Potion.UsedComponent component)
        {
            var e = new ScriptableObjectAtlasRequestEventArgs(component.componentObject);
            onAtlasRequest?.Invoke(null, e);
            return e.AtlasResult ?? Managers.TmpAtlas.settings.IngredientsAtlasName;
        }

        private static IEnumerable<CodeInstruction> TranspilePotionCraftPanelUpdateIngredientsList(IEnumerable<CodeInstruction> instructions)
        {
            var getAtlasForUsedComponentIndexMethod = AccessTools.Method(typeof(IngredientsListResolveAtlasEvent), nameof(GetAtlasForUsedComponentIndex));
            var found = false;
            foreach (var instruction in instructions)
            {
                // TODO: We shouldn't trust that this code always uses loc.0 for this value...
                // Probably should look for ldstr "<voffset=0.1em><size=270%><sprite=\"", then skip over its stelem.ref, the array dup, and lcd.i4.3 which prepares the next array index.
                if (!found && instruction.opcode == OpCodes.Ldloc_0)
                {
                    found = true;
                    yield return new CodeInstruction(OpCodes.Ldloc_3); // index
                    yield return new CodeInstruction(OpCodes.Call, getAtlasForUsedComponentIndexMethod);
                }
                else
                {
                    yield return instruction;
                }
            }

            if (!found)
            {
                Debug.Log("[RoboPhredDev.PotionCraft.Crucible] Failed to inject atlas replacement for PotionCraftPanel!");
            }
        }

        private static IEnumerable<CodeInstruction> TranspileRecipeBookLeftPageContentUpdateIngredientsList(IEnumerable<CodeInstruction> instructions)
        {
            var getAtlasForUsedComponentMethod = AccessTools.Method(typeof(IngredientsListResolveAtlasEvent), nameof(GetAtlasForUsedComponent));
            var found = false;
            foreach (var instruction in instructions)
            {
                if (!found && instruction.opcode == OpCodes.Ldloc_S && instruction.operand is LocalBuilder localBuilder && localBuilder.LocalIndex == 5 && localBuilder.LocalType == typeof(Potion.UsedComponent))
                {
                    // We should now be right before the if statement checking if the current potion is in stock
                    found = true;

                    yield return new CodeInstruction(OpCodes.Ldloc_S, 5); // currentPotion
                    yield return new CodeInstruction(OpCodes.Call, getAtlasForUsedComponentMethod);

                    // Store the result into ingredientsAtlasName so it will be used in one of the two branching string constructions.
                    yield return new CodeInstruction(OpCodes.Stloc_0);

                    // Return the first part of the if-check and continue as normal.
                    yield return instruction;
                }
                else
                {
                    yield return instruction;
                }
            }

            if (!found)
            {
                Debug.Log("[RoboPhredDev.PotionCraft.Crucible] Failed to inject atlas replacement for RecipeBookLeftPageContent!");
            }
        }
    }
}
