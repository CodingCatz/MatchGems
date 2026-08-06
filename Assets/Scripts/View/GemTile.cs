using UnityEngine;
using MatchGems.Core;
using System.Threading.Tasks;//導入核心資料

namespace MatchGems.View
{
    /// <summary>
    /// 單顆寶石的畫面元件(強制綁定SpriteRenderer)
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class GemTile : MonoBehaviour
    {
        #region 基本參數
        [SerializeField] 
        private float _tileScale = 0.9f;
        private SpriteRenderer _spriteRenderer;
        /// <summary>
        /// 視覺渲染延遲讀取
        /// </summary>
        private SpriteRenderer SpriteRenderer => _spriteRenderer == null ? _spriteRenderer = GetComponent<SpriteRenderer>() : _spriteRenderer;

        /// <summary>
        /// [靜態]共用Sprite變數
        /// </summary>
        private static Sprite _defaultSprite;
        #endregion 基本參數

        #region 公開功能
        /// <summary>
        /// 設定寶石資料並更新視覺
        /// </summary>
        /// <param name="gemData">寶石資料</param>
        public void SetGem(GemData gemData)
        {
            SpriteRenderer.sprite = GetDefaultSprite();
            SpriteRenderer.color = GetColor(gemData);
            transform.localScale = Vector3.one * _tileScale;
        }

        /// <summary>
        /// 重設寶石物理資訊
        /// </summary>
        /// <param name="pos"></param>
        public void ResetGem(Vector3 pos, GemData gemData)
        {
            SpriteRenderer.color = GetColor(gemData.Color);
            transform.position = pos;
            transform.localScale = Vector3.one * _tileScale;
        }

        /// <summary>
        /// 將 Tile 移動到指定位置(指定時限)
        /// </summary>
        /// <param name="targetPos">目標位置(終點)</param>
        /// <param name="duration">動畫時長</param>
        /// <returns></returns>
        public async Task MoveToAsync(Vector3 targetPos, float duration)
        {
            if (duration <= 0)
            {//移動時間為0，瞬間完成任務
                transform.position = targetPos;
                return;//任務結束
            }
            //動態版本
            Vector3 startPos = transform.position;//起始位置
            float timer = 0f;//計時

            while (timer < duration)
            {//時間動畫進行中
                if (this == null) return;
                timer += Time.deltaTime;//累進 1S/FPS
                float t = Mathf.Clamp01(timer / duration);//計算運作進度%
                //座標線性插值靠攏
                transform.position = Vector3.Lerp(startPos, targetPos, t);
                await Task.Yield();//等一幀：減慢迴圈使其與FPS同步
            }
            //釘板：清理誤差值
            transform.position = targetPos;
        }
        /// <summary>
        /// 將 Tile 縮小到等同消失(指定時限)
        /// </summary>
        /// <param name="duration">動畫時長</param>
        /// <returns></returns>
        public async Task PopAsync(float duration)
        {
            if (duration <= 0)
            {//縮放時間為0，瞬間完成任務
                transform.localScale = Vector3.zero;
                return;//任務結束
            }
            //動態版本
            Vector3 startScale = transform.localScale;//起始大小
            float timer = 0f;//計時

            while (timer < duration) 
            {
                if (this == null) return;
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / duration);//計算運作進度%
                //縮放線性插值靠攏
                transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
                await Task.Yield();//等一幀：減慢迴圈使其與FPS同步
            }
            //釘板：清理誤差值
            transform.localScale = Vector3.zero;
        }
        #endregion 公開功能

        #region 私有功能
        /// <summary>
        /// 取得預設Sprite圖片(白色)
        /// </summary>
        /// <returns>Sprite圖片</returns>
        private Sprite GetDefaultSprite()
        {
            if (_defaultSprite == null)
            {
                Texture2D texture = new Texture2D(1,1);//建立最小單位的貼圖
                texture.SetPixel(0, 0, Color.white);
                texture.Apply();
                //用指定參數格式建立圖片(貼圖, 預設矩形定位&尺寸, 錨點位置正中, 一單位容納像素值)
                _defaultSprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
            }
            return _defaultSprite;
        }

        /// <summary>
        /// 取得寶石種類對應的色票
        /// </summary>
        /// <param name="gemType">寶石種類</param>
        /// <returns>對應的色票</returns>
        private Color GetColor(GemType gemType)
        {
            switch (gemType)
            {
                case GemType.Red: return Color.red;
                case GemType.Blue: return Color.blue;
                case GemType.Green: return Color.green;
                case GemType.Yellow: return Color.yellow;
                case GemType.Purple: return Color.purple;
                case GemType.Pink: return Color.pink;
            }
            return Color.white;
        }
        /// <summary>
        /// 有套用特殊能力的顏色變體(多型)
        /// </summary>
        /// <param name="gemData"></param>
        /// <returns></returns>
        private Color GetColor(GemData gemData)
        {
            switch (gemData.Power)
            {
                case GemPower.HLine:
                case GemPower.VLine:
                    return Color.Lerp(GetColor(gemData.Color), Color.white, 0.5f);

                case GemPower.Bomb: 
                    return Color.Lerp(GetColor(gemData.Color), Color.white, 0.75f);

                case GemPower.Rainbow:
                    return Color.white;

                default:
                    return GetColor(gemData.Color);
            }
        }
        #endregion 私有功能
    }
}