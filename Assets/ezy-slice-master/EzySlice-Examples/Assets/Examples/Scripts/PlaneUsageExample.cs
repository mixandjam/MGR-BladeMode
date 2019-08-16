using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EzySlice;

/**
 * This class is an example of how to setup a cutting Plane from a GameObject
 * and how to work with coordinate systems.
 * 
 * When a Place slices a Mesh, the Mesh is in local coordinates whilst the Plane
 * is in world coordinates. The first step is to bring the Plane into the coordinate system
 * of the mesh we want to slice. This script shows how to do that.
 */
public class PlaneUsageExample : MonoBehaviour {

	/**
	 * This function will slice the provided object by the plane defined in this
	 * GameObject. We use the GameObject this script is attached to define the position
	 * and direction of our cutting Plane. Results are then returned to the user.
	 */
	public SlicedHull SliceObject(GameObject obj, Material crossSectionMaterial = null) {
        // slice the provided object using the transforms of this object
        return obj.Slice(transform.position, transform.up, crossSectionMaterial);
	}

	#if UNITY_EDITOR
	/**
	 * This is for Visual debugging purposes in the editor 
	 */
	public void OnDrawGizmos() {
		EzySlice.Plane cuttingPlane = new EzySlice.Plane();

		// the plane will be set to the same coordinates as the object that this
		// script is attached to
		// NOTE -> Debug Gizmo drawing only works if we pass the transform
		cuttingPlane.Compute(transform);

		// draw gizmos for the plane
		// NOTE -> Debug Gizmo drawing is ONLY available in editor mode. Do NOT try
		// to run this in the final build or you'll get crashes (most likey)
		cuttingPlane.OnDebugDraw();
	}

	#endif
}
