using System;
using System.Collections.Generic;
using UnityEngine;

namespace MatchGems.Core
{
    /// <summary>
    /// 同一拍消除能產生特殊石的計畫單
    /// </summary>
    public class SpecialGemSpawnPlan
    {
        #region 公開屬性
        /// <summary>
        /// 無計畫單預設 => 空陣列
        /// </summary>
        public static SpecialGemSpawnPlan None => new SpecialGemSpawnPlan(_emptySpawnInfo);
        /// <summary>
        /// 是否有任何特殊組成單
        /// </summary>
        public bool HasSpawns => _spawns.Length > 0;

        #endregion 公開屬性

        #region 屬性參數
        private static readonly SpecialGemSpawnInfo[] _emptySpawnInfo = Array.Empty<SpecialGemSpawnInfo>();
        private readonly SpecialGemSpawnInfo[] _spawns;
        #endregion 屬性參數

        #region 建構式
        public SpecialGemSpawnPlan(IReadOnlyList<SpecialGemSpawnInfo> spawns)
        {
            //建立多組尺寸
            _spawns = new SpecialGemSpawnInfo[spawns.Count];
            for (int i = 0; i < spawns.Count; i++) 
            {//資料對應
                _spawns[i] = spawns[i];
            }
        }
        #endregion 建構式
    }
}
