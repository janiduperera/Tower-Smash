using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class BaseCylinder : MonoBehaviour
{
    private Color m_MyColor;
    public Color CylinderColor
    {
        get { return m_MyColor; }
    }

    private GameState m_MyState;
    public GameState CylinderState
    {
        get { return m_MyState; }
        set { m_MyState = value; }
    }

    private Transform m_MyTransform;
    private Rigidbody m_MyRigidBody;
    private Renderer m_MyRenderer;
    private Collider m_MyCollider;
    private List<BaseCylinder> m_NeighbourCylinders = new List<BaseCylinder>();
    private bool m_IsDestroyCalled = false;

    private GameObject m_Explosion;
    private GameObject m_WaterSplash;

    [Header("Water Bounciness")]
    public float WaterLevel = -2f;
    public float BounceDamp = 0.05f;
    public float FloatHieght = 1f;
    public Vector3 BouncyCenterOffset;


    private float m_ForceFactor;
    private Vector3 m_ActionPoint;
    private Vector3 m_UpLift;


    private void Awake()
    {
        m_MyTransform = transform;
        m_MyRigidBody = GetComponent<Rigidbody>();
        m_MyRenderer = GetComponent<Renderer>();
        m_MyCollider = GetComponent<Collider>();
    }

    private void FixedUpdate()
    {
        m_ActionPoint = m_MyTransform.position + m_MyTransform.TransformDirection(BouncyCenterOffset);
        m_ForceFactor = 1 - ((m_ActionPoint.y - WaterLevel) / FloatHieght);

        if (m_ForceFactor > 0)
        {
            m_UpLift = -Physics.gravity * (m_ForceFactor - m_MyRigidBody.velocity.y * BounceDamp);
            m_MyRigidBody.AddForceAtPosition(m_UpLift, m_ActionPoint);

            if (!m_IsDestroyCalled)
            {
                WaterSplash();
                m_IsDestroyCalled = true;
                UpdateProgress();
            }
        }

        if(m_MyRigidBody.isKinematic == false && m_MyRigidBody.IsSleeping())
        {
            m_MyRigidBody.WakeUp();
        }

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (GameController.Instance.CurrentGameState != GameState.GameSet)
        {
            return;
        }

        BaseCylinder m_TempBaseCylinder = collision.gameObject.GetComponent<BaseCylinder>();

        if (m_TempBaseCylinder && m_TempBaseCylinder.CylinderColor == m_MyColor)
        {
            m_NeighbourCylinders.Add(m_TempBaseCylinder);
        }
    }

    public void SetColorForCylinder(Color _color)
    {
        m_MyColor = _color;
        m_MyRenderer.material.SetColor("_Color", _color);
        m_MyRigidBody.isKinematic = true;
        m_MyCollider.enabled = true;
        m_MyRenderer.enabled = true;
        m_IsDestroyCalled = false;
        m_NeighbourCylinders.Clear();
    }

    public void BlockThisCylinder()
    {
        m_MyRenderer.material.SetColor("_Color", Color.black);
        m_MyRigidBody.isKinematic = true;
    }

    public void UnBlockThisCylinder()
    {
        m_MyRenderer.material.SetColor("_Color", m_MyColor);
        m_MyRigidBody.isKinematic = false;
    }

    public void DestroyCylider()
    {
        if (m_IsDestroyCalled || m_MyRigidBody.isKinematic)
        {
            return;
        }

        m_IsDestroyCalled = true;
        UpdateProgress();
        StartCoroutine(CylinderDestroy());
    }

    IEnumerator CylinderDestroy()
    {
        Reset();

        foreach (BaseCylinder m_BaseCylinder in m_NeighbourCylinders)
        {
            m_BaseCylinder.DestroyCylider();
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void UpdateProgress()
    {
        if (GameController.Instance.CurrentGameState == GameState.GameMode)
        {
            ProgressView.Instance.UpdateProgressBar();
        }
    }

    private void Reset()
    {
        m_MyRenderer.enabled = false;
        m_MyCollider.enabled = false;

        // Explosion
        Explosion();
    }

    // Explosion
    private void Explosion()
    {
        m_Explosion = GameController.Instance.GetExplosionParticleFromPool();
        m_Explosion.transform.position = m_MyTransform.position;
        m_Explosion.GetComponent<Renderer>().material.SetColor("_Color", m_MyColor);
        m_Explosion.GetComponent<ParticleSystem>().Play();
        StartCoroutine(AfterTheExplosion());
    }
    IEnumerator AfterTheExplosion()
    {
        yield return new WaitForSeconds(2);
        GameController.Instance.AddExplosionParticleToThePool(m_Explosion);
        m_Explosion = null;
        gameObject.SetActive(false);
    }

    // Water Splash
    private void WaterSplash()
    {
        m_WaterSplash = GameController.Instance.GetWaterSplashParticleFromPool();
        m_WaterSplash.transform.position = new Vector3(m_MyTransform.position.x, WaterLevel, m_MyTransform.position.z);
        ParticleSystem[] m_Particles = m_WaterSplash.GetComponentsInChildren<ParticleSystem>();
        foreach(ParticleSystem m_PS in m_Particles)
        {
            m_PS.Play();
        }
        StartCoroutine(AfterWaterSplash());
    }
    IEnumerator AfterWaterSplash()
    {
        yield return new WaitForSeconds(2);
        GameController.Instance.AddWaterSplashParticleToThePool(m_WaterSplash);
        m_WaterSplash = null;
    }
}
