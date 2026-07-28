using UnityEngine;

namespace MatchGems.Core
{
    /// <summary>
    /// 寶石特殊能力分類
    /// </summary>
    public enum GemPower
    {
        /// <summary>
        /// 普通(3)
        /// </summary>
        Normal,
        /// <summary>
        /// 橫排清除(4)
        /// </summary>
        HLine,
        /// <summary>
        /// 直排清除(4)
        /// </summary>
        VLine,
        /// <summary>
        /// TL炸彈(5)
        /// </summary>
        Bomb,
        /// <summary>
        /// 萬能彩虹(5)
        /// </summary>
        Rainbow
    }
}

