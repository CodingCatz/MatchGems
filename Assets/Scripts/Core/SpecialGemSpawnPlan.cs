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
        #region 屬性參數
        /// <summary>
        /// 紀錄多組訂單
        /// </summary>
        private readonly List<SpecialGemSpawnInfo> _spawns = new List<SpecialGemSpawnInfo>();
        #endregion 屬性參數

        #region 公開屬性
        public int Count => _spawns.Count;
        /// <summary>
        /// 是否有任何特殊組成單
        /// </summary>
        public bool HasSpawns => Count > 0;
        public SpecialGemSpawnInfo this[int index] => _spawns[index];
        #endregion 公開屬性

        #region 公開方法
        /// <summary>
        /// 只收有特殊石產生的資訊
        /// </summary>
        /// <param name="spawnInfo"></param>
        public void Add(SpecialGemSpawnInfo spawnInfo)
        {
            if (spawnInfo.HasSpecialGem)
            {
                _spawns.Add(spawnInfo);
            }
        }
        /// <summary>
        /// 確認是否要保留該為置給特殊石
        /// </summary>
        /// <param name="coord"></param>
        /// <returns></returns>
        public bool Contains(CellCoord coord)
        {
            for (int i = 0; i < Count; i++)
            {
                CellCoord spawnCoord = _spawns[i].SpawnCoord;
                if (spawnCoord.X == coord.X && spawnCoord.Y == coord.Y) return true;
            }
            return false;
        }
        #endregion 公開方法
    }
}
