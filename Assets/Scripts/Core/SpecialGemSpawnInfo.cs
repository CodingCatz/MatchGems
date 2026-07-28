using UnityEngine;

namespace MatchGems.Core
{
    /// <summary>
    /// 寶石生產資訊(工廠模式訂單)
    /// </summary>
    public readonly struct SpecialGemSpawnInfo
    {
        #region 公開屬性
        public static SpecialGemSpawnInfo None => new SpecialGemSpawnInfo(false, null, new CellCoord(0,0));
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
        #endregion 公開屬性

        /// <summary>
        /// [建構式]寶石產生資訊初始
        /// </summary>
        public SpecialGemSpawnInfo(bool hasSpecialGem, GemData gemData, CellCoord spawnCoord)
        {
            HasSpecialGem = hasSpecialGem;
            GemData = gemData;
            SpawnCoord = spawnCoord;
        }
    }
}
