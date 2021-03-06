﻿using UnityEngine;
using System.Collections;

abstract public class AbstractPassiveObject : AbstractObject {

    // Event calls when collision
    void OnTriggerEnter(Collider collider) {
        if (collider.gameObject.GetComponent<Player>() != null) {
            Play(collider);
        }
    }
}
