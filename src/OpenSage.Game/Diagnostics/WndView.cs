using System.Numerics;
using ImGuiNET;
using OpenSage.Diagnostics.Util;
using OpenSage.Gui.Wnd.Controls;
using OpenSage.Mathematics;

namespace OpenSage.Diagnostics
{
    internal sealed class WndView : DiagnosticView
    {
        private Control _selectedControl;

        public override string DisplayName { get; } = "WND Windows";

        public WndView(DiagnosticViewContext context)
            : base(context)
        {
            
        }

        protected override void DrawOverride(ref bool isGameViewFocused)
        {
            foreach (var wndWindow in Game.Scene2D.WndWindowManager.WindowStack)
            {
                if (ImGui.TreeNodeEx(wndWindow.DisplayName, ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.OpenOnArrow))
                {
                    DrawControlTreeItemRecursive(wndWindow.Root);
                }
            }
        }

        private void DrawControlTreeItemRecursive(Control control)
        {
            var treeNodeFlags = ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.OpenOnDoubleClick;
            if(control.Controls.Count == 0)
            {
                treeNodeFlags = ImGuiTreeNodeFlags.Leaf;
            }

            if (control == _selectedControl)
            {
                treeNodeFlags |= ImGuiTreeNodeFlags.Selected;
            }

            var opened = ImGui.TreeNodeEx(control.DisplayName, treeNodeFlags);
            ImGuiUtility.DisplayTooltipOnHover(control.DisplayName);

            if (ImGuiNative.igIsItemClicked(0) > 0)
            {
                SelectControl(control);
            }

            if (opened)
            {
                foreach (var child in control.Controls.AsList())
                {
                    DrawControlTreeItemRecursive(child);
                }

                ImGui.TreePop();
            }
        }

        private void SelectControl(Control control)
        {
            if (_selectedControl != null)
            {
                _selectedControl.DebugDrawEnable = false;
                _selectedControl = null;
            }

            _selectedControl = control;
            _selectedControl.DebugDrawEnable = true;
        }
    }
}
