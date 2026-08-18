using System;
using System.Collections.Generic;
using UnityEngine;

namespace MatchGems.Core
{
    /// <summary>
    /// 炸板連鎖反應資料
    /// </summary>
    public class DetonationChain
    {
        #region 基本參數
        private readonly BoardModel _board;
        /// <summary>
        /// 不加入重複數值(哈希)
        /// </summary>
        private readonly HashSet<int> _seen = new HashSet<int>();
        private readonly List<DetonationFuse> _fuses = new List<DetonationFuse>();
        private readonly List<DetonationFuse> _list = new List<DetonationFuse>();
        #endregion 基本參數

        #region 公開屬性
        public BoardModel Board => _board;
        public bool HasFuses => _fuses.Count > 0;
        #endregion 公開屬性

        #region 建構
        /// <summary>
        /// 建立一條引爆連鎖
        /// </summary>
        public DetonationChain(BoardModel board, SpecialGemSpawnPlan spawnPlan)
        {
            _board = board;
            for (int i = 0; i < spawnPlan.Count; i++)
            {
                _seen.Add(ToKey(spawnPlan[i].SpawnCoord));
            }
        }
        #endregion 建構

        #region 公開方法
        /// <summary>
        /// 登記能連鎖的特殊寶石，成為為下一次消除的觸發條件
        /// </summary>
        public bool TryRegister(CellCoord coord)
        {
            if (!_board.HasGem(coord) || !_seen.Add(ToKey(coord))) return false;
            //重複不能登記

            GemData gemData = _board.GetGem(coord);
            if (gemData != null && gemData.IsSpecial) 
            {//插上引信
                _fuses.Add(new DetonationFuse(coord, gemData));
            }
            return true;
        }

        public IReadOnlyList<DetonationFuse> TakeFuses()
        {
            _list.Clear();//
            _list.AddRange(_fuses);//資料合併
            _fuses.Clear();
            return _list;
        }
        #endregion 公開方法

        #region 私有方法
        /// <summary>
        /// 二維座標扁平化成序列號
        /// </summary>
        /// <param name="coord"></param>
        /// <returns></returns>
        private int ToKey(CellCoord coord)
        {
            return coord.Y * _board.Width + coord.X;
        }
        #endregion 私有方法
    }
}
