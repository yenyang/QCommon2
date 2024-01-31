using Unity.Entities;

namespace QCommonLib
{
    public class QTypes
    {
        public enum Types
        {
            None,
            Building,
            Plant,
            NetSegment,
            NetNode,
            Roundabout,
            SubEntity,
            Other,
            Invalid,
        }

        public static Types GetEntityType(Entity e)
        {
            EntityManager EM = World.DefaultGameObjectInjectionWorld.EntityManager;

            if (e.Equals(Entity.Null) ||
                EM.HasComponent<Game.Common.Deleted>(e) ||
                EM.HasComponent<Game.Common.Terrain>(e))
            {
                return Types.Invalid;
            }

            if (EM.HasComponent<Game.Objects.Plant>(e))
            {
                return Types.Plant;
            }
            else if (EM.HasComponent<Game.Buildings.Building>(e))
            {
                return Types.Building;
            }
            else if (EM.HasComponent<Game.Net.Edge>(e))
            {
                return Types.NetSegment;
            }
            else if (EM.HasComponent<Game.Net.Node>(e))
            {
                return Types.NetNode;
            }
            else if (EM.HasComponent<Game.Objects.Static>(e) && EM.HasComponent<Game.Objects.NetObject>(e))
            {
                return Types.Roundabout;
            }
            else
            {
                return Types.Other;
            }
        }
    }
}
