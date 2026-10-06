using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FireballOS
{
    public class MinesweeperGame : MonoBehaviour
    {
        public enum GameState
        {
            Ready,
            Playing,
            Won,
            Lost
        }

        [Header("Game Configuration")]
        [SerializeField] private int gridWidth = 9;
        [SerializeField] private int gridHeight = 9;
        [SerializeField] private int totalMines = 10;

        [Header("Prefab & Hierarchy References")]
        [SerializeField] private GameObject cellPrefab;
        [SerializeField] private RectTransform gridContainer;
        [SerializeField] private Button smileyButton;
        [SerializeField] private TextMeshProUGUI smileyText;
        [SerializeField] private TextMeshProUGUI mineCounterText;
        [SerializeField] private TextMeshProUGUI timerText;

        public int Width => gridWidth;
        public int Height => gridHeight;
        public int TotalMines => totalMines;
        public GameState State => gameState;

        private MinesweeperCell[,] grid;
        private GameState gameState = GameState.Ready;
        private bool isFirstClick = true;
        private int flaggedCount = 0;
        private int revealedCount = 0;
        private int elapsedSeconds = 0;
        private Coroutine timerCoroutine;
        private RectTransform contentRt;

        private string GetSmileyReady()
        {
            if (smileyText != null && smileyText.font != null && smileyText.font.HasCharacter(0x1F642))
                return "🙂";
            return "☺";
        }

        private string GetSmileyLost()
        {
            if (smileyText != null && smileyText.font != null && smileyText.font.HasCharacter(0x1F635))
                return "😵";
            return "☹";
        }

        private string GetSmileyWon()
        {
            if (smileyText != null && smileyText.font != null && smileyText.font.HasCharacter(0x1F60E))
                return "😎";
            return "☻";
        }

        private void Awake()
        {
            EnforceWindowMinSize();
            BuildUIIfNeeded();
            InitializeGrid();
            ResetGame();
            UpdateLayout();
        }

        private void Start()
        {
            EnforceWindowMinSize();
            UpdateLayout();
        }

        private void OnEnable()
        {
            EnforceWindowMinSize();
            if (smileyButton != null)
            {
                smileyButton.onClick.RemoveListener(ResetGame);
                smileyButton.onClick.AddListener(ResetGame);
            }
            UpdateLayout();
        }

        private void OnDisable()
        {
            StopTimer();
        }

        private void OnRectTransformDimensionsChange()
        {
            UpdateLayout();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            EnforceWindowMinSize();
            UpdateLayout();
        }
#endif

        public void EnforceWindowMinSize()
        {
            var resizer = GetComponentInChildren<WindowResizer>(true);
            if (resizer != null)
            {
                resizer.MinSize = new Vector2(300f, 390f);
                resizer.MaxSize = new Vector2(1920f, 1080f);
            }

            RectTransform winRt = GetComponent<RectTransform>();
            if (winRt != null)
            {
                Vector2 cur = winRt.sizeDelta;
                if (cur.x < 300f || cur.y < 390f)
                {
                    winRt.sizeDelta = new Vector2(Mathf.Max(300f, cur.x), Mathf.Max(390f, cur.y));
                }
            }
        }

        public void BuildUIIfNeeded()
        {
            Transform existingContent = transform.Find("MinesweeperContent");
            if (existingContent != null)
            {
                contentRt = existingContent as RectTransform;

                if (gridContainer == null)
                {
                    Transform gc = existingContent.Find("GridContainer");
                    if (gc != null) gridContainer = gc as RectTransform;
                }

                Transform hp = existingContent.Find("HeaderPanel");
                if (hp != null)
                {
                    RectTransform existingHeaderRt = hp as RectTransform;
                    existingHeaderRt.anchorMin = new Vector2(0f, 1f);
                    existingHeaderRt.anchorMax = new Vector2(1f, 1f);
                    existingHeaderRt.pivot = new Vector2(0.5f, 1f);
                    existingHeaderRt.anchoredPosition = new Vector2(0f, -6f);
                    existingHeaderRt.sizeDelta = new Vector2(-16f, 38f);

                    Transform mb = hp.Find("MineCounterBox");
                    if (mb != null)
                    {
                        RectTransform existingMbRt = mb as RectTransform;
                        existingMbRt.anchorMin = new Vector2(0f, 0.5f);
                        existingMbRt.anchorMax = new Vector2(0f, 0.5f);
                        existingMbRt.pivot = new Vector2(0f, 0.5f);
                        existingMbRt.anchoredPosition = new Vector2(8f, 0f);
                        existingMbRt.sizeDelta = new Vector2(56f, 28f);
                    }

                    Transform sb = hp.Find("Button_Smiley");
                    if (sb != null)
                    {
                        RectTransform existingSbRt = sb as RectTransform;
                        existingSbRt.anchorMin = new Vector2(0.5f, 0.5f);
                        existingSbRt.anchorMax = new Vector2(0.5f, 0.5f);
                        existingSbRt.pivot = new Vector2(0.5f, 0.5f);
                        existingSbRt.anchoredPosition = Vector2.zero;
                        existingSbRt.sizeDelta = new Vector2(32f, 32f);
                    }

                    Transform tb = hp.Find("TimerBox");
                    if (tb != null)
                    {
                        RectTransform existingTbRt = tb as RectTransform;
                        existingTbRt.anchorMin = new Vector2(1f, 0.5f);
                        existingTbRt.anchorMax = new Vector2(1f, 0.5f);
                        existingTbRt.pivot = new Vector2(1f, 0.5f);
                        existingTbRt.anchoredPosition = new Vector2(-8f, 0f);
                        existingTbRt.sizeDelta = new Vector2(56f, 28f);
                    }
                }

                if (smileyButton == null)
                {
                    smileyButton = existingContent.GetComponentInChildren<Button>(true);
                }

                if (smileyText == null && smileyButton != null)
                {
                    smileyText = smileyButton.GetComponentInChildren<TextMeshProUGUI>(true);
                }

                var texts = existingContent.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (var t in texts)
                {
                    if (mineCounterText == null && t.gameObject.name == "Text_MineCounter") mineCounterText = t;
                    else if (timerText == null && t.gameObject.name == "Text_Timer") timerText = t;
                    else if (smileyText == null && t.gameObject.name == "Text_Smiley") smileyText = t;
                }

                UpdateLayout();
                return;
            }

            // Create Main Content Panel
            GameObject contentGo = new GameObject("MinesweeperContent", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            contentGo.transform.SetParent(transform, false);

            contentRt = contentGo.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0f, 0f);
            contentRt.anchorMax = new Vector2(1f, 1f);
            contentRt.offsetMin = new Vector2(8f, 8f);
            contentRt.offsetMax = new Vector2(-8f, -34f); // Below TitleBar

            Image contentImg = contentGo.GetComponent<Image>();
            contentImg.color = new Color(0.76f, 0.76f, 0.76f, 1f);

            // Create Header Panel (Sunken, stretches across top)
            GameObject headerGo = new GameObject("HeaderPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            headerGo.transform.SetParent(contentGo.transform, false);

            RectTransform headerRt = headerGo.GetComponent<RectTransform>();
            headerRt.anchorMin = new Vector2(0f, 1f);
            headerRt.anchorMax = new Vector2(1f, 1f);
            headerRt.pivot = new Vector2(0.5f, 1f);
            headerRt.anchoredPosition = new Vector2(0f, -6f);
            headerRt.sizeDelta = new Vector2(-16f, 38f);

            Image headerImg = headerGo.GetComponent<Image>();
            headerImg.color = new Color(0.6f, 0.6f, 0.6f, 1f);

            // Mine Counter Digital Box (Left)
            GameObject mineBox = new GameObject("MineCounterBox", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            mineBox.transform.SetParent(headerGo.transform, false);
            RectTransform mbRt = mineBox.GetComponent<RectTransform>();
            mbRt.anchorMin = new Vector2(0f, 0.5f);
            mbRt.anchorMax = new Vector2(0f, 0.5f);
            mbRt.pivot = new Vector2(0f, 0.5f);
            mbRt.anchoredPosition = new Vector2(8f, 0f);
            mbRt.sizeDelta = new Vector2(56f, 28f);
            mineBox.GetComponent<Image>().color = Color.black;

            GameObject mineTextGo = new GameObject("Text_MineCounter", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            mineTextGo.transform.SetParent(mineBox.transform, false);
            RectTransform mtRt = mineTextGo.GetComponent<RectTransform>();
            mtRt.anchorMin = Vector2.zero;
            mtRt.anchorMax = Vector2.one;
            mtRt.sizeDelta = Vector2.zero;
            mineCounterText = mineTextGo.GetComponent<TextMeshProUGUI>();
            mineCounterText.alignment = TextAlignmentOptions.Center;
            mineCounterText.fontSize = 18;
            mineCounterText.fontStyle = FontStyles.Bold;
            mineCounterText.color = Color.red;
            mineCounterText.text = "010";

            // Smiley Button (Center)
            GameObject smileyGo = new GameObject("Button_Smiley", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            smileyGo.transform.SetParent(headerGo.transform, false);
            RectTransform smRt = smileyGo.GetComponent<RectTransform>();
            smRt.anchorMin = new Vector2(0.5f, 0.5f);
            smRt.anchorMax = new Vector2(0.5f, 0.5f);
            smRt.pivot = new Vector2(0.5f, 0.5f);
            smRt.anchoredPosition = Vector2.zero;
            smRt.sizeDelta = new Vector2(32f, 32f);
            smileyGo.GetComponent<Image>().color = new Color(0.85f, 0.85f, 0.85f, 1f);
            smileyButton = smileyGo.GetComponent<Button>();

            GameObject smileyTextGo = new GameObject("Text_Smiley", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            smileyTextGo.transform.SetParent(smileyGo.transform, false);
            RectTransform stRt = smileyTextGo.GetComponent<RectTransform>();
            stRt.anchorMin = Vector2.zero;
            stRt.anchorMax = Vector2.one;
            stRt.sizeDelta = Vector2.zero;
            smileyText = smileyTextGo.GetComponent<TextMeshProUGUI>();
            smileyText.alignment = TextAlignmentOptions.Center;
            smileyText.fontSize = 20;
            smileyText.color = Color.black;
            smileyText.text = GetSmileyReady();

            // Timer Digital Box (Right)
            GameObject timerBox = new GameObject("TimerBox", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            timerBox.transform.SetParent(headerGo.transform, false);
            RectTransform tbRt = timerBox.GetComponent<RectTransform>();
            tbRt.anchorMin = new Vector2(1f, 0.5f);
            tbRt.anchorMax = new Vector2(1f, 0.5f);
            tbRt.pivot = new Vector2(1f, 0.5f);
            tbRt.anchoredPosition = new Vector2(-8f, 0f);
            tbRt.sizeDelta = new Vector2(56f, 28f);
            timerBox.GetComponent<Image>().color = Color.black;

            GameObject timerTextGo = new GameObject("Text_Timer", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            timerTextGo.transform.SetParent(timerBox.transform, false);
            RectTransform ttRt = timerTextGo.GetComponent<RectTransform>();
            ttRt.anchorMin = Vector2.zero;
            ttRt.anchorMax = Vector2.one;
            ttRt.sizeDelta = Vector2.zero;
            timerText = timerTextGo.GetComponent<TextMeshProUGUI>();
            timerText.alignment = TextAlignmentOptions.Center;
            timerText.fontSize = 18;
            timerText.fontStyle = FontStyles.Bold;
            timerText.color = Color.red;
            timerText.text = "000";

            // Grid Container (Centered below header)
            GameObject gridGo = new GameObject("GridContainer", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(GridLayoutGroup));
            gridGo.transform.SetParent(contentGo.transform, false);
            gridContainer = gridGo.GetComponent<RectTransform>();
            gridContainer.anchorMin = new Vector2(0.5f, 0.5f);
            gridContainer.anchorMax = new Vector2(0.5f, 0.5f);
            gridContainer.pivot = new Vector2(0.5f, 0.5f);

            gridGo.GetComponent<Image>().color = new Color(0.6f, 0.6f, 0.6f, 1f);

            GridLayoutGroup gridLayout = gridGo.GetComponent<GridLayoutGroup>();
            gridLayout.cellSize = new Vector2(24f, 24f);
            gridLayout.spacing = new Vector2(2f, 2f);
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 9;
            gridLayout.childAlignment = TextAnchor.MiddleCenter;
            gridLayout.padding = new RectOffset(2, 2, 2, 2);

            SetLayerRecursively(contentGo, 5);

            UpdateLayout();
        }

        public void UpdateLayout()
        {
            if (contentRt == null)
            {
                Transform existingContent = transform.Find("MinesweeperContent");
                if (existingContent != null) contentRt = existingContent as RectTransform;
                else return;
            }

            if (gridContainer == null) return;

            float contentW = contentRt.rect.width;
            float contentH = contentRt.rect.height;

            if (contentW <= 0f || contentH <= 0f) return;

            float headerHeightWithMargin = 50f;
            float bottomMargin = 8f;
            float sideMargin = 8f;

            float availW = Mathf.Max(50f, contentW - (sideMargin * 2f));
            float availH = Mathf.Max(50f, contentH - headerHeightWithMargin - bottomMargin);

            float maxSide = Mathf.Min(availW, availH);

            float spacing = 2f;
            float totalSpacing = spacing * (gridWidth - 1);
            float cellSide = Mathf.Floor((maxSide - totalSpacing) / gridWidth);
            cellSide = Mathf.Max(24f, cellSide);

            float finalGridSide = (cellSide * gridWidth) + totalSpacing;
            gridContainer.sizeDelta = new Vector2(finalGridSide, finalGridSide);

            // Center vertically in the area below the header
            float remainingTop = (contentH * 0.5f) - headerHeightWithMargin;
            float remainingBottom = -(contentH * 0.5f) + bottomMargin;
            float centerY = (remainingTop + remainingBottom) * 0.5f;

            gridContainer.anchoredPosition = new Vector2(0f, centerY);

            GridLayoutGroup gridLayout = gridContainer.GetComponent<GridLayoutGroup>();
            if (gridLayout != null)
            {
                gridLayout.cellSize = new Vector2(cellSide, cellSide);
                gridLayout.spacing = new Vector2(spacing, spacing);
                gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                gridLayout.constraintCount = gridWidth;
                gridLayout.childAlignment = TextAnchor.MiddleCenter;
            }

            // Adjust font size on cells based on cellSide
            float fontSize = Mathf.Clamp(cellSide * 0.58f, 13f, 44f);
            if (grid != null)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    for (int y = 0; y < gridHeight; y++)
                    {
                        if (grid[x, y] != null && grid[x, y].CellText != null)
                        {
                            grid[x, y].CellText.fontSize = fontSize;
                        }
                    }
                }
            }
        }

        private static void SetLayerRecursively(GameObject go, int layer)
        {
            if (go == null) return;
            go.layer = layer;
            foreach (Transform child in go.transform)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }

        private void InitializeGrid()
        {
            grid = new MinesweeperCell[gridWidth, gridHeight];

            if (gridContainer == null) return;

            // If cells already exist in gridContainer, link them up
            if (gridContainer.childCount == gridWidth * gridHeight)
            {
                for (int i = 0; i < gridContainer.childCount; i++)
                {
                    int x = i % gridWidth;
                    int y = i / gridWidth;
                    MinesweeperCell cell = gridContainer.GetChild(i).GetComponent<MinesweeperCell>();
                    if (cell != null)
                    {
                        cell.Init(this, x, y);
                        grid[x, y] = cell;
                    }
                }
                return;
            }

            // Clean up any stale children
            for (int i = gridContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(gridContainer.GetChild(i).gameObject);
            }

            // Create cells
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    grid[x, y] = CreateCell(x, y);
                }
            }
        }

        private MinesweeperCell CreateCell(int x, int y)
        {
            GameObject cellObj;
            if (cellPrefab != null)
            {
                cellObj = Instantiate(cellPrefab, gridContainer);
            }
            else
            {
                GameObject loaded = Resources.Load<GameObject>("MinesweeperCellPrefab");
                if (loaded != null)
                {
                    cellObj = Instantiate(loaded, gridContainer);
                }
                else
                {
                    cellObj = new GameObject($"Cell_{x}_{y}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(MinesweeperCell));
                    cellObj.transform.SetParent(gridContainer, false);

                    RectTransform rt = cellObj.GetComponent<RectTransform>();
                    rt.sizeDelta = new Vector2(24f, 24f);

                    Image img = cellObj.GetComponent<Image>();
                    img.color = new Color(0.75f, 0.75f, 0.75f, 1f);

                    GameObject textGo = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                    textGo.transform.SetParent(cellObj.transform, false);

                    RectTransform textRt = textGo.GetComponent<RectTransform>();
                    textRt.anchorMin = Vector2.zero;
                    textRt.anchorMax = Vector2.one;
                    textRt.sizeDelta = Vector2.zero;

                    TextMeshProUGUI tmp = textGo.GetComponent<TextMeshProUGUI>();
                    tmp.alignment = TextAlignmentOptions.Center;
                    tmp.fontSize = 15;
                    tmp.fontStyle = FontStyles.Bold;
                    tmp.color = Color.black;
                    tmp.raycastTarget = false;
                }
            }

            cellObj.name = $"Cell_{x}_{y}";
            SetLayerRecursively(cellObj, 5);
            MinesweeperCell cell = cellObj.GetComponent<MinesweeperCell>();
            if (cell == null)
            {
                cell = cellObj.AddComponent<MinesweeperCell>();
            }

            cell.Init(this, x, y);
            return cell;
        }

        public void ResetGame()
        {
            StopTimer();
            elapsedSeconds = 0;
            isFirstClick = true;
            gameState = GameState.Ready;
            flaggedCount = 0;
            revealedCount = 0;

            if (timerText != null)
            {
                timerText.text = "000";
            }

            if (mineCounterText != null)
            {
                mineCounterText.text = totalMines.ToString("D3");
            }

            if (smileyText != null)
            {
                smileyText.text = GetSmileyReady();
            }

            if (grid == null || (gridContainer != null && gridContainer.childCount != gridWidth * gridHeight))
            {
                InitializeGrid();
            }
            else
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    for (int y = 0; y < gridHeight; y++)
                    {
                        if (grid[x, y] != null)
                        {
                            grid[x, y].ResetCell();
                        }
                    }
                }
            }
        }

        public void OnCellLeftClick(MinesweeperCell cell)
        {
            if (gameState == GameState.Won || gameState == GameState.Lost) return;
            if (cell.IsRevealed || cell.IsFlagged) return;

            if (isFirstClick)
            {
                isFirstClick = false;
                gameState = GameState.Playing;
                GenerateMines(cell.X, cell.Y);
                StartTimer();
            }

            if (cell.IsMine)
            {
                GameOver(cell);
            }
            else
            {
                if (cell.AdjacentMines == 0)
                {
                    FloodFillReveal(cell);
                }
                else
                {
                    cell.Reveal();
                    revealedCount++;
                }

                CheckWinCondition();
            }
        }

        public void OnCellRightClick(MinesweeperCell cell)
        {
            if (gameState == GameState.Won || gameState == GameState.Lost) return;
            if (cell.IsRevealed) return;

            bool newFlag = !cell.IsFlagged;
            cell.SetFlagged(newFlag);

            if (newFlag)
            {
                flaggedCount++;
            }
            else
            {
                flaggedCount--;
            }

            UpdateMineCounter();
        }

        private void UpdateMineCounter()
        {
            if (mineCounterText != null)
            {
                int remaining = totalMines - flaggedCount;
                if (remaining >= 0)
                {
                    mineCounterText.text = remaining.ToString("D3");
                }
                else
                {
                    mineCounterText.text = "-" + Mathf.Abs(remaining).ToString("D2");
                }
            }
        }

        /// <summary>
        /// Generates mines ensuring the first-clicked cell and its immediate neighbors are never mines (safe first-click).
        /// </summary>
        private void GenerateMines(int excludeX, int excludeY)
        {
            List<Vector2Int> primaryCandidates = new List<Vector2Int>();
            List<Vector2Int> secondaryCandidates = new List<Vector2Int>();

            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    int dx = Mathf.Abs(x - excludeX);
                    int dy = Mathf.Abs(y - excludeY);

                    if (dx <= 1 && dy <= 1)
                    {
                        if (x != excludeX || y != excludeY)
                        {
                            secondaryCandidates.Add(new Vector2Int(x, y));
                        }
                    }
                    else
                    {
                        primaryCandidates.Add(new Vector2Int(x, y));
                    }
                }
            }

            // Shuffle primary candidates (Fisher-Yates)
            for (int i = 0; i < primaryCandidates.Count; i++)
            {
                int rand = Random.Range(i, primaryCandidates.Count);
                Vector2Int temp = primaryCandidates[i];
                primaryCandidates[i] = primaryCandidates[rand];
                primaryCandidates[rand] = temp;
            }

            int placed = 0;
            for (int i = 0; i < primaryCandidates.Count && placed < totalMines; i++)
            {
                grid[primaryCandidates[i].x, primaryCandidates[i].y].IsMine = true;
                placed++;
            }

            // Fallback if totalMines > primary candidates
            if (placed < totalMines)
            {
                for (int i = 0; i < secondaryCandidates.Count && placed < totalMines; i++)
                {
                    grid[secondaryCandidates[i].x, secondaryCandidates[i].y].IsMine = true;
                    placed++;
                }
            }

            // Calculate adjacent mine counts for each cell
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (grid[x, y].IsMine) continue;

                    int count = 0;
                    for (int ox = -1; ox <= 1; ox++)
                    {
                        for (int oy = -1; oy <= 1; oy++)
                        {
                            if (ox == 0 && oy == 0) continue;
                            int nx = x + ox;
                            int ny = y + oy;
                            if (nx >= 0 && nx < gridWidth && ny >= 0 && ny < gridHeight)
                            {
                                if (grid[nx, ny].IsMine) count++;
                            }
                        }
                    }
                    grid[x, y].AdjacentMines = count;
                }
            }
        }

        /// <summary>
        /// Queue-based BFS flood-fill reveal for 0-neighbor cells.
        /// </summary>
        private void FloodFillReveal(MinesweeperCell startCell)
        {
            Queue<MinesweeperCell> queue = new Queue<MinesweeperCell>();

            startCell.Reveal();
            revealedCount++;
            queue.Enqueue(startCell);

            while (queue.Count > 0)
            {
                MinesweeperCell current = queue.Dequeue();

                for (int ox = -1; ox <= 1; ox++)
                {
                    for (int oy = -1; oy <= 1; oy++)
                    {
                        if (ox == 0 && oy == 0) continue;

                        int nx = current.X + ox;
                        int ny = current.Y + oy;

                        if (nx >= 0 && nx < gridWidth && ny >= 0 && ny < gridHeight)
                        {
                            MinesweeperCell neighbor = grid[nx, ny];

                            if (!neighbor.IsRevealed && !neighbor.IsFlagged && !neighbor.IsMine)
                            {
                                neighbor.Reveal();
                                revealedCount++;

                                if (neighbor.AdjacentMines == 0)
                                {
                                    queue.Enqueue(neighbor);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void CheckWinCondition()
        {
            int safeCellsTotal = (gridWidth * gridHeight) - totalMines;
            if (revealedCount >= safeCellsTotal)
            {
                gameState = GameState.Won;
                StopTimer();

                if (smileyText != null)
                {
                    smileyText.text = GetSmileyWon();
                }

                // Automatically flag all unflagged mines
                for (int x = 0; x < gridWidth; x++)
                {
                    for (int y = 0; y < gridHeight; y++)
                    {
                        if (grid[x, y].IsMine && !grid[x, y].IsFlagged)
                        {
                            grid[x, y].SetFlagged(true);
                        }
                    }
                }

                if (mineCounterText != null)
                {
                    mineCounterText.text = "000";
                }
            }
        }

        private void GameOver(MinesweeperCell hitCell)
        {
            gameState = GameState.Lost;
            StopTimer();

            if (smileyText != null)
            {
                smileyText.text = GetSmileyLost();
            }

            // Reveal clicked mine in red
            if (hitCell != null)
            {
                hitCell.Reveal(isExplodedMine: true);
            }

            // Reveal all other mines and mark incorrect flags
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    MinesweeperCell cell = grid[x, y];
                    if (cell == hitCell) continue;

                    if (cell.IsMine && !cell.IsFlagged)
                    {
                        cell.Reveal(isExplodedMine: false);
                    }
                    else if (!cell.IsMine && cell.IsFlagged)
                    {
                        cell.ShowWrongFlag();
                    }
                }
            }
        }

        private void StartTimer()
        {
            StopTimer();
            if (gameObject.activeInHierarchy)
            {
                timerCoroutine = StartCoroutine(TimerRoutine());
            }
        }

        private void StopTimer()
        {
            if (timerCoroutine != null)
            {
                StopCoroutine(timerCoroutine);
                timerCoroutine = null;
            }
        }

        private IEnumerator TimerRoutine()
        {
            while (gameState == GameState.Playing && elapsedSeconds < 999)
            {
                yield return new WaitForSeconds(1f);
                if (gameState != GameState.Playing) yield break;

                elapsedSeconds++;
                if (timerText != null)
                {
                    timerText.text = elapsedSeconds.ToString("D3");
                }
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureMinesweeperAttached()
        {
            var windows = Resources.FindObjectsOfTypeAll<Window>();
            foreach (var window in windows)
            {
                if (window.WindowTitle == "Minesweeper" || window.gameObject.name == "Window_Minesweeper")
                {
                    if (window.GetComponent<MinesweeperGame>() == null)
                    {
                        window.gameObject.AddComponent<MinesweeperGame>();
                    }
                }
            }
        }
    }
}
