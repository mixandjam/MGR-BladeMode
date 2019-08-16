using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EzySlice;

/**
 * For debugging purposes only. 
 */
public class TriangulationDebug : MonoBehaviour {

	public GameObject[] points;

	void OnDrawGizmos() {
		if (points == null || points.Length < 3) {
			return;
		}

		List<Vector3> pt = new List<Vector3>();

		for (int i = 0; i < points.Length; i++) {
			if (points[i] == null) {
				continue;
			}

			pt.Add(points[i].transform.position);

			Gizmos.DrawSphere(points[i].transform.position, 0.1f);
		}

		if (pt.Count < 3) {
			return;
		}

		List<Triangle> tri;

		// perform triangulation
		if (Triangulator.MonotoneChain(pt, Vector3.up, out tri)) {
			
			for (int i = 0; i < tri.Count; i++) {
				tri[i].OnDebugDraw(Color.yellow);
			}
		}
	}
}
