﻿using OpenSage.Data.Ini;
using OpenSage.Gui.InGame;

namespace OpenSage.Logic.Object
{
    public sealed class DynamicShroudClearingRangeUpdate : UpdateModule
    {
        private uint _unknown1;
        private uint _unknown2;
        private uint _unknown3;
        private uint _unknown4;
        private uint _unknown5;
        private uint _unknown6;
        private uint _unknown7;
        private float _unknownFloat;

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);

            _unknown1 = reader.ReadUInt32();
            _unknown2 = reader.ReadUInt32();
            _unknown3 = reader.ReadUInt32();
            _unknown4 = reader.ReadUInt32();
            _unknown5 = reader.ReadUInt32();
            _unknown6 = reader.ReadUInt32();
            _unknown7 = reader.ReadUInt32();

            var unknown7_1 = reader.ReadBoolean();
            if (!unknown7_1)
            {
                throw new InvalidStateException();
            }

            reader.SkipUnknownBytes(4);

            var unknown9 = reader.ReadSingle();
            if (unknown9 != 300.0f)
            {
                throw new InvalidStateException();
            }

            _unknownFloat = reader.ReadSingle();
        }
    }

    public sealed class DynamicShroudClearingRangeUpdateModuleData : UpdateModuleData
    {
        internal static DynamicShroudClearingRangeUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<DynamicShroudClearingRangeUpdateModuleData> FieldParseTable = new IniParseTable<DynamicShroudClearingRangeUpdateModuleData>
        {
            { "FinalVision", (parser, x) => x.FinalVision = parser.ParseFloat() },
            { "ChangeInterval", (parser, x) => x.ChangeInterval = parser.ParseInteger() },
            { "ShrinkDelay", (parser, x) => x.ShrinkDelay = parser.ParseInteger() },
            { "ShrinkTime", (parser, x) => x.ShrinkTime = parser.ParseInteger() },
            { "GrowDelay", (parser, x) => x.GrowDelay = parser.ParseInteger() },
            { "GrowTime", (parser, x) => x.GrowTime = parser.ParseInteger() },
            { "GrowInterval", (parser, x) => x.GrowInterval = parser.ParseInteger() },

            { "GridDecalTemplate", (parser, x) => x.GridDecalTemplate = RadiusDecalTemplate.Parse(parser) }
        };

        public float FinalVision { get; private set; }
        public int ChangeInterval { get; private set; }
        public int ShrinkDelay { get; private set; }
        public int ShrinkTime { get; private set; }
        public int GrowDelay { get; private set; }
        public int GrowTime { get; private set; }
        public int GrowInterval { get; private set; }

        public RadiusDecalTemplate GridDecalTemplate { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new DynamicShroudClearingRangeUpdate();
        }
    }
}
