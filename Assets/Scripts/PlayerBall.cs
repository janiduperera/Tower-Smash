using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBall : MonoBehaviour
{
    //Cache
    private Transform m_MyTransform;
    private Vector3 m_TargetPosition = new Vector3(3, 0, -5);
    private BaseCylinder m_TargetCylinder;
    private Collider m_MyCollider;
    private Color m_PlayerColor;

    public float firingAngle = 45.0f;
    public float gravity = 9.8f;

    private float m_Target_Distance;
    private float m_Projectile_Velocity;
    private float m_VelocityX;
    private float m_VelocityY;
    private float m_FlightDuration;

    private void Awake()
    {
        m_MyTransform = transform;
        m_MyCollider = GetComponent<Collider>();
    }

    public void ShootTheBall(BaseCylinder _choosenCylinder, Vector3 _targetPosition)
    {
        m_PlayerColor = GameController.Instance.CurrentThemeColor;
        m_MyCollider.enabled = true;
        m_TargetCylinder = _choosenCylinder;
        m_TargetPosition = _targetPosition;
        m_MyTransform.SetParent(null);
        StartCoroutine(SimulateProjectile());
    }

    IEnumerator SimulateProjectile()
    {
        m_Target_Distance = Vector3.Distance(m_MyTransform.position, m_TargetPosition);

        m_Projectile_Velocity = m_Target_Distance / (Mathf.Sin(2 * firingAngle * Mathf.Deg2Rad) / gravity);

        m_VelocityX = Mathf.Sqrt(m_Projectile_Velocity) * Mathf.Cos(firingAngle * Mathf.Deg2Rad);
        m_VelocityY = Mathf.Sqrt(m_Projectile_Velocity) * Mathf.Sin(firingAngle * Mathf.Deg2Rad);

        m_FlightDuration = m_Target_Distance / m_VelocityX;

        m_MyTransform.rotation = Quaternion.LookRotation(m_TargetPosition - m_MyTransform.position);

        float m_ElapsedTime = 0;

        while (m_ElapsedTime < m_FlightDuration)
        {
            m_MyTransform.Translate(0, (m_VelocityY - (gravity * m_ElapsedTime)) * Time.deltaTime, m_VelocityX * Time.deltaTime);

            m_ElapsedTime += Time.deltaTime;

            yield return null;
        }

        if (m_PlayerColor == m_TargetCylinder.CylinderColor)
        {
            m_TargetCylinder.DestroyCylider();
            gameObject.SetActive(false);
            GameController.Instance.AddToPlayerBallPool(gameObject);
        }
        else
        {
            StartCoroutine(FallDown());
        }

    }

    IEnumerator FallDown()
    {
        Vector3 p = GameController.Instance.GetCamera.ViewportToWorldPoint(new Vector3(0.5f, 0, 30));

        while(m_MyTransform.position.y > p.y)
        {
            m_MyTransform.Translate(Vector3.down * Time.deltaTime * gravity);
            yield return null;
        }

        gameObject.SetActive(false);
        GameController.Instance.AddToPlayerBallPool(gameObject);
    }
}
