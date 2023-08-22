using Game.Tools;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Entities;

namespace QCommonLib
{
    public class QCommon
    {
        public static ToolBaseSystem ActiveTool { get => QCommon.ToolSystem.activeTool; }

        public static ToolSystem ToolSystem
        {
            get
            {
                if (_toolSystem == null)
                {
                    _toolSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<ToolSystem>();
                }
                return _toolSystem;
            }
        }
        private static ToolSystem _toolSystem = null;

        public static DefaultToolSystem DefaultTool
        {
            get
            {
                if (_defaultTool == null)
                {
                    _defaultTool = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<DefaultToolSystem>();
                }
                return _defaultTool;
            }
        }
        private static DefaultToolSystem _defaultTool = null;
    }
}
