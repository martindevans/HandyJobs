using System;
using System.Globalization;
using System.Text;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

namespace Scenes.Auction
{
    public class ProjectileStatistics
        : MonoBehaviour
    {
        public int Hits;
        public int Interceptions;
        public int BulletHits;
        public int BulletsFired;
        public float ClosestInterceptionDistance = 9999;
        public float AverageInterceptionDistance;
        public float BulletHitRate;

        public TMP_Text HitsText;
        public TMP_Text InterceptionsText;
        public TMP_Text ClosestInterceptionText;
        public TMP_Text AverageInterceptionText;
        public TMP_Text BulletStats;
        private readonly StringBuilder _builder = new();

        public void ReportHit()
        {
            Hits++;
        }

        public void ReportInterception(Vector3 position)
        {
            var dist = position.magnitude;
            ClosestInterceptionDistance = math.min(ClosestInterceptionDistance, dist);;

            if (Interceptions == 1)
                AverageInterceptionDistance = dist;
            else
                AverageInterceptionDistance += (dist - AverageInterceptionDistance) / (Interceptions + 1);

            BulletHitRate = BulletHits / (float)BulletsFired;

            Interceptions++;
        }

        public void ReportBulletInterception(Vector3 position)
        {
            ReportInterception(position);
            BulletHits++;
        }

        public void ReportBulletFired()
        {
            BulletsFired++;
        }

        public void ReportBulletMiss()
        {
        }

        private void Update()
        {
            _builder.Append("Hits: ");
            _builder.Append(Hits);
            HitsText.SetText(_builder);
            _builder.Clear();

            _builder.Append("Interceptions: ");
            _builder.Append(Interceptions);
            InterceptionsText.SetText(_builder);
            _builder.Clear();

            _builder.Append("Closest: ");
            _builder.Append(ClosestInterceptionDistance);
            ClosestInterceptionText.SetText(_builder);
            _builder.Clear();

            _builder.Append("Average: ");
            _builder.Append(AverageInterceptionDistance);
            AverageInterceptionText.SetText(_builder);
            _builder.Clear();

            Span<char> buffer = stackalloc char[32];
            BulletHitRate.TryFormat(buffer, out var written, "P", CultureInfo.InvariantCulture);

            _builder.Append("Bullet Hits: ");
            _builder.Append(buffer[..written]);
            _builder.Append(" (");
            _builder.Append(BulletHits);
            _builder.Append("/");
            _builder.Append(BulletsFired);
            _builder.Append(")");
            BulletStats.SetText(_builder);
            _builder.Clear();
        }
    }
}
