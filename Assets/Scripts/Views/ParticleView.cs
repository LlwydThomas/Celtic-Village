using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleView : MonoBehaviour {
    public ParticleSystem[] particleSystems;

    public void AlignParticleSystems(GridModel gridModel) {
        Debug.Log("ParticlesAlligned");
        foreach (ParticleSystem particleSystem in particleSystems) {

            var newShape = particleSystem.shape;
            newShape.position = new Vector3(0, gridModel.height, 0);
            newShape.radius = gridModel.width;
        }
    }

    public void ManageParticleSystems(int index = -1) {
        foreach (ParticleSystem particleSystem in particleSystems) {
            particleSystem.gameObject.SetActive(false);
        }
        if (index != -1) {
            Debug.Log("PARTV - Enabled weather of " + particleSystems[index].gameObject.name);
            particleSystems[index].gameObject.SetActive(true);
        }

    }
}