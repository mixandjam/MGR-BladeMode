using UnityEngine;
using System.Collections;
using EzySlice;

/**
 * An example fun script to show how a shatter operation can be applied to a GameObject
 * by repeatedly and randomly slicing an object
 */
public class ShatterExample : MonoBehaviour {

    /**
     * This function will slice the provided object by the plane defined in this
     * GameObject. We use the GameObject this script is attached to define the position
     * and direction of our cutting Plane. Results are then returned to the user.
     */
    public bool ShatterObject(GameObject obj, int iterations, Material crossSectionMaterial = null) {
        if (iterations > 0) {
            GameObject[] slices = obj.SliceInstantiate(GetRandomPlane(obj.transform.position, obj.transform.localScale),
                                                       new TextureRegion(0.0f, 0.0f, 1.0f, 1.0f),
                                                       crossSectionMaterial);

            if (slices != null) {
                // shatter the shattered!
                for (int i = 0; i < slices.Length; i++) {
                    if (ShatterObject(slices[i], iterations - 1, crossSectionMaterial)) {
                        // delete the parent
                        GameObject.DestroyImmediate(slices[i]);
                    }
                }

                return true;
            }

            return ShatterObject(obj, iterations - 1, crossSectionMaterial);
        }

        return false;
    }

    /**
     * Given an offset position and an offset scale, calculate a random plane
     * which can be used to randomly slice an object
     */
    public EzySlice.Plane GetRandomPlane(Vector3 positionOffset, Vector3 scaleOffset) {
        Vector3 randomPosition = Random.insideUnitSphere;

        randomPosition += positionOffset;

        Vector3 randomDirection = Random.insideUnitSphere.normalized;

        return new EzySlice.Plane(randomPosition, randomDirection);
    }
}
