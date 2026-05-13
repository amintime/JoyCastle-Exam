using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/**
界面上有三个输入框，分别对应 X,Y,Z 的值，请实现 {@link Q1.onGenerateBtnClick} 函数，生成一个 10 × 10 的可控随机矩阵，并显示到界面上，矩阵要求如下：
1. {@link COLORS} 中预定义了 5 种颜色
2. 每个点可选 5 种颜色中的 1 种
3. 按照从左到右，从上到下的顺序，依次为每个点生成颜色，(0, 0)为左上⻆点，(9, 9)为右下⻆点，(0, 9)为右上⻆点
4. 点(0, 0)随机在 5 种颜色中选取
5. 其他各点的颜色计算规则如下，设目标点坐标为(m, n）：
    a. (m, n - 1)所属颜色的概率为基准概率加 X%
    b. (m - 1, n)所属颜色的概率为基准概率加 Y%
    c. 如果(m, n - 1)和(m - 1, n)同色，则该颜色的概率为基准概率加 Z%
    d. 其他颜色平分剩下的概率
*/

public class Q1 : MonoBehaviour
{
    private static readonly Color[] COLORS = new Color[]
    {
        Color.red,
        Color.green,
        Color.blue,
        Color.yellow,
        new Color(1f, 0.5f, 0f) // Orange
    };

    // 每个格子的大小
    private const float GRID_ITEM_SIZE = 75f;

    [SerializeField]
    private InputField xInputField = null;

    [SerializeField]
    private InputField yInputField = null;

    [SerializeField]
    private InputField zInputField = null;

    // 为了后续居中输出图片，需要得到size，所以改用RectTransform
    [SerializeField]
    private RectTransform gridRootNode = null;

    [SerializeField]
    private GameObject gridItemPrefab = null;

    // 提前存储
    private Image[,] ImageList;

    private void Start()
    {
        ImageList = new Image[10, 10];
        // 居中对齐所需数据
        Vector2 gridRootSize = gridRootNode.rect.size;
        Vector2 offsetSize = new Vector2((gridRootSize.x - GRID_ITEM_SIZE * 10) / 2, (gridRootSize.y - GRID_ITEM_SIZE * 10) / 2);

        for (int m = 0; m < 10; m++)
        {
            for (int n = 0; n < 10; n++)
            {
                // 实例化 UI 格子并设置属性
                GameObject newItem = Instantiate(gridItemPrefab, gridRootNode);

                // 设置格子的颜色
                Image itemImage = newItem.GetComponent<Image>();
                if (itemImage != null)
                {
                    itemImage.color = new Color(1, 1, 1, 0);
                    ImageList[n, m] = itemImage;
                }

                // 计算位置
                float posX = n * GRID_ITEM_SIZE - gridRootSize.x / 2 + GRID_ITEM_SIZE / 2 + offsetSize.x;
                float posY = -m * GRID_ITEM_SIZE + gridRootSize.y / 2 - GRID_ITEM_SIZE / 2 - offsetSize.y;
                newItem.GetComponent<RectTransform>().anchoredPosition = new Vector2(posX, posY);
            }
        }
    }

    public void OnGenerateBtnClick()
    {
        // TODO: 请在此处开始作答

        // 解析输入，有问题则报错
        int xVal = 0, yVal = 0, zVal = 0;
        bool isSuccessX = int.TryParse(xInputField.text, out xVal);
        bool isSuccessY = int.TryParse(yInputField.text, out yVal);
        bool isSuccessZ = int.TryParse(zInputField.text, out zVal);

        if (!(isSuccessX && isSuccessY && isSuccessZ))
        {
            Debug.Log("hat 输入有误");
            return;
        }

        // 清除旧数据
        // TODO:后续优化思路，提前实例化好格子与位置，后续只需要设置颜色，防止反复实例化和销毁造成巨大的GC压力
        //foreach (Transform child in gridRootNode)
        //{
        //    Destroy(child.gameObject);
        //}

        int[,] gridColors = new int[10, 10];

        // 居中对齐所需数据
        Vector2 gridRootSize = gridRootNode.rect.size;
        Vector2 offsetSize = new Vector2((gridRootSize.x - GRID_ITEM_SIZE * 10) / 2, (gridRootSize.y - GRID_ITEM_SIZE * 10) / 2);

        for (int m = 0; m < 10; m++)
        {
            for (int n = 0; n < 10; n++)
            {
                int selectedColorIndex = 0;

                // (0, 0) 左上角点，完全随机选取
                if (m == 0 && n == 0)
                {
                    selectedColorIndex = Random.Range(0, COLORS.Length);
                }
                else
                {
                    // 其他点，根据规则计算概率权重

                    // 便于计算且避免浮点精度问题，所以都使用 int
                    int[] weights = new int[COLORS.Length];
                    int leftColor = (n > 0) ? gridColors[m, n - 1] : -1;
                    int upColor = (m > 0) ? gridColors[m - 1, n] : -1;

                    // 计算各颜色的权重
                    for (int i = 0; i < COLORS.Length; i++)
                    {
                        int weight = 100; // 基准概率

                        // [m, n - 1] 所属颜色，概率加 X
                        if (i == leftColor) weight += xVal;

                        // [m - 1, n] 所属颜色，概率加 Y
                        if (i == upColor) weight += yVal;

                        // 如果[m, n - 1] [m - 1, n]两者同色，该颜色概率再加 Z
                        if (leftColor == upColor && i == leftColor)
                        {
                            weight += zVal;
                        }

                        weights[i] = weight;
                    }

                    // 根据权重进行随机选择
                    selectedColorIndex = GetWeightedRandomIndex(weights);
                }

                // 记录当前点颜色
                gridColors[n, m] = selectedColorIndex;

                //// 实例化 UI 格子并设置属性
                //GameObject newItem = Instantiate(gridItemPrefab, gridRootNode);

                //// 设置格子的颜色
                //Image itemImage = newItem.GetComponent<Image>();
                //if (itemImage != null)
                //{
                //    ImageList[n, m].color = COLORS[selectedColorIndex];
                //}

                ImageList[n, m].color = COLORS[selectedColorIndex];
            }
        }
    }

    // 根据权重数组，得到随机颜色
    private int GetWeightedRandomIndex(int[] weights)
    {
        int totalWeight = 0;
        for (int i = 0; i < weights.Length; i++)
        {
            totalWeight += weights[i];
        }

        int randomValue = Random.Range(0, totalWeight);
        int accumulatedWeight = 0;

        for (int i = 0; i < weights.Length; i++)
        {
            accumulatedWeight += weights[i];
            if (randomValue <= accumulatedWeight)
            {
                return i;
            }
        }

        // 防止报错
        return weights.Length - 1;
    }

}
