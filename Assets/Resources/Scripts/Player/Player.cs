﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Player : MonoBehaviour {

    [HideInInspector] public Controller m_controller;
    public GameObject m_torchlight;

    public bool m_waitForDying = false;
    public bool m_isDead = false;
    public Material m_colorMat;
    public bool m_enableInteractions;

    public int m_nbOnLight = 0;
    public int m_nbPressUseless = 0;
    public int m_nbInterraction = 0;
    public int m_nbKill = 0;
    public float m_timeAlife = 0.0f;

    public AudioSource m_torchSound;
    private bool m_prevLightInput = false;
    public Animator m_animator;
    private Quaternion m_prevLightOrientation;
    protected CharacterController m_characterController;

    [HideInInspector] public GameObject m_UI_icon;


    private void Awake()
    {
        for (int i = 0; i < this.transform.childCount; i++)
            if (this.transform.GetChild(i).transform.name == "Torch")
                m_torchlight = this.transform.GetChild(i).gameObject;
    }

    // Start & Update functions
    void Start()
    {
        m_characterController = GetComponent<CharacterController>();
    }

    public void updatePlayer()
    {
        if (this.m_isDead)
            return;

        if(m_controller.GetType() == typeof(IA))
        {
            m_controller.updateControll();
        }

        // Increase Time Alife
        m_timeAlife += Time.deltaTime;

        // Get angle of right and left stick
        Vector3 displacementVector = m_controller.getDisplacement();
        Vector2 aimVector = m_controller.getAngleTorchlight();

        // Move character
        m_characterController.Move(displacementVector * Time.deltaTime);

        // Aim
        if (aimVector.x != 0.0f || aimVector.y != 0.0f)
        {
            float lightAngle = Mathf.Atan2(aimVector.y, aimVector.x) * Mathf.Rad2Deg + 90.0f;
            this.transform.localEulerAngles = new Vector3(0.0f, lightAngle, 0.0f);

            m_prevLightOrientation = m_torchlight.transform.rotation;
        }
        else if (displacementVector.x != 0.0f || displacementVector.z != 0.0f)
        {
            float lightAngle = Mathf.Atan2(-displacementVector.z, displacementVector.x) * Mathf.Rad2Deg + 90.0f;
            this.transform.localEulerAngles = new Vector3(0.0f, lightAngle, 0.0f);

            m_prevLightOrientation = m_torchlight.transform.rotation;
        }
        else
        {
            m_torchlight.transform.rotation = m_prevLightOrientation;
        }

        // Get interact input
        if (m_enableInteractions)
        {
            if(m_controller.getLightInput())
            {
                if(!m_torchlight.GetComponent<Torchlight>().setOn())
                {
                    // ENTER si state torchlight = ON ou COOLDOWN

                    if(m_controller.getLightInput() != m_prevLightInput)
                    {
                        m_nbPressUseless += 1;
                        m_prevLightInput = m_controller.getLightInput();
                    }
                }
                else
                {
                    m_torchSound.Play();
                    m_torchlight.GetComponent<Light>().enabled = true;
                    m_prevLightInput = m_controller.getLightInput();
                    m_nbOnLight += 1;
                }
            }
            else if(m_controller.getLightInput() != m_prevLightInput)
                m_prevLightInput = m_controller.getLightInput();
        }
    
        // Animation
        if(displacementVector != Vector3.zero)
        {
            m_animator.SetBool("moving", true);
        }
        else
        {
            m_animator.SetBool("moving", false);
        }
    }
    
    void OnTriggerStay(Collider collider)
    {
        if(!m_isDead)
        {
            if(collider.gameObject.tag == "Torch")
            {
                Player collidedPlayer = collider.GetComponentInParent<Light>().GetComponentInParent<Player>();

                // Try if there is no obstable between both players
                RaycastHit hit;

                Vector3 origin = collidedPlayer.transform.position;
                Vector3 direction = (this.transform.position - origin).normalized;

                if(Physics.Raycast(origin,direction,out hit))
                {
                    if(hit.collider.gameObject == this.gameObject)
                    {
                        if(collidedPlayer.getLightOn())
                        {
                            collider.GetComponentInParent<Light>().GetComponentInParent<Player>().m_nbKill += 1;
                            this.m_waitForDying = true;
                        }
                    }
                }
            }
        }
    }


    // Dead animation and over displacements
    public void deadNow()
    {
        this.m_isDead = true;
        m_animator.SetBool("die", true);
        m_animator.SetBool("moving", false);
        m_UI_icon.GetComponent<Image>().color = new Color(1.0f,1.0f,1.0f,0.3f);
    }


    // Class functions
    public Player(Controller controller)
    {
        m_controller = controller;
    }

    public void setColor(Material newMaterial)
    {
        m_colorMat = newMaterial;
        this.GetComponentInChildren<Light>().GetComponentInChildren<MeshRenderer>().material = m_colorMat;
    }

    public bool getInteract()
    { return m_enableInteractions && m_controller.getInteractInput(); }
    public bool getLightOn()
    { return m_torchlight.GetComponent<Light>().enabled; }
}