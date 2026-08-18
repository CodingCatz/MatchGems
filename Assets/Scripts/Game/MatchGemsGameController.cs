using MatchGems.Core;
using MatchGems.View;
using MatchGems.Inputs;
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MatchGems.Game
{
    /// <summary>
    /// 遊戲流程主程式(控制)
    /// </summary>
    public class MatchGemsGameController : MonoBehaviour
    {
        #region 基本參數
        [SerializeField] private BoardView _boardView;
        [SerializeField] private BoardInput _boardInput;
        [SerializeField] private int _width = 8;
        [SerializeField] private int _height = 8;
        /// <summary>
        /// 交換珠的移動時間
        /// </summary>
        [SerializeField] private float _swapAnimationDuration = 0.3f;
        /// <summary>
        /// 清除珠的時間
        /// </summary>
        [SerializeField] private float _clearAnimationDuration = 0.2f;
        /// <summary>
        /// 落下移動時間
        /// </summary>
        [SerializeField] private float _buildAnimationDuration = 0.3f;
        private readonly List<CellCoord> _moveCells = new List<CellCoord>();
        private BoardModel _boardModel;
        private GridMapper _gridMapper;
        /// <summary>
        /// 預建立的流程控制器
        /// </summary>
        private readonly BoardFlowController _boardFlowController = new BoardFlowController();
        /// <summary>
        /// 主流程是否正在忙碌：
        /// 非處於待機狀態(運算中)
        /// </summary>
        private bool _isBusy => _boardFlowController.State != BoardState.Idle;
        #endregion 基本參數

        #region 生命週期
        void Start()
        {
            CreateBoard();
            CreateMapper();
            BuildView();
            ConfigureInput();
        }
        #endregion 生命週期

        #region 私有方法
        /// <summary>
        /// 建立棋盤
        /// </summary>
        private void CreateBoard()
        {
            _boardModel = new BoardModel(_width, _height);
            //流程控制：補珠
            _boardFlowController.FillInitial(_boardModel);
        }
        /// <summary>
        /// 建立轉換器
        /// </summary>
        private void CreateMapper()
        {
            //建構Root物件座標即為原點
            _gridMapper = new GridMapper(_boardView.transform.position, _boardView.CellWorldSize);
        }
        /// <summary>
        /// 以資料驅動視覺
        /// </summary>
        private void BuildView()
        {
            _boardView.Build(_boardModel, _gridMapper);
        }
        /// <summary>
        /// 設置輸入操作
        /// </summary>
        private void ConfigureInput()
        {
            _boardInput.Configure(_gridMapper);//CellSize先走預設
            _boardInput.SwapAction = TrySwap;
        }

        /// <summary>
        /// 嘗試交換兩格的寶石資料
        /// </summary>
        /// <param name="from">起始</param>
        /// <param name="to">目標</param>
        private async void TrySwap(CellCoord from, CellCoord to)
        {
            //嘗試執行交換資料(純資料)
            if (!_boardFlowController.TrySwap(_boardModel, from, to)) return;
            //嘗試執行交換動畫(純視覺)
            await _boardView.AnimateSwapAsync(from, to, _swapAnimationDuration);

            //動畫任務結束：檢查是否為無效移動(沒配對)
            MatchResult result = _boardFlowController.FindMatches(_boardModel);

            if (!result.HasMatch)
            {//沒配到：資料換回，動畫回彈
                _boardModel.SwapGems(from, to);
                await _boardView.AnimateSwapAsync(from, to, _swapAnimationDuration);
                _boardFlowController.SetIdle();//回到待機
                return;//任務中斷
            }

            //資訊重置
            int comboCount = 0;
            _moveCells.Clear();
            _moveCells.Add(from);
            _moveCells.Add(to);

            //有配對：進入循環(進到忙碌計算)
            while (result.HasMatch)
            {
                // [多組特殊石修正] 一拍先建立整份 Plan，再交給清除與 View 逐筆處理。
                SpecialGemSpawnPlan spawnPlan =
                    _boardFlowController.CreateSpawnPlan(result, _moveCells);

                //清除資料(依組別排除特殊石的資料)
                ClearStepResult clearStepResult =
                    _boardFlowController.ClearStep(
                        _boardModel,
                        result,
                        spawnPlan,
                        out DetonationChain chain);

                comboCount++;//計算連鎖數
                await _boardView.AnimateClearAsync(clearStepResult.ClearedCoords, _clearAnimationDuration);
                
                //每一顆新生特殊石都要各自刷新外觀。
                for (int i = 0; i < spawnPlan.Count; i++)
                {
                    _boardView.RefreshGem(_boardModel, spawnPlan[i].SpawnCoord);
                }

                //特殊石引爆：獨立多層連鎖運算
                await RunDetonactionAsync(chain);


                //套用重力：落下資料
                List<TileMove> falls = _boardFlowController.ApplyGravity(_boardModel);
                await _boardView.AnimateFallAsync(_boardModel, falls, _buildAnimationDuration);

                //套用天降：填充資料
                List<TileMove> fills = _boardFlowController.ApplyFill(_boardModel);
                await _boardView.AnimateFillAsync(_boardModel, fills, _buildAnimationDuration);

                //再次檢查有無配對
                result = _boardFlowController.FindMatches(_boardModel);
            }
            //無任何天降配對後
            _boardFlowController.SetIdle();//回到待機
        }

        /// <summary>
        /// 特殊能力石清板運算
        /// </summary>
        /// <param name="chain"></param>
        /// <returns></returns>
        private async Task RunDetonactionAsync(DetonationChain chain)
        {
            while (chain.HasFuses)
            {
                ClearStepResult result = _boardFlowController.DetonactionStep(chain);

                await _boardView.AnimateClearAsync(result.ClearedCoords, _clearAnimationDuration);
            }
        }
        #endregion 私有方法

        #region 生命週期
        private void Update()
        {
            if (_isBusy) return;
            //遊戲正在執行邏輯運算，阻擋任何即時性操作
        }
        #endregion 生命週期

        [ContextMenu("強制更新所有寶石")]
        public void RefreshAllGems()
        {
            for (int y = 0; y < _boardModel.Height; y++)
                for (int x = 0; x < _boardModel.Width; x++)
                {
                    CellCoord coord = new CellCoord(x, y);
                    _boardView.RefreshGem(_boardModel, coord);
                }
        }

        [ContextMenu("強制更新所有寶石變普通")]
        public void RefreshAllGemsToNormal()
        {
            for (int y = 0; y < _boardModel.Height; y++)
                for (int x = 0; x < _boardModel.Width; x++)
                {
                    CellCoord coord = new CellCoord(x, y);
                    _boardModel.GetGem(coord).SetPower();
                    _boardView.RefreshGem(_boardModel, coord);
                }
        }

        [ContextMenu("強制更新面板")]
        public void RefreshBoard()
        {
            _boardFlowController.FillInitial(_boardModel);
            for (int y = 0; y < _boardModel.Height; y++)
                for (int x = 0; x < _boardModel.Width; x++)
                {
                    CellCoord coord = new CellCoord(x, y);
                    _boardModel.GetGem(coord).SetPower();
                    _boardView.RefreshGem(_boardModel, coord);
                }
        }

        // ===== [課堂除錯快捷：一步雙四連／五連 BEGIN] =====
        // 本區只建立固定測試盤面，不介入交換、消除、重力或特殊石生成規則。
        [ContextMenu("測試盤面/一步雙四連")]
        private void ArrangeDoubleFourMatchBoard()
        {
            ArrangeDoubleLineMatchBoard(4);
        }

        [ContextMenu("測試盤面/一步雙五連")]
        private void ArrangeDoubleFiveMatchBoard()
        {
            ArrangeDoubleLineMatchBoard(5);
        }

        private void ArrangeDoubleLineMatchBoard(int lineLength)
        {
            if (_boardModel == null || _boardView == null)
            {
                Debug.LogWarning("請先進入 Play Mode，等棋盤建立後再使用測試盤面快捷。");
                return;
            }

            if (_isBusy)
            {
                Debug.LogWarning("棋盤流程仍在運作，請等 State 回到 Idle 再排測試盤面。");
                return;
            }

            if (_boardModel.Width < lineLength || _boardModel.Height < 2)
            {
                Debug.LogWarning($"一步雙{lineLength}連至少需要 {lineLength} × 2 的棋盤。");
                return;
            }

            GemType[] gemTypes = (GemType[])System.Enum.GetValues(typeof(GemType));
            FillTestPattern(gemTypes);

            int firstRow = Mathf.Max(0, _boardModel.Height / 2 - 1);
            int secondRow = firstRow + 1;
            int firstColumn = Mathf.Max(0, (_boardModel.Width - lineLength) / 2);
            int swapColumn = firstColumn + lineLength / 2;

            //中央兩排各放一顆對方顏色；交換這兩格後，同時完成兩組連線。
            for (int offset = 0; offset < lineLength; offset++)
            {
                int x = firstColumn + offset;
                _boardModel.SetGem(x, firstRow, GemType.Red);
                _boardModel.SetGem(x, secondRow, GemType.Blue);
            }

            _boardModel.SetGem(swapColumn, firstRow, GemType.Blue);
            _boardModel.SetGem(swapColumn, secondRow, GemType.Red);

            //測試線左右放不同色，避免連線超過指定長度。
            if (firstColumn > 0)
            {
                _boardModel.SetGem(firstColumn - 1, firstRow, GemType.Green);
                _boardModel.SetGem(firstColumn - 1, secondRow, GemType.Yellow);
            }

            int afterLastColumn = firstColumn + lineLength;
            if (afterLastColumn < _boardModel.Width)
            {
                _boardModel.SetGem(afterLastColumn, firstRow, GemType.Purple);
                _boardModel.SetGem(afterLastColumn, secondRow, GemType.Pink);
            }

            CellCoord from = new CellCoord(swapColumn, firstRow);
            CellCoord to = new CellCoord(swapColumn, secondRow);
            bool presetIsValid = ValidateDoubleLinePreset(from, to, lineLength);
            RefreshAllGems();

            string result = presetIsValid ? "通過" : "失敗，請檢查盤面生成規則";
            Debug.Log($"一步雙{lineLength}連盤面已建立。交換 {from.pos} 與 {to.pos}；資料預驗證：{result}。");
        }

        private void FillTestPattern(GemType[] gemTypes)
        {
            for (int y = 0; y < _boardModel.Height; y++)
            {
                for (int x = 0; x < _boardModel.Width; x++)
                {
                    int typeIndex = (x + y * 2) % gemTypes.Length;
                    _boardModel.SetGem(x, y, gemTypes[typeIndex]);
                }
            }
        }

        private bool ValidateDoubleLinePreset(CellCoord from, CellCoord to, int expectedLength)
        {
            if (_boardFlowController.FindMatches(_boardModel).HasMatch) return false;

            _boardModel.SwapGems(from, to);
            try
            {
                MatchResult result = _boardFlowController.FindMatches(_boardModel);
                if (result.LineCount != 2) return false;

                SpecialGemSpawnPlan plan = _boardFlowController.CreateSpawnPlan(
                    result,
                    new List<CellCoord> { from, to });

                //快捷本身也要確認兩條線真的會轉成兩筆生成結果。
                if (plan.Count != 2) return false;
                for (int i = 0; i < result.LineCount; i++)
                {
                    if (result.Line[i].Length != expectedLength) return false;
                }

                return true;
            }
            finally
            {
                //預驗證結束後，盤面必須回到等待學員交換的狀態。
                _boardModel.SwapGems(from, to);
            }
        }
        // ===== [課堂除錯快捷：一步雙四連／五連 END] =====
    }
}
