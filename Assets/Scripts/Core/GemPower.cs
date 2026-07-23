using UnityEngine;

namespace MatchGems.Core
{
    /// <summary>
    /// 寶石特殊能力分類
    /// </summary>
    public enum GemPower
    {
        /// <summary>
        /// 普通
        /// </summary>
        Normal,
        /// <summary>
        /// 橫排清除
        /// </summary>
        HLine,
        /// <summary>
        /// 直排清除
        /// </summary>
        VLine,
        /// <summary>
        /// TL炸彈
        /// </summary>
        Bomb,
        /// <summary>
        /// 萬能彩虹
        /// </summary>
        Rainbow
    }
}

