using System.Linq;
using UnityEngine;

namespace CodeParts
{
    public static class Extensions
    {
        /// <summary>
        /// 渡された点から最も近い面上の点を取得します
        /// </summary>
        /// <param name="point">ターゲット</param>
        /// <param name="mesh">面を構成する点</param>
        /// <returns></returns>
        public static Vector3 ClosestPoint(this Vector3 point, Mesh mesh)
        {
            var nearestVertex = mesh.vertices.Select((v, i) => (v, i)).Aggregate((result, current) => (result.v - point).sqrMagnitude < (current.v - point).sqrMagnitude ? current : result);
            Vector3? nearestClosestPoint = null;
            for (var i = 0; i < mesh.triangles.Length; i++)
            {
                if (i != nearestVertex.i)
                {
                    continue;
                }

                var triangleFirstIndex = i - i % 3;
                var closestPoint = ClosestPoint(
                    point,
                    new[] {
                        mesh.vertices[mesh.triangles[triangleFirstIndex]],
                        mesh.vertices[mesh.triangles[triangleFirstIndex + 1]],
                        mesh.vertices[mesh.triangles[triangleFirstIndex + 2]],
                    });

                if (!nearestClosestPoint.HasValue || (closestPoint - point).sqrMagnitude < (nearestClosestPoint.Value - point).sqrMagnitude)
                {
                    nearestClosestPoint = closestPoint;
                }
            }
            
            return nearestClosestPoint.Value;
        }
        
        /// <summary>
        /// 渡された点から最も近い面上の点を取得します
        /// </summary>
        /// <param name="point">ターゲット</param>
        /// <param name="triangles">面を構成する点</param>
        /// <returns></returns>
        public static Vector3 ClosestPoint(this Vector3 point, Vector3[] triangles)
        {
            Vector3 Delta(Vector3 point1, Vector3 point2) => point2 - point1;
            Vector3 PointAt(Vector3 point1, Vector3 point2, float t) => point1 + t * Delta(point1, point2);
            float Project(Vector3 point1, Vector3 point2, Vector3 p) => Vector3.Dot(p - point1, Delta(point1, point2)) / Delta(point1, point2).sqrMagnitude;
            bool IsAbove(Vector3 planePoint, Vector3 direction, Vector3 p) => Vector3.Dot(direction, p - planePoint) > 0;
            
            // 頂点を返す
            var uab = Project(triangles[0], triangles[1], point);
            var ubc = Project(triangles[1], triangles[2], point);
            var uca = Project(triangles[2], triangles[0], point);

            if (uca > 1 && uab < 0)
            {
                return triangles[0];
            }

            if (uab > 1 && ubc < 0)
            {
                return triangles[1];
            }

            if (ubc > 1 && uca < 0)
            {
                return triangles[2];
            }

            // 辺上の点を返す
            var triNormal = Vector3.Cross(triangles[0] - triangles[1], triangles[0] - triangles[2]).normalized;

            if (0 < uab && uab < 1  && !IsAbove(triangles[0], Vector3.Cross(triNormal, Delta(triangles[0], triangles[1])), point))
            {
                return PointAt(triangles[0], triangles[1], uab);
            }
            
            if (0 < ubc && ubc < 1  && !IsAbove(triangles[1], Vector3.Cross(triNormal, Delta(triangles[1], triangles[2])), point))
            {
                return PointAt(triangles[1], triangles[2], ubc);
            }
            
            if (0 < uca && uca < 1  && !IsAbove(triangles[2], Vector3.Cross(triNormal, Delta(triangles[2], triangles[0])), point))
            {
                return PointAt(triangles[2], triangles[0], uca);
            }

            // 面上の点を返す
            var center = (triangles[0] + triangles[1] + triangles[2]) / 3;
            return center - Vector3.ProjectOnPlane(center - point, triNormal.normalized);
        }
    }
}
