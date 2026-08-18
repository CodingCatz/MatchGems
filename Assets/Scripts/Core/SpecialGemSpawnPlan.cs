using System.Collections.Generic;

namespace MatchGems.Core
{
    /// <summary>
    /// 保存同一拍要生成的全部特殊石。
    /// 一筆 SpecialGemSpawnInfo 代表一顆；Plan 代表這一拍的 0～N 顆。
    /// </summary>
    public sealed class SpecialGemSpawnPlan
    {
        private readonly List<SpecialGemSpawnInfo> _spawns =
            new List<SpecialGemSpawnInfo>();

        public int Count => _spawns.Count;
        public bool HasSpawns => Count > 0;
        public SpecialGemSpawnInfo this[int index] => _spawns[index];

        /// <summary>只收下有效的特殊石生成結果。</summary>
        public void Add(SpecialGemSpawnInfo spawn)
        {
            if (spawn.HasSpecialGem)
            {
                _spawns.Add(spawn);
            }
        }

        /// <summary>確認某格是否要保留給新生特殊石。</summary>
        public bool Contains(CellCoord coord)
        {
            for (int i = 0; i < _spawns.Count; i++)
            {
                CellCoord spawnCoord = _spawns[i].SpawnCoord;
                if (spawnCoord.X == coord.X && spawnCoord.Y == coord.Y)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
