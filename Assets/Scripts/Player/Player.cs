﻿using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

    protected CharacterController m_characterController;
    [HideInInspector] public Controller m_controller;

    private bool m_interact;
    private bool m_lightOn = false;
    private Quaternion m_prevLightOrientation;

    public float m_lightOnCooldown = 3.0f;
    public float m_lightOnDuration = 1.0f;
    private float m_lightOnTime;
    public Animator m_batteryAnimator;

    public int m_nbOnLight = 0;
    public int m_nbPressUseless = 0;
    public int m_nbInterraction = 0;
    public int m_nbKill = 0;
    public float m_timeAlife = 0.0f;

    public GameObject m_batteryUI;

    private Light light;
    private Renderer cone;

    // Start & Update functions
    void Start()
    {
        m_characterController = GetComponent<CharacterController>();

        light = this.GetComponentInChildren<Light>();
        cone = this.GetComponentInChildren<Light>().GetComponentInChildren<Renderer>();

        light.enabled = false;
        cone.enabled = false;
    }

    public void updatePlayer()
    {
        // Increase Time Alife
        m_timeAlife += Time.deltaTime;

        // Get angle of right and left stick
        Vector3 displacementVector = m_controller.getDisplacement();
        Vector2 aimVector = m_controller.getAngleTorchlight();

        // Move character
        m_characterController.Move(displacementVector * Time.deltaTime);

        // Aim
        if(aimVector.x != 0.0f || aimVector.y != 0.0f)
        {
            float lightAngle = Mathf.Atan2(aimVector.y, aimVector.x) * Mathf.Rad2Deg + 90.0f;
            this.transform.localEulerAngles = new Vector3(0.0f, lightAngle, 0.0f);

            m_prevLightOrientation = light.transform.rotation;
        }
        else if (displacementVector.x != 0.0f || displacementVector.z != 0.0f)
        {
            float lightAngle = Mathf.Atan2(-displacementVector.z, displacementVector.x) * Mathf.Rad2Deg + 90.0f;
            this.transform.localEulerAngles = new Vector3(0.0f, lightAngle, 0.0f);

            m_prevLightOrientation = light.transform.rotation;
        }
        else
        {
            light.transform.rotation = m_prevLightOrientation;
        }

        // Get interact input
        m_interact = m_controller.getInteractInput();

        // Turn on the light
        if (m_controller.getLightInput())
        {
            if (!m_lightOn && ((Time.time - m_lightOnTime + m_lightOnDuration) > m_lightOnCooldown))
            {
                light.enabled = true;
                cone.enabled = true;
                m_lightOn = true;
                m_lightOnTime = Time.time;

                m_batteryUI.GetComponent<Animator>().Play("BatteryCharging");
                m_nbOnLight += 1;
            }
            else
            {
                m_nbPressUseless += 1;
            }
        }
        else if (m_lightOn && ((Time.time - m_lightOnTime) > m_lightOnDuration))
        {
            light.enabled = false;
            cone.enabled = false;
            m_lightOn = false;
        }

        //if(m_controller.getLightInput())
        //    this.GetComponentInChildren<Light>().enabled = true;
        //else
        //    this.GetComponentInChildren<Light>().enabled = false;
    }

    // Class functions
    public Player(Controller controller)
    {
        m_controller = controller;
    }

    public void setColor(Material newMaterial)
    {
        this.GetComponentInChildren<Light>().GetComponentInChildren<MeshRenderer>().material = newMaterial;
    }

    public bool getInteract()
    { return m_interact; }
    public void setInteract(bool interact)
    { interact = m_interact; }

    public bool getLightOn()
    { return m_lightOn; }
    public void setLightOn(bool lightOn)
    { lightOn = m_lightOn; }
}