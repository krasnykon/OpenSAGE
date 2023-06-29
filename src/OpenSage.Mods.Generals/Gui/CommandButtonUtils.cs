﻿using System;
using System.Collections.Generic;
using System.Linq;
using OpenSage.Content;
using OpenSage.Content.Translation;
using OpenSage.Gui;
using OpenSage.Gui.ControlBar;
using OpenSage.Gui.Wnd;
using OpenSage.Gui.Wnd.Controls;
using OpenSage.Logic;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;

namespace OpenSage.Mods.Generals.Gui
{
    class CommandButtonUtils
    {
        private static NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private static string GetObjectPrerequisites(
            List<ObjectPrerequisiteList> ObjectPrerequisites, List<SciencePrerequisiteList> SciencePrerequisites)
        {
            string result = "";
            string or = " " + "CONTROLBAR:OrRequirement".Translate() + " ";
            foreach (var objPrerequisites in ObjectPrerequisites)
            {
                var strList = objPrerequisites.Select(def => def.Value.DisplayName.Translate());
                string tmp = string.Join(or, strList);
                tmp = "CONTROLBAR:Requirements".TranslateFormatted((object)tmp);
                result += "\n " + tmp;
            }
            foreach (var req in SciencePrerequisites)
            {
                result += "\n " + string.Join(or, req);
            }
            return result;
        }

        public static void SetCommandButton(Button buttonControl, CommandButton commandButton, GeneralsControlBar controlBar)
        {
            buttonControl.BackgroundImage = buttonControl.Window.ImageLoader.CreateFromMappedImageReference(commandButton.ButtonImage);

            buttonControl.DisabledBackgroundImage = buttonControl.BackgroundImage?.WithGrayscale(true);

            buttonControl.BorderColor = GetBorderColor(commandButton.ButtonBorderType, controlBar.Scheme).ToColorRgbaF();
            buttonControl.BorderWidth = 1;

            buttonControl.HoverOverlayImage = controlBar.CommandButtonHover;
            buttonControl.PushedOverlayImage = controlBar.CommandButtonPush;

            var objectDefinition = commandButton.Object?.Value;
            buttonControl.SystemCallback = (control, message, context) =>
            {
                Logger.Debug($"Button callback: {control.Name}, {commandButton.Command}");
                Logger.Debug($"Relevant object: {objectDefinition?.Name}");

                CommandButtonCallback.HandleCommand(context.Game, commandButton, objectDefinition, false);
            };

            buttonControl.InputCallback = (control, message, context) =>
            {
                //TODO: fix the commandbutton description
                var windowManager = buttonControl.Window.Game.Scene2D.WndWindowManager;
                if (message.MessageType == WndWindowMessageType.MouseEnter)
                {
                    string description = "";
                    float cost;
                    string costStr = "";

                    // TODO: set the string correctly
                    switch (commandButton.Command)
                    {
                        case CommandType.DozerConstruct:
                        case CommandType.UnitBuild:
                            cost = commandButton.Object.Value.BuildCost;
                            costStr = "TOOLTIP:Cost".TranslateFormatted(cost);
                            description = commandButton.DescriptLabel.Translate();
                            description += GetObjectPrerequisites(commandButton.ObjectPrerequisites, commandButton.SciencePrerequisites);
                            break;
                        case CommandType.SetRallyPoint:
                        case CommandType.Sell:
                        case CommandType.FireWeapon:
                        case CommandType.SpecialPower:
                            description = commandButton.DescriptLabel.Translate();
                            break;
                        case CommandType.PlayerUpgrade:
                        case CommandType.ObjectUpgrade:
                            cost = commandButton.Upgrade.Value.BuildCost;
                            costStr = "TOOLTIP:Cost".TranslateFormatted(cost);
                            description = commandButton.DescriptLabel.Translate();
                            break;
                        case CommandType.PurchaseScience:
                            cost = commandButton.Science[0].Value.SciencePurchasePointCost;
                            costStr = "TOOLTIP:ScienceCost".TranslateFormatted(cost);
                            description = commandButton.Science[0].Value.Description.Translate();
                            break;
                    }
                    controlBar.ShowDescription(commandButton.TextLabel, costStr, description);
                }
                else if (message.MessageType == WndWindowMessageType.MouseExit)
                {
                    controlBar.HideDescription();
                }

                return control.DefaultInput(control, message, context);
            };
        }

        private static ColorRgba GetBorderColor(CommandButtonBorderType borderType, ControlBarScheme scheme)
        {
            switch (borderType)
            {
                case CommandButtonBorderType.None:
                    return ColorRgba.Transparent;

                case CommandButtonBorderType.Action:
                    return scheme.ButtonBorderActionColor;

                case CommandButtonBorderType.Build:
                    return scheme.ButtonBorderBuildColor;

                case CommandButtonBorderType.Upgrade:
                    return scheme.ButtonBorderUpgradeColor;

                case CommandButtonBorderType.System:
                    return scheme.ButtonBorderSystemColor;

                default:
                    throw new ArgumentOutOfRangeException(nameof(borderType));
            }
        }
    }
}
