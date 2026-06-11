using System;
using System.Collections.Generic;
using UnityEngine;

namespace MathSeesaw
{
    /// <summary>
    /// 关卡数据配置
    /// </summary>
    [Serializable]
    public class LevelData
    {
        public int levelNumber;
        public int[] numbers;
        public SeesawMode seesawMode = SeesawMode.Single;
        public bool[] hideNumbers; // 哪些数字需要隐藏
        public int[] seatMultipliers; // 座位倍率，null表示都是1
        public string skinName = "default";

        public LevelData(int level, int[] nums, SeesawMode mode = SeesawMode.Single)
        {
            levelNumber = level;
            numbers = nums;
            seesawMode = mode;
        }
    }

    /// <summary>
    /// 关卡配置管理器
    /// </summary>
    [CreateAssetMenu(fileName = "LevelDatabase", menuName = "MathSeesaw/Level Database")]
    public class LevelDatabase : ScriptableObject
    {
        public List<LevelData> levels = new List<LevelData>();

        public LevelData GetLevel(int levelNumber)
        {
            foreach (var level in levels)
            {
                if (level.levelNumber == levelNumber)
                    return level;
            }
            return null;
        }

        public int GetTotalLevels() => levels.Count;

        /// <summary>
        /// 创建默认关卡数据
        /// </summary>
        public void CreateDefaultLevels()
        {
            levels.Clear();

            // 简单关卡 1-5
            levels.Add(new LevelData(1, new int[] { 1, 2, 3 }, SeesawMode.Single));
            levels.Add(new LevelData(2, new int[] { 1, 2, 3, 4 }, SeesawMode.Single));
            levels.Add(new LevelData(3, new int[] { 2, 3, 5 }, SeesawMode.Single));
            levels.Add(new LevelData(4, new int[] { 1, 2, 3, 4, 5 }, SeesawMode.Single));
            levels.Add(new LevelData(5, new int[] { 2, 4, 6, 8 }, SeesawMode.Single));

            // 中等难度 6-10
            levels.Add(new LevelData(6, new int[] { 3, 5, 7, 9 }, SeesawMode.Single));
            levels.Add(new LevelData(7, new int[] { 1, 4, 5, 6 }, SeesawMode.Single));
            levels.Add(new LevelData(8, new int[] { 2, 3, 5, 7, 11 }, SeesawMode.Single));
            levels.Add(new LevelData(9, new int[] { 1, 2, 4, 8, 16 }, SeesawMode.Single));
            levels.Add(new LevelData(10, new int[] { 3, 6, 9, 12 }, SeesawMode.Single));

            // 双跷跷板关卡 11-15
            levels.Add(new LevelData(11, new int[] { 1, 2, 3, 4, 5, 6 }, SeesawMode.Double));
            levels.Add(new LevelData(12, new int[] { 2, 4, 6, 8, 10 }, SeesawMode.Double));
            levels.Add(new LevelData(13, new int[] { 1, 3, 5, 7, 9, 11 }, SeesawMode.Double));
            levels.Add(new LevelData(14, new int[] { 2, 3, 5, 7, 11, 13 }, SeesawMode.Double));
            levels.Add(new LevelData(15, new int[] { 1, 4, 9, 16, 25 }, SeesawMode.Double));

            // 困难关卡 16-20
            levels.Add(new LevelData(16, new int[] { 5, 10, 15, 20, 25 }, SeesawMode.Single));
            levels.Add(new LevelData(17, new int[] { 3, 6, 9, 12, 15, 18 }, SeesawMode.Double));
            levels.Add(new LevelData(18, new int[] { 7, 14, 21, 28 }, SeesawMode.Single));
            levels.Add(new LevelData(19, new int[] { 2, 4, 8, 16, 32 }, SeesawMode.Double));
            levels.Add(new LevelData(20, new int[] { 1, 2, 3, 5, 8, 13 }, SeesawMode.Double));
        }
    }
}
