using System.Collections.Generic;
using UnityEngine;

namespace MathSeesaw
{
    public class NumContainerPan : MonoBehaviour
    {
        public int initScore;
        public TextMesh textTotal;
        public List<ManPlace> places = new List<ManPlace>();
        public float seatSpacing = 0.6f;

        public int TotalScore { get; private set; }

        public void UpdateTotalScore(bool forceShow = false)
        {
            int total = initScore;
            bool anyHidden = false;
            foreach (var p in places)
            {
                if (p == null || !p.RuntimeAvailable)
                    continue;
                total += p.GetScore();
                if (p.ScoreHidden)
                    anyHidden = true;
            }
            TotalScore = total;
            if (textTotal != null)
                textTotal.text = (anyHidden && !forceShow) ? "?" : total.ToString();
        }

        public ManPlace GetNearestEmptyPlace(Vector3 worldPos)
        {
            ManPlace best = null;
            float bestDist = float.MaxValue;
            foreach (var p in places)
            {
                if (p == null)
                    continue;
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
                if (p != null && p.RuntimeAvailable && p.PuttedMan != null)
                    p.PuttedMan.Reveal();
            UpdateTotalScore(true);
        }

        public void ApplySeatCount(int seatCount)
        {
            int clamped = Mathf.Clamp(seatCount, 0, places.Count);
            float startX = -(clamped - 1) * seatSpacing * 0.5f;

            for (int i = 0; i < places.Count; i++)
            {
                var place = places[i];
                if (place == null)
                    continue;

                bool available = i < clamped;
                place.SetRuntimeAvailable(available);
                if (!available)
                    continue;

                float x = startX + i * seatSpacing;
                if (place.seatRenderer != null)
                {
                    place.seatRenderer.transform.localPosition = new Vector3(x, 0.12f, 0f);
                    place.seatRenderer.transform.localScale = new Vector3(0.5f, 0.055f, 0.5f);
                }
                if (place.standPoint != null)
                    place.standPoint.localPosition = new Vector3(x, 0.17f, 0f);
            }
        }
    }
}
