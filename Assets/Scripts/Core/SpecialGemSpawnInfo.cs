using UnityEngine;

namespace MatchGems.Core
{
    public class SpecialGemSpawnInfo
    {
        public static SpecialGemSpawnInfo None => new SpecialGemSpawnInfo();
        /// <summary>
        /// 是否產生特殊寶石
        /// </summary>
        public bool HasSpecialGem { get; }
        /// <summary>
        /// 產生的特殊寶石資料
        /// </summary>
        public GemData GemData { get; }
        /// <summary>
        /// 特殊寶石的產生定位
        /// </summary>
        public CellCoord SpawnCoord { get; }
    }
}
