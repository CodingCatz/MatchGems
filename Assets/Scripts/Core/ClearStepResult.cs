using System.Collections.Generic;
using UnityEngine;

namespace MatchGems.Core
{
    /// <summary>
    /// 單一COMBO清除結果，消了那些格/顏色
    /// </summary>
    public class ClearStepResult
    {
        #region 公開屬性
        public IReadOnlyList<CellCoord> ClearedCoords { get; private set; }
        public IReadOnlyList<GemType> ClearedGemTypes { get; private set; }
        #endregion 公開屬性

        public ClearStepResult(IReadOnlyList<CellCoord> clearedCoords, IReadOnlyList<GemType> clearedGemTypes) 
        {
            ClearedCoords = clearedCoords;
            ClearedGemTypes = clearedGemTypes;
        }
    }
}
