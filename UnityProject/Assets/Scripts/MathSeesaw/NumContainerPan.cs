using System.Collections.Generic;
using UnityEngine;

namespace MathSeesaw
{
    public class NumContainerPan : MonoBehaviour
    {
        public int initScore;
        public TextMesh textTotal;
        public List<ManPlace> places = new List<ManPlace>();

        public int TotalScore { get; private set; }

        public void UpdateTotalScore(bool forceShow = false)
        {
            int total = initScore;
            bool anyHidden = false;
            foreach (var p in places)
            {
                total += p.GetScore();
                if (p.ScoreHidden)
                    anyHidden = true;
            }
            TotalScore = total;
            textTotal.text = (anyHidden && !forceShow) ? "?" : total.ToString();
        }

        public ManPlace GetNearestEmptyPlace(Vector3 worldPos)
        {
            ManPlace best = null;
            float bestDist = float.MaxValue;
            foreach (var p in places)
            {
                if (!p.IsEmpty) continue;
                float d = (p.standPoint.position - worldPos).sqrMagnitude;
                if (d < bestDist)
                {
                    bestDist = d;
                    best = p;
                }
            }
            return best;
        }

        public void OnGameWin()
        {
            foreach (var p in places)
                if (p.PuttedMan != null)
                    p.PuttedMan.Reveal();
            UpdateTotalScore(true);
        }
    }
}
